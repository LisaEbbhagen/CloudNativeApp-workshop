param location string = resourceGroup().location
param environmentName string = 'cae-inventory-dev-lisa'
param appName string = 'ca-cloudnativeapp-api'
param acrName string = 'acrstudent${uniqueString(resourceGroup().id)}'

// RESURS 1: Log Analytics Workspace
// resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2022-10-01' = {
//   name: 'law-student-logs'
//   location: location
//   properties: {
//     sku: {
//       name: 'PerGB2018' // Standard för Azure for Students
//     }
//     retentionInDays: 30
//   }
// }

// RESURS 2: Azure Container Apps Environment
resource containerAppEnv 'Microsoft.App/managedEnvironments@2023-05-01' existing = {
  name: environmentName
  scope: resourceGroup('rg-LisaE-dev') // Eftersom vi redan har skapat miljön i ett tidigare steg, refererar vi till den som en "existing resource".
  //name: environmentName
  // location: location
  // properties: {
  //   zoneRedundant: false // Mycket viktigt för Azure for Students
  //   appLogsConfiguration: {
  //     destination: 'log-analytics'
  //     logAnalyticsConfiguration: {
  //       customerId: logAnalytics.properties.customerId
  //       sharedKey: logAnalytics.listKeys().primarySharedKey
  //     }
  //   }
  // }
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