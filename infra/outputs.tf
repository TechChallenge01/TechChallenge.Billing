output "billing_service_url" {
  description = "URL pública do Billing Service"
  value       = "http://${aws_lb.billing_service.dns_name}"
}

output "mercadopago_webhook_url" {
  description = "URL completa para configurar no painel do Mercado Pago"
  value       = "http://${aws_lb.billing_service.dns_name}/webhooks/mercadopago"
}

output "ecr_billing_repository_url" {
  description = "URL do repositório ECR do Billing Service"
  value       = aws_ecr_repository.billing_service.repository_url
}
