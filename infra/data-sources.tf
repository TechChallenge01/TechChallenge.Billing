# Referências aos recursos criados pelo OS Service infra (TechChallenge.OS/infra).
# Pré-requisito: aplicar o OS infra antes do Billing infra.

data "aws_vpc" "main" {
  tags = { Name = "${var.project_name}-vpc" }
}

data "aws_subnets" "public" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.main.id]
  }
  filter {
    name   = "map-public-ip-on-launch"
    values = ["true"]
  }
}

data "aws_security_group" "ecs" {
  name   = "${var.project_name}-ecs-sg"
  vpc_id = data.aws_vpc.main.id
}

data "aws_security_group" "alb" {
  name   = "${var.project_name}-alb-sg"
  vpc_id = data.aws_vpc.main.id
}

data "aws_ecs_cluster" "main" {
  cluster_name = var.project_name
}

data "aws_iam_role" "lab_role" {
  name = "LabRole"
}

data "aws_instance" "services" {
  filter {
    name   = "tag:Name"
    values = ["${var.project_name}-services"]
  }
  filter {
    name   = "instance-state-name"
    values = ["running"]
  }
}
