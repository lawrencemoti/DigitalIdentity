#Variables for the Digital Identity project
variable "project_name" {
  type    = string
  default = "dgIdentity"
}

variable "region" {
  type    = string
  default = "af-south-1"
}
variable "azs" {
  type    = list(string)
  default = ["af-south-1a", "af-south-1b"]
}
variable "vpc_cidr" {
  type    = string
  default = "10.0.0.0/16"
}
variable "public_subnets" {
  type    = list(string)
  default = ["10.0.0.0/20", "10.0.16.0/20"]
}
variable "private_app_subnets" {
  type    = list(string)
  default = ["10.0.128.0/20", "10.0.144.0/20"]
}
variable "private_db_subnets" {
  type    = list(string)
  default = ["10.0.160.0/20", "10.0.176.0/20"]
}


variable "db_name" {
  type    = string
  default = "DigitalIdentityDb"
}
variable "db_username" {
  type    = string
  default = "identityadmin"
}
variable "db_password" {
  type      = string
  sensitive = true
  default = "!IhaveRealTrustIssues1"
}


variable "container_port" {
  type    = number
  default = 8080
}
variable "desired_count_api" {
  type    = number
  default = 2
}
variable "desired_count_worker" {
  type    = number
  default = 2
}


variable "datanamixApi_baseUrl" { 
    type = string
    default = "https://api.datanamix.com/"
}

#Providers configuration
#terraform {
#    required_version = ">= 1.6.0"
#    required_providers {
#        aws = {
#            source = "hashicorp/aws"
#            version = ">= 5.50"
#        }
#    }
#}

terraform {
  required_version = ">= 0.12"
  backend "local" {}
}


provider "aws" {
region = "af-south-1"

endpoints {
    rds      = "http://localhost:4566"
    apigateway = "http://localhost:4566"
    # Add other services as needed, e.g.:
    sqs   = "http://localhost:4566"
    sns   = "http://localhost:4566"
    lambda = "http://localhost:4566"
    iam = "http://localhost:4566"
    secretsmanager = "http://localhost:4566"
    ecr = "http://localhost:4566"
    ecs = "http://localhost:4566"
    cloudwatch = "http://localhost:4566"
  }
}

#Local tags for resource identification
locals {
    tags = {
        Project = var.project_name
        Env = "dev/test"
    }
}

# Create VPC
resource "aws_vpc" "this" {
    cidr_block = var.vpc_cidr
    enable_dns_hostnames = true
    enable_dns_support = true
    tags = merge(local.tags, { Name = "${var.project_name}-vpc-01" })
}

# Register IGW (Internet Gateway for VPC)
resource "aws_internet_gateway" "igw" {
    vpc_id = aws_vpc.this.id
    tags = merge(local.tags, { Name = "${var.project_name}-igw" })
}

# Create Public Subnets foreach AZs (2 Availability Zones)
resource "aws_subnet" "public" {
    for_each = { for idx, cidr in var.public_subnets : idx => cidr }
    vpc_id = aws_vpc.this.id
    cidr_block = each.value
    availability_zone = var.azs[tonumber(each.key)]
    map_public_ip_on_launch = true
    tags = merge(local.tags, {
    Name = "${var.project_name}-sb-public-${each.key}",
    Tier = "public"
    })
}

# Create Private Subnets for the App foreach AZ
resource "aws_subnet" "private_app" {
    for_each = { for idx, cidr in var.private_app_subnets : idx => cidr }
    vpc_id = aws_vpc.this.id
    cidr_block = each.value
    availability_zone = var.azs[tonumber(each.key)]
    tags = merge(local.tags, {
    Name = "${var.project_name}-sb-private-app-${each.key}",
    Tier = "private-app"
    })
}

# Create Private Database Subnets for each AZ
resource "aws_subnet" "private_db" {
    for_each = { for idx, cidr in var.private_db_subnets : idx => cidr }
    vpc_id = aws_vpc.this.id
    cidr_block = each.value
    availability_zone = var.azs[tonumber(each.key)]
    tags = merge(local.tags, {
    Name = "${var.project_name}-sb-private-db-${each.key}",
    Tier = "private-db"
    })
}

# NAT per AZ for HA
resource "aws_eip" "nat" {
    for_each = aws_subnet.public
    domain = "vpc"
    tags = merge(local.tags, { Name = "${var.project_name}-eip-nat-${each.key}" })
}

