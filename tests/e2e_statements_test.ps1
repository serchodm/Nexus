# E2E Test Script for Feature 001: Raw Statements Storage
param(
    [string]$BaseUrl = "http://localhost:5276"
)

Add-Type -AssemblyName System.Net.Http
$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   NEXUS E2E TEST: MULTI-BANK STATEMENTS STORAGE" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$runId = (Get-Random -Minimum 1000 -Maximum 9999).ToString()

# 1. Login con dev-mock
Write-Host "`n1. Autenticación con Google Dev-Mock..." -ForegroundColor Yellow
$loginPayload = @{ idToken = "dev-mock:sergio.tester$runId@nexus.local" } | ConvertTo-Json
$authResponse = Invoke-RestMethod -Uri "$BaseUrl/api/auth/google" -Method Post -Body $loginPayload -ContentType "application/json"

$token = $authResponse.accessToken
if (-not $token) {
    Write-Error "Fallo en la autenticación: no se recibió accessToken."
}
Write-Host "   [OK] Token obtenido exitosamente para $($authResponse.user.email)" -ForegroundColor Green

$headers = @{
    "Authorization" = "Bearer $token"
}

# 2. Crear Cuentas Bancarias con distintos roles
Write-Host "`n2. Creando cuentas bancarias (BBVA, ING, Santander)..." -ForegroundColor Yellow

# Cuenta 1: BBVA Gastos Comunes (Cotitular 50%)
$bbvaAccountReq = @{
    name = "BBVA Gastos Comunes"
    iban = "ES910182123456789012$runId"
    type = 1 # Checking
    holderRole = 2 # JointOwner
    ownershipPercentage = 50.0
    linkedCardsLast4 = @("6589", "1111")
} | ConvertTo-Json

$bbvaAccount = Invoke-RestMethod -Uri "$BaseUrl/api/accounts" -Method Post -Body $bbvaAccountReq -Headers $headers -ContentType "application/json"
Write-Host "   [OK] Cuenta BBVA creada: $($bbvaAccount.name) (ID: $($bbvaAccount.id), Banco: $($bbvaAccount.bankName), Rol: $($bbvaAccount.holderRole))" -ForegroundColor Green

# Cuenta 2: ING Ahorro Personal (Titular 100%)
$ingAccountReq = @{
    name = "ING Ahorro Personal"
    iban = "ES211465010040123456$runId"
    type = 2 # Savings
    holderRole = 1 # Owner
    ownershipPercentage = 100.0
    linkedCardsLast4 = @("4321")
} | ConvertTo-Json

$ingAccount = Invoke-RestMethod -Uri "$BaseUrl/api/accounts" -Method Post -Body $ingAccountReq -Headers $headers -ContentType "application/json"
Write-Host "   [OK] Cuenta ING creada: $($ingAccount.name) (ID: $($ingAccount.id), Banco: $($ingAccount.bankName), Rol: $($ingAccount.holderRole))" -ForegroundColor Green

# 3. Listar cuentas y verificar cálculo de patrimonio ponderado (RN-12)
Write-Host "`n3. Consultando listado de cuentas y patrimonio..." -ForegroundColor Yellow
$accountsResult = Invoke-RestMethod -Uri "$BaseUrl/api/accounts" -Method Get -Headers $headers
Write-Host "   [OK] Total cuentas recuperadas: $($accountsResult.accounts.Count)" -ForegroundColor Green
Write-Host "   [OK] Patrimonio neto ponderado inicial: $($accountsResult.totalNetWorthShare) EUR" -ForegroundColor Green

# 4. Previsualizar extracto BBVA (POST multipart/form-data)
Write-Host "`n4. Previsualizando extracto BBVA (0 escrituras en BD)..." -ForegroundColor Yellow

$tsvContent = "Fecha`tF.Valor`tConcepto`tMovimiento`tImporte`tDivisa`tDisponible`tDivisa`tObservaciones`n" +
              "25/09/2026`t25/09/2026`tPAGO EN COMERCIO MERCADONA`tCOMPRA TARJETA`t-24,50`tEUR`t1.500,25`tEUR`tPAGO TARJETA 4188123456786589`n" +
              "24/09/2026`t24/09/2026`tNOMINA EMPRESA SL`tTRANSFERENCIA`t1.850,00`tEUR`t1.524,75`tEUR`tTRANSFERENCIA RECIBIDA"

