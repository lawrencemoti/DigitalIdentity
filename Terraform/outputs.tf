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