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