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