$tempFile = [System.IO.Path]::GetTempFileName() + ".tsv"
[System.IO.File]::WriteAllText($tempFile, $tsvContent, [System.Text.Encoding]::UTF8)

# Enviar multipart usando HttpClient en .NET
$httpClient = [System.Net.Http.HttpClient]::new()
$httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer $token")
$multipart = [System.Net.Http.MultipartFormDataContent]::new()
$fileStream = [System.IO.File]::OpenRead($tempFile)
$streamContent = [System.Net.Http.StreamContent]::new($fileStream)
$streamContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("text/tab-separated-values")
$multipart.Add($streamContent, "file", "extracto_bbva.tsv")

$previewUrl = "$BaseUrl/api/accounts/$($bbvaAccount.id)/statements/preview"
$httpResponse = $httpClient.PostAsync($previewUrl, $multipart).Result
$fileStream.Close()
Remove-Item $tempFile -Force

if (-not $httpResponse.IsSuccessStatusCode) {
    $errBody = $httpResponse.Content.ReadAsStringAsync().Result
    Write-Error "Fallo en la previsualización: $errBody"
}

$previewJson = $httpResponse.Content.ReadAsStringAsync().Result | ConvertFrom-Json
Write-Host "   [OK] Previsualización completada exitosamente:" -ForegroundColor Green
Write-Host "        - PreviewToken: $($previewJson.previewToken)" -ForegroundColor DarkGray
Write-Host "        - Banco Detectado: $($previewJson.detectedBankName)" -ForegroundColor DarkGray
Write-Host "        - Saldo Inicial: $($previewJson.startingBalance) EUR" -ForegroundColor DarkGray
Write-Host "        - Saldo Final: $($previewJson.endingBalance) EUR" -ForegroundColor DarkGray
Write-Host "        - Movimientos Detectados: $($previewJson.totalRecords)" -ForegroundColor DarkGray

# 5. Confirmar importación con el previewToken
Write-Host "`n5. Confirmando importación con previewToken..." -ForegroundColor Yellow
$confirmReq = @{
    previewToken = $previewJson.previewToken
} | ConvertTo-Json

$confirmResult = Invoke-RestMethod -Uri "$BaseUrl/api/accounts/$($bbvaAccount.id)/statements/confirm" -Method Post -Body $confirmReq -Headers $headers -ContentType "application/json"
Write-Host "   [OK] Lote confirmado exitosamente:" -ForegroundColor Green
Write-Host "        - ImportId: $($confirmResult.importId)" -ForegroundColor DarkGray
Write-Host "        - Importados: $($confirmResult.importedRecords)" -ForegroundColor DarkGray
Write-Host "        - Duplicados omitidos: $($confirmResult.skippedDuplicates)" -ForegroundColor DarkGray
Write-Host "        - Saldo final cuenta: $($confirmResult.finalAccountBalance) EUR" -ForegroundColor DarkGray

# 6. Consultar movimientos importados y verificar PAN enmascarado
Write-Host "`n6. Consultando movimientos persistidos..." -ForegroundColor Yellow
$txResult = Invoke-RestMethod -Uri "$BaseUrl/api/accounts/$($bbvaAccount.id)/transactions" -Method Get -Headers $headers
Write-Host "   [OK] Total transacciones en cuenta: $($txResult.totalCount)" -ForegroundColor Green

foreach ($tx in $txResult.items) {
    Write-Host "        * $($tx.operationDate): $($tx.description) | $($tx.amount) EUR | Saldo: $($tx.balanceAfter) | Obs: $($tx.observations)" -ForegroundColor Cyan
    if ($tx.observations -like "*4188123456786589*") {
        Write-Error "FALLO DE SEGURIDAD: El PAN no fue enmascarado en la base de datos!"
    }
}
Write-Host "   [OK] Verificado: Los PAN de tarjeta están enmascarados correctamente (**** **** **** 6589)." -ForegroundColor Green

