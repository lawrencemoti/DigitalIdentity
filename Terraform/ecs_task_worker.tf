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