resource "aws_sns_topic" "idv" {
name = "${var.project_name}-idv-topic"
tags = local.tags
}


resource "aws_sqs_queue" "dlq" {
name = "${var.project_name}-idv-dlq"
message_retention_seconds = 1209600
tags = local.tags
}

resource "aws_sqs_queue" "queue" {
name = "${var.project_name}-idv-queue"
visibility_timeout_seconds = 60
message_retention_seconds = 345600
receive_wait_time_seconds = 20
redrive_policy = jsonencode({
deadLetterTargetArn = aws_sqs_queue.dlq.arn,
maxReceiveCount = 5
})
tags = local.tags
}

resource "aws_sns_topic_subscription" "sub" {
topic_arn = aws_sns_topic.idv.arn
protocol = "sqs"
endpoint = aws_sqs_queue.queue.arn
raw_message_delivery = true
}

# Allow SNS to publish to SQS
data "aws_iam_policy_document" "sqs_policy" {
    statement {
        actions = ["SQS:SendMessage"]
        resources = [aws_sqs_queue.queue.arn]
        principals { 
            type = "Service" 
            identifiers = ["sns.amazonaws.com"] 
            }
        condition {
            test = "ArnEquals"
            variable = "aws:SourceArn"
            values = [aws_sns_topic.idv.arn]
        }
    }
}

resource "aws_sqs_queue_policy" "queue" {
queue_url = aws_sqs_queue.queue.id
policy = data.aws_iam_policy_document.sqs_policy.json
}