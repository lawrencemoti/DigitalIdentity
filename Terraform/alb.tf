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