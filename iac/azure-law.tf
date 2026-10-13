resource "azurerm_log_analytics_workspace" "law" {
  location            = azurerm_resource_group.myPCArg.location
  name                = "pcalaw${var.env_id}"
  resource_group_name = azurerm_resource_group.myPCArg.name
  sku                 = "PerGB2018"
  retention_in_days   = 30

  tags = {
    environment = var.env_id
    src = var.src_key
    review = "demo"
  }
}