resource "aws_nat_gateway" "nat" {
    for_each = aws_subnet.public
    allocation_id = aws_eip.nat[each.key].id
    subnet_id = aws_subnet.public[each.key].id
    tags = merge(local.tags, { Name = "${var.project_name}-nat-${each.key}" })
    depends_on = [aws_internet_gateway.igw]
}

# Route tables
    resource "aws_route_table" "public" {
    vpc_id = aws_vpc.this.id
    tags = merge(local.tags, { Name = "${var.project_name}-rt-public" })
}

resource "aws_route" "public_internet" {
    route_table_id = aws_route_table.public.id
    destination_cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.igw.id
}

resource "aws_route_table_association" "public_assoc" {
    for_each = aws_subnet.public
    subnet_id = each.value.id
    route_table_id = aws_route_table.public.id
}

# Private-App RTs per AZ pointing to same‑AZ NAT
resource "aws_route_table" "private_app" {
    for_each = aws_subnet.private_app
    vpc_id = aws_vpc.this.id
    tags = merge(local.tags, { Name = "${var.project_name}-rt-private-app-${each.key}" })
}

resource "aws_route" "private_app_default" {
    for_each = aws_route_table.private_app
    route_table_id = each.value.id
    destination_cidr_block = "0.0.0.0/0"
    nat_gateway_id = aws_nat_gateway.nat[each.key].id
}

resource "aws_route_table_association" "private_app_assoc" {
    for_each = aws_subnet.private_app
    subnet_id = each.value.id
    route_table_id = aws_route_table.private_app[each.key].id
}

# Private-DB RTs (no internet route)
resource "aws_route_table" "private_db" {
    vpc_id = aws_vpc.this.id
    tags = merge(local.tags, { Name = "${var.project_name}-rt-private-db" })
}


resource "aws_route_table_association" "private_db_assoc" {
    for_each = aws_subnet.private_db
    subnet_id = each.value.id
    route_table_id = aws_route_table.private_db.id
}

# Security Groups configuration
resource "aws_security_group" "vpclink" {
    name = "${var.project_name}-sg-vpclink"
    description = "SG for API GW VPC Link ENIs"
    vpc_id = aws_vpc.this.id
    egress { 
        from_port = 0 
        to_port = 0 
        protocol = "-1" 
        cidr_blocks = [var.vpc_cidr] 
    }
    tags = merge(local.tags, { Name = "${var.project_name}-sg-vpclink" })
}

resource "aws_security_group" "alb" {
    name = "${var.project_name}-sg-alb"
    vpc_id = aws_vpc.this.id
    ingress {
        description = "From VPC Link"
        from_port = 80
        to_port = 80
        protocol = "tcp"
        security_groups = [aws_security_group.vpclink.id]
    }
    egress { 
        from_port = 0 
        to_port = 0 
        protocol = "-1" 
        cidr_blocks = [var.vpc_cidr] 
    }
    tags = merge(local.tags, { Name = "${var.project_name}-sg-alb" })
}

resource "aws_security_group" "ecs_api" {
    name = "${var.project_name}-sg-ecs-api"
    vpc_id = aws_vpc.this.id
    ingress {
        description = "ALB to API"
        from_port = var.container_port
        to_port = var.container_port
        protocol = "tcp"
        security_groups = [aws_security_group.alb.id]
    }
    egress {
        from_port = 0
        to_port = 0
        protocol = "-1"
        cidr_blocks = ["0.0.0.0/0"]
    }
    tags = merge(local.tags, { Name = "${var.project_name}-sg-ecs-api" })
}

resource "aws_security_group" "ecs_worker" {
    name = "${var.project_name}-sg-ecs-worker"
    vpc_id = aws_vpc.this.id
    egress { 
        from_port = 0 
        to_port = 0 
        protocol = "-1" 
        cidr_blocks = ["0.0.0.0/0"] 
    }
    tags = merge(local.tags, { Name = "${var.project_name}-sg-ecs-worker" })
}

resource "aws_security_group" "db" {
    name = "${var.project_name}-sg-db"
    vpc_id = aws_vpc.this.id
    ingress {
        description = "DB from ECS API"
        from_port = 3306
        to_port = 3306
        protocol = "tcp"
        security_groups = [aws_security_group.ecs_api.id, aws_security_group.ecs_worker.id]
    }
    egress { 
        from_port = 0 
        to_port = 0 
        protocol = "-1" 
        cidr_blocks = [var.vpc_cidr] 
    }
    tags = merge(local.tags, { Name = "${var.project_name}-sg-db" })
}

