variable "aws_region" {
  description = "Região AWS"
  type        = string
  default     = "us-east-1"
}

variable "environment" {
  description = "Ambiente (prod, staging)"
  type        = string
  default     = "prod"
}

variable "project_name" {
  description = "Deve ser idêntico ao usado no OS Service infra (usado para localizar recursos via data source)"
  type        = string
  default     = "techchallenge"
}

variable "system_user_id" {
  description = "GUID usado como idUsuario nos use cases acionados por eventos"
  type        = string
  default     = "00000000-0000-0000-0000-000000000001"
}

# ─── Secrets compartilhados (mesmos valores do OS Service) ───────────────────

variable "mongodb_root_password" {
  description = "Senha do admin do MongoDB — deve ser igual à usada no OS Service infra"
  type        = string
  sensitive   = true
}

variable "rabbitmq_username" {
  description = "Usuário do RabbitMQ"
  type        = string
  sensitive   = true
}

variable "rabbitmq_password" {
  description = "Senha do RabbitMQ"
  type        = string
  sensitive   = true
}

variable "jwt_key" {
  description = "JWT signing key — deve ser igual à usada no OS Service"
  type        = string
  sensitive   = true
}

# ─── Secrets exclusivos do Billing ───────────────────────────────────────────

variable "mercadopago_access_token" {
  description = "Mercado Pago Access Token"
  type        = string
  sensitive   = true
}

variable "mercadopago_webhook_secret" {
  description = "Mercado Pago Webhook Secret"
  type        = string
  sensitive   = true
  default     = ""
}
