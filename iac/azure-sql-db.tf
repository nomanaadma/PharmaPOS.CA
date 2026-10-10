resource "azurerm_postgresql_flexible_server" "postgre_sql" {
  name                = "pharmapos-psql-${var.env_id}a1q"
  resource_group_name = azurerm_resource_group.myPCArg.name
  location            = azurerm_resource_group.myPCArg.location

  version                = "16"
  administrator_login    = "psqladmino"
  administrator_password = var.sql_pass

  # PostgreSQL Flexible Server storage is specified in MB.
  storage_mb = 32768

  # Example SKU; verify availability and subscription quota.
  sku_name = "B_Standard_B1ms"

  tags = {
    environment = var.env_id
    src         = var.src_key
  }
}

resource "azurerm_postgresql_flexible_server_database" "db" {
  name      = "pharmapos-db"
  server_id = azurerm_postgresql_flexible_server.postgre_sql.id

  charset   = "UTF8"
  collation = "en_US.utf8"

  lifecycle {
    prevent_destroy = false
  }
}

resource "azurerm_postgresql_flexible_server_firewall_rule" "rule" {
  name      = "all_azure"
  server_id = azurerm_postgresql_flexible_server.postgre_sql.id

  # Allow connections from Azure services that connect
  # using the rule's special 0.0.0.0 setting.
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}