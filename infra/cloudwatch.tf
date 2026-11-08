resource "aws_cloudwatch_log_group" "billing_service" {
  name              = "/ecs/billing-service"
  retention_in_days = 7

  tags = { Name = "billing-service-logs" }
}
