resource "azurerm_resource_group" "myPCArg" {
  location = "West Europe"
  name     = "pca-rg"
  
  tags = {
    environment = var.env_id
    src = var.src_key
  }
}
