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