# (Optional VPC Interface Endpoints for SNS/SQS)
resource "aws_vpc_endpoint" "sqs" {
    vpc_id = aws_vpc.this.id
    service_name = "com.amazonaws.${var.region}.sqs"
    vpc_endpoint_type = "Interface"
    subnet_ids = [for s in aws_subnet.private_app : s.id]
    security_group_ids = [aws_security_group.ecs_api.id]
    private_dns_enabled = true
    tags = merge(local.tags, { Name = "${var.project_name}-vpce-sqs" })
}


resource "aws_vpc_endpoint" "sns" {
    vpc_id = aws_vpc.this.id
    service_name = "com.amazonaws.${var.region}.sns"
    vpc_endpoint_type = "Interface"
    subnet_ids = [for s in aws_subnet.private_app : s.id]
    security_group_ids = [aws_security_group.ecs_api.id]
    private_dns_enabled = true
    tags = merge(local.tags, { Name = "${var.project_name}-vpce-sns" })
}

# RDS Aurora Serverless v2 Configuration
resource "aws_db_subnet_group" "this" {
    name = "${var.project_name}-db-subnets"
    subnet_ids = [for s in aws_subnet.private_db : s.id]
    tags = merge(local.tags, { Name = "${var.project_name}-db-subnets" })
}

resource "aws_rds_cluster" "this" {
    cluster_identifier = "${var.project_name}-aurora"
    engine = "aurora-mysql"
    engine_mode = "provisioned" # For serverless v2, use "provisioned"
    engine_version = "8.0.mysql_aurora.3.06.0" # adjust as needed
    database_name = var.db_name
    master_username = var.db_username
    master_password = var.db_password
    db_subnet_group_name = aws_db_subnet_group.this.name
    vpc_security_group_ids = [aws_security_group.db.id]
    enable_http_endpoint    = true  # Enables Data API
    copy_tags_to_snapshot   = true
    backtrack_window        = 0
    enabled_cloudwatch_logs_exports = ["audit", "error", "general", "slowquery"]
    deletion_protection = true
    storage_encrypted = true
    apply_immediately = true
    scaling_configuration {
        auto_pause               = false # Keep always on for serverless v2
        max_capacity             = 8  # Min vCPUs
        min_capacity             = 2  # Max vCPUs
        seconds_until_auto_pause = 300 # Not used if auto_pause is false
    }
    tags = merge(local.tags, { Name = "${var.project_name}-aurora" })
}

# Define writer instance for Aurora Serverless v2
resource "aws_rds_cluster_instance" "writer" {
    identifier = "${var.project_name}-aurora-writer"
    cluster_identifier = aws_rds_cluster.this.id
    instance_class = "db.r6g.large" # Instance class for the writer node
    engine = aws_rds_cluster.this.engine
    engine_version = aws_rds_cluster.this.engine_version
    publicly_accessible = false
    db_subnet_group_name = aws_db_subnet_group.this.name
    tags = merge(local.tags, { Role = "writer" })
}

# Define read replica for Aurora Serverless v2
resource "aws_rds_cluster_instance" "reader" {
    identifier = "${var.project_name}-aurora-reader"
    cluster_identifier = aws_rds_cluster.this.id
    instance_class = "db.r6g.large" # Instance class for the reader node
    engine = aws_rds_cluster.this.engine
    engine_version = aws_rds_cluster.this.engine_version
    publicly_accessible = false
    db_subnet_group_name = aws_db_subnet_group.this.name
    tags = merge(local.tags, { Role = "reader" })
}

# Store DB credentials in AWS Secrets Manager
resource "aws_secretsmanager_secret" "db" {
    name = "${var.project_name}/db"
    tags = local.tags
}


resource "aws_secretsmanager_secret_version" "db" {
    secret_id = aws_secretsmanager_secret.db.id
    secret_string = jsonencode({
        username = var.db_username,
        password = var.db_password,
        engine = "mysql",
        host = aws_rds_cluster.this.endpoint,
        port = 3306,
        dbname = var.db_name
    })
}

# SNS Topic and SQS Queues for IDV processing
resource "aws_sns_topic" "idv" {
name = "${var.project_name}-idv-topic"
tags = local.tags
}


