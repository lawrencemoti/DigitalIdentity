variable "project_name" {
  type    = string
  default = "dgIdentity"
}

variable "region" {
  type    = string
  default = "af-south-1"
}
variable "azs" {
  type    = list(string)
  default = ["af-south-1a", "af-south-1b"]
}
variable "vpc_cidr" {
  type    = string
  default = "10.0.0.0/16"
}
variable "public_subnets" {
  type    = list(string)
  default = ["10.0.0.0/20", "10.0.16.0/20"]
}
variable "private_app_subnets" {
  type    = list(string)
  default = ["10.0.128.0/20", "10.0.144.0/20"]
}
variable "private_db_subnets" {
  type    = list(string)
  default = ["10.0.160.0/20", "10.0.176.0/20"]
}


variable "db_name" {
  type    = string
  default = "identitydb"
}
variable "db_username" {
  type    = string
  default = "identityadmin"
}
variable "db_password" {
  type      = string
  sensitive = true
  default = "!IhaveRealTrustIssues1"
}


variable "container_port" {
  type    = number
  default = 8080
}
variable "desired_count_api" {
  type    = number
  default = 2
}
variable "desired_count_worker" {
  type    = number
  default = 2
}


variable "datanamixApi_baseUrl" { 
    type = string
    default = "https://api.datanamix.com/"
}
