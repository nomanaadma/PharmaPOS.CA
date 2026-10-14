resource "azurerm_container_app" "app" {
  container_app_environment_id = azurerm_container_app_environment.app_env.id
  name                         = "pca-razor-app-${var.env_id}"
  resource_group_name          = azurerm_resource_group.myPCArg.name
  revision_mode                = "Single"

  template {
    min_replicas = 1
    max_replicas = 2
    container {
      cpu    = 0.25
      image  = "mcr.microsoft.com/k8se/quickstart:latest"
      memory = "0.5Gi"
      name   = "pca-razor-app-${var.env_id}"
    }
  }

  lifecycle {
    ignore_changes = [template[0].container[0].image]
  }

  ingress {
    allow_insecure_connections = false
    external_enabled = true
    target_port = 8080

    traffic_weight {
      percentage = 100
      label = "primary"
      latest_revision = true
    }
  }

  tags = {
    environment = var.env_id
    src = var.src_key
  }
}
