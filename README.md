# TechChallenge.Billing — Microsserviço de Faturamento

Microsserviço responsável por orçamentos e pagamentos via Mercado Pago PIX, parte do sistema TechChallenger (oficina mecânica).

## Responsabilidades

- Criação e gestão de orçamentos por OS
- Aprovação/rejeição de orçamentos pelo cliente
- Integração com Mercado Pago SDK para geração de cobrança PIX
- Validação de webhooks via HMAC-SHA256
- Publicação de eventos de pagamento para a Saga

## Arquitetura

Clean Architecture com 5 camadas: **Domain**, **Shared**, **Application**, **Infra**, **API**.

## Banco de Dados

- **MongoDB** — banco de dados NoSQL
- Coleções: `orcamentos`, `pagamentos`
- Orçamento contém lista de itens embutida (Document Model)
- Expiração automática de orçamentos após 3 dias

## Padrão Saga — Coreografado via RabbitMQ

### Por que Coreografia?

A coreografia elimina o acoplamento: o Billing reage ao evento `os.diagnostico.registrado` sem precisar ser chamado diretamente pelo OS Service. Isso permite que ambos evoluam independentemente.

### Eventos publicados

| Exchange | Routing Key | Quando |
|----------|-------------|--------|
| `tech.saga` | `orcamento.criado` | Orçamento gerado a partir do diagnóstico |
| `tech.saga` | `pagamento.confirmado` | Webhook do Mercado Pago confirmou pagamento |
| `tech.saga` | `pagamento.falhou` | Webhook recusou pagamento ou timeout |

### Eventos consumidos

| Exchange | Routing Key | Ação |
|----------|-------------|------|
| `tech.saga` | `os.diagnostico.registrado` | Cria orçamento com itens e valor estimado |

### Compensação (Rollback)

- Pagamento recusado → publica `pagamento.falhou`; OS Service cancela a OS
- Orçamento expirado (3 dias) → status `Expirado`; cliente deve abrir nova OS
- Dead Letter Queue (`billing.saga.dlq`) para reprocessamento

## Executando localmente

```bash
# Pré-requisitos: Docker, .NET 10 SDK, conta Mercado Pago sandbox
docker compose up -d
dotnet run --project src/API
```

## Variáveis de ambiente

| Variável | Descrição |
|----------|-----------|
| `ConnectionStrings__MongoDB` | URI do MongoDB |
| `MongoDB__DatabaseName` | Nome do banco (default: `techchallenge_billing`) |
| `RabbitMQ__Host` | Host do RabbitMQ |
| `RabbitMQ__Username` | Usuário RabbitMQ |
| `RabbitMQ__Password` | Senha RabbitMQ |
| `Jwt__Key` | Chave JWT |
| `MercadoPago__AccessToken` | Access Token do Mercado Pago |
| `MercadoPago__WebhookSecret` | Secret para validação HMAC-SHA256 |

## Endpoints principais

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/orcamentos/{id}` | Buscar orçamento por ID |
| `GET` | `/orcamentos/os/{osId}` | Buscar orçamento da OS |
| `POST` | `/orcamentos/{id}/aprovar` | Aprovar orçamento |
| `POST` | `/orcamentos/{id}/rejeitar` | Rejeitar orçamento |
| `GET` | `/pagamentos/{id}` | Buscar pagamento |
| `POST` | `/webhook/mercadopago` | Webhook de notificação do MP |

## Kubernetes

Manifestos em `k8s/`: `deployment.yaml`, `service.yaml`, `hpa.yaml`.

## CI/CD

| Workflow | Trigger | Descrição |
|----------|---------|-----------|
| `deploy.yml` | push main | Build, ECR, ECS deploy |
| `infra.yml` | push main (infra/) | Terraform apply |
| `quality.yml` | push/PR main | Testes + SonarCloud |
| `k8s-deploy.yml` | manual | Deploy para EKS |

## Testes

```bash
dotnet test tests/UnitTests
```

Cobertura mínima: **80%** — validada pelo SonarCloud.

**Configuração SonarCloud**: Secret `SONAR_TOKEN`, Variables `SONAR_PROJECT_KEY` e `SONAR_ORGANIZATION`.