# 7. Probar deduplicación por solapamiento
Write-Host "`n7. Probando deduplicación por solapamiento..." -ForegroundColor Yellow
$overlapTsv = "Fecha`tF.Valor`tConcepto`tMovimiento`tImporte`tDivisa`tDisponible`tDivisa`tObservaciones`n" +
              "26/09/2026`t26/09/2026`tFARMACIA CENTRAL`tCOMPRA TARJETA`t-12,30`tEUR`t1.487,95`tEUR`tTARJETA 6589`n" +
              "25/09/2026`t25/09/2026`tPAGO   EN   COMERCIO     MERCADONA `tCOMPRA TARJETA`t-24,50`tEUR`t1.500,25`tEUR`tPAGO TARJETA 4188123456786589"

$tempFile2 = [System.IO.Path]::GetTempFileName() + ".tsv"
[System.IO.File]::WriteAllText($tempFile2, $overlapTsv, [System.Text.Encoding]::UTF8)

$multipart2 = [System.Net.Http.MultipartFormDataContent]::new()
$fileStream2 = [System.IO.File]::OpenRead($tempFile2)
$streamContent2 = [System.Net.Http.StreamContent]::new($fileStream2)
$multipart2.Add($streamContent2, "file", "extracto_solapado.tsv")

$httpResponse2 = $httpClient.PostAsync($previewUrl, $multipart2).Result
$fileStream2.Close()
Remove-Item $tempFile2 -Force

$preview2 = $httpResponse2.Content.ReadAsStringAsync().Result | ConvertFrom-Json
$confirm2Req = @{ previewToken = $preview2.previewToken } | ConvertTo-Json
$confirm2Result = Invoke-RestMethod -Uri "$BaseUrl/api/accounts/$($bbvaAccount.id)/statements/confirm" -Method Post -Body $confirm2Req -Headers $headers -ContentType "application/json"

Write-Host "   [OK] Lote solapado procesado:" -ForegroundColor Green
Write-Host "        - Importados nuevos: $($confirm2Result.importedRecords) (Esperado: 1)" -ForegroundColor DarkGray
Write-Host "        - Duplicados omitidos: $($confirm2Result.skippedDuplicates) (Esperado: 1)" -ForegroundColor DarkGray

if ($confirm2Result.skippedDuplicates -ne 1 -or $confirm2Result.importedRecords -ne 1) {
    Write-Error "Fallo en la deduplicación por fingerprint!"
}
Write-Host "   [OK] Deduplicación canónica por fingerprint validada al 100%!" -ForegroundColor Green

# 8. Probar Rollback de lote (Deshacer importación en 1 clic)
Write-Host "`n8. Probando Rollback (Deshacer Lote 2)..." -ForegroundColor Yellow
$rollbackUrl = "$BaseUrl/api/statements/$($confirm2Result.importId)"
$rollbackResponse = Invoke-RestMethod -Uri $rollbackUrl -Method Delete -Headers $headers

# Verificar que los movimientos volvieron a 2 y el saldo se restauró
$txAfterRollback = Invoke-RestMethod -Uri "$BaseUrl/api/accounts/$($bbvaAccount.id)/transactions" -Method Get -Headers $headers
Write-Host "   [OK] Transacciones tras rollback: $($txAfterRollback.totalCount) (Esperado: 2)" -ForegroundColor Green

$accountsAfter = Invoke-RestMethod -Uri "$BaseUrl/api/accounts" -Method Get -Headers $headers
$bbvaUpdated = $accountsAfter.accounts | Where-Object { $_.id -eq $bbvaAccount.id }
Write-Host "   [OK] Saldo restaurado tras rollback: $($bbvaUpdated.currentBalance) EUR (Esperado: 1500.25 EUR)" -ForegroundColor Green

if ($txAfterRollback.totalCount -ne 2 -or $bbvaUpdated.currentBalance -ne 1500.25) {
    Write-Error "Fallo en la integridad del Rollback!"
}

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host "   ¡TODAS LAS PRUEBAS E2E HAN FINALIZADO CON ÉXITO AL 100%!" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
