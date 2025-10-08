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