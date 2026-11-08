terraform {
  required_version = ">= 1.6"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }

  # Mesmo bucket do OS Service, chave diferente — estados independentes.
  backend "s3" {
    bucket         = "techchallenge-tfstate" # mesmo bucket criado pelo bootstrap.sh do OS repo
    key            = "techchallenge/billing/terraform.tfstate"
    region         = "us-east-1"
    dynamodb_table = "techchallenge-tflock"
    encrypt        = true
  }
}

provider "aws" {
  region = var.aws_region

  default_tags {
    tags = {
      Project     = "TechChallenge"
      Environment = var.environment
      ManagedBy   = "Terraform"
    }
  }
}
