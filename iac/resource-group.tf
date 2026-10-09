resource "azurerm_resource_group" "myPCArg" {
  location = "East Asia"
  name     = "pca-rg"
  
  tags = {
    environment = var.env_id
    src = var.src_key
  }
}
