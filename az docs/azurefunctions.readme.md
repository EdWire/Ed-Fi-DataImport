#Scope
E2E Agent Manaul FileUpload usecase.

#Deffred Scope
FTPS
SFTP
Powershell

#Keyvalut scripts
az keyvault secret set --name "edfi-dataimport-web-azure-functions" --vault-name "eg-kv-dev-scus" --file "host.json" --expires '2014-12-30T00:00:00Z'
az keyvault secret set --name "edfi-dataimport-web-azure-functions-manager" --vault-name "eg-kv-dev-scus" --file "host.json" --expires '2014-12-30T00:00:00Z'

#Deffered Improvements:
Serilog workaround
Durable Task Monitoring
K8 Secrets

#Azure Functions Caution:
Task hub name should contain only alphanumeric characters, start with a letter, and have length between 3 and 45.
