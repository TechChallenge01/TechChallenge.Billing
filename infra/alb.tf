resource "aws_lb" "billing_service" {
  name               = "${var.project_name}-billing-alb"
  internal           = false
  load_balancer_type = "application"
  security_groups    = [data.aws_security_group.alb.id]
  subnets            = data.aws_subnets.public.ids

  tags = { Name = "billing-service-alb" }
}

resource "aws_lb_target_group" "billing_service" {
  name        = "${var.project_name}-billing-tg"
  port        = 8080
  protocol    = "HTTP"
  vpc_id      = data.aws_vpc.main.id
  target_type = "ip"

  health_check {
    enabled             = true
    healthy_threshold   = 2
    unhealthy_threshold = 3
    timeout             = 5
    interval            = 30
    path                = "/health"
    matcher             = "200"
  }

  tags = { Name = "billing-service-tg" }
}

resource "aws_lb_listener" "billing_service" {
  load_balancer_arn = aws_lb.billing_service.arn
  port              = 80
  protocol          = "HTTP"

  default_action {
    type             = "forward"
    target_group_arn = aws_lb_target_group.billing_service.arn
  }
}