resource "aws_sqs_queue" "dlq" {
name = "${var.project_name}-idv-dlq"
message_retention_seconds = 1209600
tags = local.tags
}

resource "aws_sqs_queue" "queue" {
name = "${var.project_name}-idv-queue"
visibility_timeout_seconds = 60
message_retention_seconds = 345600
receive_wait_time_seconds = 20
redrive_policy = jsonencode({
deadLetterTargetArn = aws_sqs_queue.dlq.arn,
maxReceiveCount = 5
})
tags = local.tags
}

resource "aws_sns_topic_subscription" "sub" {
topic_arn = aws_sns_topic.idv.arn
protocol = "sqs"
endpoint = aws_sqs_queue.queue.arn
raw_message_delivery = true
}

# Allow SNS to publish to SQS
data "aws_iam_policy_document" "sqs_policy" {
    statement {
        actions = ["SQS:SendMessage"]
        resources = [aws_sqs_queue.queue.arn]
        principals { 
            type = "Service" 
            identifiers = ["sns.amazonaws.com"] 
            }
        condition {
            test = "ArnEquals"
            variable = "aws:SourceArn"
            values = [aws_sns_topic.idv.arn]
        }
    }
}

resource "aws_sqs_queue_policy" "queue" {
queue_url = aws_sqs_queue.queue.id
policy = data.aws_iam_policy_document.sqs_policy.json
}

# ECR Repositories for API and Worker
resource "aws_ecr_repository" "api" {
name = "${var.project_name}-digitalIdentity-api"
image_scanning_configuration { scan_on_push = true }
tags = local.tags
}


resource "aws_ecr_repository" "worker" {
name = "${var.project_name}-digitalIdentity-worker"
image_scanning_configuration { scan_on_push = true }
tags = local.tags
}

#IAM Roles and Policies for ECS Tasks
# Execution role for ECS tasks
resource "aws_iam_role" "ecs_execution" {
    name = "${var.project_name}-ecs-exec-role"
    assume_role_policy = jsonencode({
    Version = "2012-10-17",
    Statement = [{
    Effect = "Allow",
    Principal = { Service = "ecs-tasks.amazonaws.com" },
    Action = "sts:AssumeRole"
    }]
    })
}

resource "aws_iam_role_policy_attachment" "exec_base" {
role = aws_iam_role.ecs_execution.name
policy_arn = "arn:aws:iam::aws:policy/service-role/AmazonECSTaskExecutionRolePolicy"
}

# Allow execution role to read Secrets Manager for DB string (pull at startup)
resource "aws_iam_policy" "exec_secrets" {
    name = "${var.project_name}-ecs-exec-secrets"
    policy = jsonencode({
    Version = "2012-10-17",
    Statement = [{
    Effect = "Allow",
    Action = ["secretsmanager:GetSecretValue"],
    Resource = [aws_secretsmanager_secret.db.arn]
    }]
    })
}

resource "aws_iam_role_policy_attachment" "exec_secrets_attach" {
role = aws_iam_role.ecs_execution.name
policy_arn = aws_iam_policy.exec_secrets.arn
}

# Task role for API
resource "aws_iam_role" "task_api" {
name = "${var.project_name}-ecs-task-role-api"
assume_role_policy = aws_iam_role.ecs_execution.assume_role_policy
}

resource "aws_iam_policy" "api_policy" {
    name = "${var.project_name}-api-policy"
    policy = jsonencode({
    Version = "2012-10-17",
    Statement = [
    {
    Effect: "Allow",
    Action: ["sns:Publish"],
    Resource: [aws_sns_topic.identity.arn]
    },
    {
    Effect: "Allow",
    Action: ["secretsmanager:GetSecretValue"],
    Resource: [aws_secretsmanager_secret.db.arn]
    }
    ]
    })
}

resource "aws_iam_role_policy_attachment" "api_attach" {
role = aws_iam_role.task_api.name
policy_arn = aws_iam_policy.api_policy.arn
}


# Task role for Worker
resource "aws_iam_role" "task_worker" {
name = "${var.project_name}-ecs-task-role-worker"
assume_role_policy = aws_iam_role.ecs_execution.assume_role_policy
}


