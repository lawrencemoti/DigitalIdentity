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