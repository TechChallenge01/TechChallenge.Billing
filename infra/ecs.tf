resource "aws_ecs_task_definition" "billing_service" {
  family                   = "billing-service"
  network_mode             = "awsvpc"
  requires_compatibilities = ["FARGATE"]
  cpu                      = "512"
  memory                   = "1024"
  execution_role_arn       = data.aws_iam_role.lab_role.arn
  task_role_arn            = data.aws_iam_role.lab_role.arn

  container_definitions = jsonencode([{
    name      = "billing-service"
    image     = "${aws_ecr_repository.billing_service.repository_url}:latest"
    essential = true

    portMappings = [{
      containerPort = 8080
      protocol      = "tcp"
    }]

    environment = [
      { name = "ASPNETCORE_URLS",       value = "http://+:8080" },
      { name = "MongoDB__DatabaseName", value = "billing" },
      { name = "RabbitMQ__Port",        value = "5672" },
      { name = "Jwt__Issuer",           value = "TechChallenger" },
      { name = "Jwt__Audience",         value = "TechChallenger" },
      { name = "SystemUserId",          value = var.system_user_id },
      { name = "Datadog__StatsdHost",   value = "localhost" },
      { name = "Datadog__StatsdPort",   value = "8125" },
    ]

    secrets = [
      { name = "MongoDB__ConnectionString",  valueFrom = "${aws_secretsmanager_secret.billing_service.arn}:MongoDB__ConnectionString::" },
      { name = "RabbitMQ__Host",             valueFrom = "${aws_secretsmanager_secret.billing_service.arn}:RabbitMQ__Host::" },
      { name = "RabbitMQ__UserName",         valueFrom = "${aws_secretsmanager_secret.billing_service.arn}:RabbitMQ__UserName::" },
      { name = "RabbitMQ__Password",         valueFrom = "${aws_secretsmanager_secret.billing_service.arn}:RabbitMQ__Password::" },
      { name = "Jwt__Key",                   valueFrom = "${aws_secretsmanager_secret.billing_service.arn}:Jwt__Key::" },
      { name = "MercadoPago__AccessToken",   valueFrom = "${aws_secretsmanager_secret.billing_service.arn}:MercadoPago__AccessToken::" },
      { name = "MercadoPago__WebhookSecret", valueFrom = "${aws_secretsmanager_secret.billing_service.arn}:MercadoPago__WebhookSecret::" },
    ]

    logConfiguration = {
      logDriver = "awslogs"
      options = {
        "awslogs-group"         = aws_cloudwatch_log_group.billing_service.name
        "awslogs-region"        = var.aws_region
        "awslogs-stream-prefix" = "ecs"
      }
    }
  }])

  tags = { Name = "billing-service" }
}

resource "aws_ecs_service" "billing_service" {
  name            = "billing-service"
  cluster         = data.aws_ecs_cluster.main.id
  task_definition = aws_ecs_task_definition.billing_service.arn
  desired_count   = 1
  launch_type     = "FARGATE"

  network_configuration {
    subnets          = data.aws_subnets.public.ids
    security_groups  = [data.aws_security_group.ecs.id]
    assign_public_ip = true
  }

  load_balancer {
    target_group_arn = aws_lb_target_group.billing_service.arn
    container_name   = "billing-service"
    container_port   = 8080
  }

  depends_on = [aws_lb_listener.billing_service]

  lifecycle {
    ignore_changes = [task_definition]
  }
}