resource "aws_iam_policy" "worker_policy" {
name = "${var.project_name}-worker-policy"
policy = jsonencode({
Version = "2012-10-17",
Statement = [
{
Effect: "Allow",
Action: [
"sqs:ReceiveMessage", "sqs:DeleteMessage", "sqs:GetQueueUrl", "sqs:ChangeMessageVisibility"
],
Resource: [aws_sqs_queue.queue.arn]
},
{
Effect: "Allow",
Action: ["secretsmanager:GetSecretValue"],
Resource: [aws_secretsmanager_secret.db.arn]
}
]
})
}


resource "aws_iam_role_policy_attachment" "worker_attach" {
role = aws_iam_role.task_worker.name
policy_arn = aws_iam_policy.worker_policy.arn
}

# ECS Cluster and CloudWatch Log Groups
resource "aws_ecs_cluster" "this" {
name = "${var.project_name}-cluster"
setting { 
    name = "containerInsights" 
    value = "enabled" 
    }
tags = local.tags
}


resource "aws_cloudwatch_log_group" "api" {
name = "/${var.project_name}/api"
retention_in_days = 30
}


resource "aws_cloudwatch_log_group" "worker" {
name = "/${var.project_name}/worker"
retention_in_days = 30
}

# Application Load Balancer Configuration
resource "aws_lb" "internal" {
name = "${var.project_name}-alb-int"
internal = true
load_balancer_type = "application"
subnets = [for s in aws_subnet.private_app : s.id]
security_groups = [aws_security_group.alb.id]
tags = local.tags
}

resource "aws_lb_target_group" "api" {
    name = "${var.project_name}-tg-api"
    port = var.container_port
    protocol = "HTTP"
    vpc_id = aws_vpc.this.id
    target_type = "ip"
    health_check {
    path = "/health"
    healthy_threshold = 2
    unhealthy_threshold = 2
    timeout = 5
    interval = 20
    matcher = "200-399"
    }
    tags = local.tags
}


resource "aws_lb_listener" "http" {
    load_balancer_arn = aws_lb.internal.arn
    port = 80
    protocol = "HTTP"
    default_action {
    type = "forward"
    target_group_arn = aws_lb_target_group.api.arn
    }
}

# API Gateway v2 with VPC Link to ALB
resource "aws_apigatewayv2_vpc_link" "this" {
name = "${var.project_name}-vpclink"
security_group_ids = [aws_security_group.vpclink.id]
subnet_ids = [for s in aws_subnet.private_app : s.id]
tags = local.tags
}


resource "aws_apigatewayv2_api" "http" {
name = "${var.project_name}-httpapi"
protocol_type = "HTTP"
tags = local.tags
}


# Integration to ALB Listener via VPC Link
resource "aws_apigatewayv2_integration" "alb" {
api_id = aws_apigatewayv2_api.http.id
integration_type = "HTTP_PROXY"
integration_method = "ANY"
connection_type = "VPC_LINK"
connection_id = aws_apigatewayv2_vpc_link.this.id
integration_uri = aws_lb_listener.http.arn
payload_format_version = "1.0"
}

# Routes
resource "aws_apigatewayv2_route" "identity_post" {
api_id = aws_apigatewayv2_api.http.id
route_key = "POST /identity"
target = "integrations/${aws_apigatewayv2_integration.alb.id}"
}


resource "aws_apigatewayv2_route" "identities_batch" {
api_id = aws_apigatewayv2_api.http.id
route_key = "POST /identity/batch"
target = "integrations/${aws_apigatewayv2_integration.alb.id}"
}


resource "aws_apigatewayv2_route" "identity_get" {
api_id = aws_apigatewayv2_api.http.id
route_key = "POST /identity/retrieve"
target = "integrations/${aws_apigatewayv2_integration.alb.id}"
}

resource "aws_apigatewayv2_stage" "default" {
    api_id = aws_apigatewayv2_api.http.id
    name = "$default"
    auto_deploy = true
        access_log_settings {
        destination_arn = aws_cloudwatch_log_group.api.arn
        format = jsonencode({
        requestId = "$context.requestId",
        httpMethod = "$context.httpMethod",
        path = "$context.path",
        status = "$context.status",
        ip = "$context.identity.sourceIp"
        })
    }
}

