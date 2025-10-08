# Create VPC
resource "aws_vpc" "this" {
    cidr_block = var.vpc_cidr
    enable_dns_hostnames = true
    enable_dns_support = true
    tags = merge(local.tags, { Name = "${var.project_name}-vpc-01" })
}

# Register IGW (Internet Gateway for VPC)
resource "aws_internet_gateway" "igw" {
    vpc_id = aws_vpc.this.id
    tags = merge(local.tags, { Name = "${var.project_name}-igw" })
}

# Create Public Subnets foreach AZs (2 Availability Zones)
resource "aws_subnet" "public" {
    for_each = { for idx, cidr in var.public_subnets : idx => cidr }
    vpc_id = aws_vpc.this.id
    cidr_block = each.value
    availability_zone = var.azs[tonumber(each.key)]
    map_public_ip_on_launch = true
    tags = merge(local.tags, {
    Name = "${var.project_name}-sb-public-${each.key}",
    Tier = "public"
    })
}

# Create Private Subnets for the App foreach AZ
resource "aws_subnet" "private_app" {
    for_each = { for idx, cidr in var.private_app_subnets : idx => cidr }
    vpc_id = aws_vpc.this.id
    cidr_block = each.value
    availability_zone = var.azs[tonumber(each.key)]
    tags = merge(local.tags, {
    Name = "${var.project_name}-sb-private-app-${each.key}",
    Tier = "private-app"
    })
}

# Create Private Database Subnets for each AZ
resource "aws_subnet" "private_db" {
    for_each = { for idx, cidr in var.private_db_subnets : idx => cidr }
    vpc_id = aws_vpc.this.id
    cidr_block = each.value
    availability_zone = var.azs[tonumber(each.key)]
    tags = merge(local.tags, {
    Name = "${var.project_name}-sb-private-db-${each.key}",
    Tier = "private-db"
    })
}

# NAT per AZ for HA
resource "aws_eip" "nat" {
    for_each = aws_subnet.public
    domain = "vpc"
    tags = merge(local.tags, { Name = "${var.project_name}-eip-nat-${each.key}" })
}

resource "aws_nat_gateway" "nat" {
    for_each = aws_subnet.public
    allocation_id = aws_eip.nat[each.key].id
    subnet_id = aws_subnet.public[each.key].id
    tags = merge(local.tags, { Name = "${var.project_name}-nat-${each.key}" })
    depends_on = [aws_internet_gateway.igw]
}

# Route tables
    resource "aws_route_table" "public" {
    vpc_id = aws_vpc.this.id
    tags = merge(local.tags, { Name = "${var.project_name}-rt-public" })
}

resource "aws_route" "public_internet" {
    route_table_id = aws_route_table.public.id
    destination_cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.igw.id
}

resource "aws_route_table_association" "public_assoc" {
    for_each = aws_subnet.public
    subnet_id = each.value.id
    route_table_id = aws_route_table.public.id
}

# Private-App RTs per AZ pointing to same‑AZ NAT
resource "aws_route_table" "private_app" {
    for_each = aws_subnet.private_app
    vpc_id = aws_vpc.this.id
    tags = merge(local.tags, { Name = "${var.project_name}-rt-private-app-${each.key}" })
}

resource "aws_route" "private_app_default" {
    for_each = aws_route_table.private_app
    route_table_id = each.value.id
    destination_cidr_block = "0.0.0.0/0"
    nat_gateway_id = aws_nat_gateway.nat[each.key].id
}

resource "aws_route_table_association" "private_app_assoc" {
    for_each = aws_subnet.private_app
    subnet_id = each.value.id
    route_table_id = aws_route_table.private_app[each.key].id
}

# Private-DB RTs (no internet route)
resource "aws_route_table" "private_db" {
    vpc_id = aws_vpc.this.id
    tags = merge(local.tags, { Name = "${var.project_name}-rt-private-db" })
}


resource "aws_route_table_association" "private_db_assoc" {
    for_each = aws_subnet.private_db
    subnet_id = each.value.id
    route_table_id = aws_route_table.private_db.id
}