resource "aws_secretsmanager_secret" "billing_service" {
  name                    = "${var.project_name}/billing-service"
  recovery_window_in_days = 0

  tags = { Name = "billing-service-secrets" }
}

resource "aws_secretsmanager_secret_version" "billing_service" {
  secret_id = aws_secretsmanager_secret.billing_service.id

  secret_string = jsonencode({
    "MongoDB__ConnectionString"  = "mongodb://admin:${var.mongodb_root_password}@${data.aws_instance.services.private_ip}:27017/?authSource=admin"
    "RabbitMQ__Host"             = data.aws_instance.services.private_ip
    "RabbitMQ__UserName"         = var.rabbitmq_username
    "RabbitMQ__Password"         = var.rabbitmq_password
    "Jwt__Key"                   = var.jwt_key
    "MercadoPago__AccessToken"   = var.mercadopago_access_token
    "MercadoPago__WebhookSecret" = var.mercadopago_webhook_secret
  })
}
