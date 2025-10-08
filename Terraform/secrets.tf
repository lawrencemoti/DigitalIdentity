resource "aws_secretsmanager_secret" "db" {
    name = "${var.project_name}/db"
    tags = local.tags
}


resource "aws_secretsmanager_secret_version" "db" {
    secret_id = aws_secretsmanager_secret.db.id
    secret_string = jsonencode({
        username = var.db_username,
        password = var.db_password,
        engine = "mysql",
        host = aws_rds_cluster.this.endpoint,
        port = 3306,
        dbname = var.db_name
    })
}