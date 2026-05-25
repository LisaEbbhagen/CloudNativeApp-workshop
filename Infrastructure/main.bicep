param location string = resourceGroup().location
param environmentName string = 'cae-cloudnative-dev-lisa'
param appName string = 'ca-cloudnative-api'
param acrName string = 'acrstudent${uniqueString(resourceGroup().id)}'

// RESURS 1: Log Analytics Workspace
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
  name: 'law-student-logs'
  location: location
  properties: {
    sku: {
      name: 'PerGB2018' // Standard för Azure for Students
    }
    retentionInDays: 30
  }
}

// RESURS 2: Azure Container Apps Environment
  resource containerAppEnv 'Microsoft.App/managedEnvironments@2023-05-01' = {
      name: environmentName
  location: location
  properties: {
    zoneRedundant: false // Mycket viktigt för Azure for Students
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

// RESURS 3: Azure Container Registry (ACR)
resource acr 'Microsoft.ContainerRegistry/registries@2023-07-01' = {
  name: acrName
  location: location
  sku: {
    name: 'Basic' // Mycket viktigt för Azure for Students!
  }
  properties: {
    adminUserEnabled: true // Gör det enkelt för oss att logga in tillfälligt.
  }
}

// RESURS 4: Azure Container App (.NET 9 API)
resource containerApp 'Microsoft.App/containerApps@2023-05-01' = {
  name: appName
  location: location
  // Vi sätter på SystemAssigned Managed Identity
  // För att appen ska kunna hämta secrets från Key Vault utan lösenord senare.
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    managedEnvironmentId: containerAppEnv.id
    configuration: {
      ingress: {
        external: true
        // Target port 8080.
        // Eftersom vi kör .NET 9 Rootless Containers lyssnar de automatiskt på 8080 (icke-privilegierad port), inte 80.
        targetPort: 8080
        allowInsecure: false // Tvingar HTTPS 
      }
    }
    template: {
      containers: [
        {
          name: appName
          // Vi startar med en tillfällig standard-image. Vår GitHub Actions pipeline kommer byta ut denna senare.
          image: 'mcr.microsoft.com/azuredocs/containerapps-helloworld:latest'
          env: [
            {
              name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
              value: appInsights.properties.ConnectionString // Sätter miljönvariabeln så att vår app kan skicka telemetri till Application Insights utan extra konfiguration i koden.
            }
          ]
          resources: {
            cpu: json('0.25') // Minsta möjliga för att spara student-krediter
            memory: '0.5Gi'
          }
        }
      ]
      scale: {
        minReplicas: 1 // Undviker kallstart under utveckling (kostar väldigt lite)
        maxReplicas: 3
      }
    }
  }
}
// RESURS 5: Azure Key Vault
resource keyVault 'Microsoft.KeyVault/vaults@2023-02-01' = {
  name: 'kvstudent${uniqueString(resourceGroup().id)}'
  location: location
  properties: {
    tenantId: subscription().tenantId
    sku: {
      family: 'A'
      name: 'standard'
    }
    enableRbacAuthorization: true // Vi använder moderna RBAC-roller istället för Access Policies.
  }
}
// RESURS 6: RBAC Role Assignment

// Letar upp det inbyggda ID:t för rollen "Key Vault Secrets User" i Azure.
resource kvSecretsUserRole 'Microsoft.Authorization/roleDefinitions@2022-04-01' existing = {
  scope: subscription()
  name: '4633458b-17de-408a-b874-0445c86b69e6' // Detta är det statiska ID:t för Secrets User över hela Azure.
}

// Knyter samman Container App med rollen och Key Vault.
resource roleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, containerApp.id, kvSecretsUserRole.id)
  scope: keyVault
  properties: {
    roleDefinitionId: kvSecretsUserRole.id
    principalId: containerApp.identity.principalId // Appens identitet
    principalType: 'ServicePrincipal'
  }
}
// RESURS 7: Application Insights

// Detta är tjänsten som analyserar OpenTelemetry-data (Loggar, Metrics, Traces).
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: 'appi-student-${uniqueString(resourceGroup().id)}'
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}