# ADR 1: Säker hantering av AI-nycklar (Secret-Zero)

**Kontext:**
Vår .NET-applikation behöver anropa en extern AI-tjänst (exempelvis Azure AI Foundry eller OpenAI) och behöver därmed en API-nyckel. Att hårdkoda denna nyckel i källkoden eller CI/CD-pipelinen skapar en enorm säkerhetsrisk.

**Beslut:**
Vi väljer att lösa "Secret-Zero"-problemet genom att använda **Azure Key Vault** i kombination med **System-Assigned Managed Identity**.

**Konsekvenser:**
* **Positivt:** Applikationen får en egen identitet i Microsoft Entra ID. Vi kan hämta hemligheter i koden via `DefaultAzureCredential` utan att ha några lösenord sparade i vårt kod-repo.
* **Att tänka på:** Detta tvingar oss dock till att hantera RBAC-rättigheter ("Key Vault Secrets User") för vår Azure Container App i vår Bicep-kod.