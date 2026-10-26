variable "env_id" {
  type = string
  description = "The environment id"
  default = "dev"
}

variable "subscription_id" {
  type = string
  description = "The Azure Subscription id"
}

variable "src_key" {
  type = string
  description = "The Infrastructure source"
  default = "terraform"
}

variable "sql_pass" {
  type = string
  description = "The PostgreSQL password"
}
