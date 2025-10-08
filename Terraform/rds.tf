resource "aws_db_subnet_group" "this" {
    name = "${var.project_name}-db-subnets"
    subnet_ids = [for s in aws_subnet.private_db : s.id]
    tags = merge(local.tags, { Name = "${var.project_name}-db-subnets" })
}

resource "aws_rds_cluster" "this" {
    cluster_identifier = "${var.project_name}-aurora"
    engine = "aurora-mysql"
    engine_mode = "provisioned" # For serverless v2, use "provisioned"
    engine_version = "8.0.mysql_aurora.3.06.0" # adjust as needed
    database_name = var.db_name
    master_username = var.db_username
    master_password = var.db_password
    db_subnet_group_name = aws_db_subnet_group.this.name
    vpc_security_group_ids = [aws_security_group.db.id]
    enable_http_endpoint    = true  # Enables Data API
    copy_tags_to_snapshot   = true
    backtrack_window        = 0
    enabled_cloudwatch_logs_exports = ["audit", "error", "general", "slowquery"]
    deletion_protection = true
    storage_encrypted = true
    apply_immediately = true
    scaling_configuration {
        auto_pause               = false # Keep always on for serverless v2
        max_capacity             = 8  # Min vCPUs
        min_capacity             = 2  # Max vCPUs
        seconds_until_auto_pause = 300 # Not used if auto_pause is false
    }
    tags = merge(local.tags, { Name = "${var.project_name}-aurora" })
}

# Define writer instance for Aurora Serverless v2
resource "aws_rds_cluster_instance" "writer" {
    identifier = "${var.project_name}-aurora-writer"
    cluster_identifier = aws_rds_cluster.this.id
    instance_class = "db.r6g.large" # Instance class for the writer node
    engine = aws_rds_cluster.this.engine
    engine_version = aws_rds_cluster.this.engine_version
    publicly_accessible = false
    db_subnet_group_name = aws_db_subnet_group.this.name
    tags = merge(local.tags, { Role = "writer" })
}

# Define read replica for Aurora Serverless v2
resource "aws_rds_cluster_instance" "reader" {
    identifier = "${var.project_name}-aurora-reader"
    cluster_identifier = aws_rds_cluster.this.id
    instance_class = "db.r6g.large" # Instance class for the reader node
    engine = aws_rds_cluster.this.engine
    engine_version = aws_rds_cluster.this.engine_version
    publicly_accessible = false
    db_subnet_group_name = aws_db_subnet_group.this.name
    tags = merge(local.tags, { Role = "reader" })
}