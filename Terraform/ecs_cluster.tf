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