# ECS Task Definitions and Services for API and Worker
resource "aws_ecs_task_definition" "api" {
family = "${var.project_name}-api"
requires_compatibilities = ["FARGATE"]
network_mode = "awsvpc"
cpu = 512
memory = 1024
execution_role_arn = aws_iam_role.ecs_execution.arn
task_role_arn = aws_iam_role.task_api.arn


container_definitions = jsonencode([
{
name = "api",
image = "${aws_ecr_repository.api.repository_url}:latest",
essential = true,
portMappings = [{ containerPort = var.container_port, protocol = "tcp" }],
logConfiguration = {
logDriver = "awslogs",
options = {
awslogs-group = aws_cloudwatch_log_group.api.name,
awslogs-region = var.region,
awslogs-stream-prefix = "api"
}
},
environment = [
{ name = "ASPNETCORE_URLS", value = "http://0.0.0.0:${var.container_port}" },
{ name = "SNS_TOPIC_ARN", value = aws_sns_topic.identity.arn },
{ name = "DB_SECRET_ARN", value = aws_secretsmanager_secret.db.arn }
]
}
])
}

resource "aws_ecs_service" "api" {
name = "${var.project_name}-svc-api"
cluster = aws_ecs_cluster.this.id
task_definition = aws_ecs_task_definition.api.arn
desired_count = var.desired_count_api
launch_type = "FARGATE"


network_configuration {
subnets = [for s in aws_subnet.private_app : s.id]
security_groups = [aws_security_group.ecs_api.id]
assign_public_ip = false
}


load_balancer {
target_group_arn = aws_lb_target_group.api.arn
container_name = "api"
container_port = var.container_port
}


deployment_minimum_healthy_percent = 50
deployment_maximum_percent = 200


depends_on = [aws_lb_listener.http]
}

# ECS Task Definition and Service for Worker with Auto Scaling based on SQS
resource "aws_ecs_task_definition" "worker" {
family = "${var.project_name}-worker"
requires_compatibilities = ["FARGATE"]
network_mode = "awsvpc"
cpu = 512
memory = 1024
execution_role_arn = aws_iam_role.ecs_execution.arn
task_role_arn = aws_iam_role.task_worker.arn


container_definitions = jsonencode([
{
name = "worker",
image = "${aws_ecr_repository.worker.repository_url}:latest",
essential = true,
logConfiguration = {
logDriver = "awslogs",
options = {
awslogs-group = aws_cloudwatch_log_group.worker.name,
awslogs-region = var.region,
awslogs-stream-prefix = "worker"
}
},
environment = [
{ name = "SQS_QUEUE_URL", value = aws_sqs_queue.queue.id },
{ name = "DB_SECRET_ARN", value = aws_secretsmanager_secret.db.arn },
{ name = "DATANAMIX_API_BASE_URL", value = var.datanamixApi_baseUrl }
]
}
])
}

resource "aws_appautoscaling_target" "worker_sqs" {
max_capacity = 10
min_capacity = var.desired_count_worker
resource_id = "service/${aws_ecs_cluster.this.name}/${aws_ecs_service.worker.name}"
scalable_dimension = "ecs:service:DesiredCount"
service_namespace = "ecs"
}

resource "aws_appautoscaling_policy" "worker_scale_on_sqs" {
    name = "${var.project_name}-worker-queue-scaling"
    policy_type = "StepScaling"
    resource_id = aws_appautoscaling_target.worker_sqs.resource_id
    scalable_dimension = aws_appautoscaling_target.worker_sqs.scalable_dimension
    service_namespace = aws_appautoscaling_target.worker_sqs.service_namespace


    step_scaling_policy_configuration {
        adjustment_type = "ChangeInCapacity"
        cooldown = 60
        metric_aggregation_type = "Average"
        step_adjustment { 
            scaling_adjustment = 1 
            metric_interval_lower_bound = 0 
        }
    }
}

# CloudWatch metric alarm on ApproximateNumberOfMessagesVisible (not shown) should trigger policy (add if desired)
resource "aws_ecs_service" "worker" {
name = "${var.project_name}-svc-worker"
cluster = aws_ecs_cluster.this.id
task_definition = aws_ecs_task_definition.worker.arn
desired_count = var.desired_count_worker
launch_type = "FARGATE"


network_configuration {
subnets = [for s in aws_subnet.private_app : s.id]
security_groups = [aws_security_group.ecs_worker.id]
assign_public_ip = false
}
}

# Outputs
output "api_gateway_endpoint" {
value = aws_apigatewayv2_api.http.api_endpoint
}


output "aurora_writer_endpoint" {
value = aws_rds_cluster.this.endpoint
}


output "sqs_queue_url" {
value = aws_sqs_queue.queue.id
}


output "sns_topic_arn" {
value = aws_sns_topic.identity.arn
}