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