# E2E Test Script for Feature 003: Conectores de Comercio & Reconciliación Externa (Amazon & PayPal)
param(
    [string]$BaseUrl = "http://localhost:5276"
)

Add-Type -AssemblyName System.Net.Http
$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   NEXUS E2E TEST: COMMERCE CONNECTORS & RECONCILIATION" -ForegroundColor Cyan
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

# 2. Crear Cuenta Bancaria
Write-Host "`n2. Creando cuenta bancaria de prueba..." -ForegroundColor Yellow
$ibanRandom = "ES91" + (Get-Random -Minimum 1000000000 -Maximum 9999999999).ToString() + (Get-Random -Minimum 1000000000 -Maximum 9999999999).ToString()
$accountReq = @{
    name = "BBVA Commerce Test $runId"
    iban = $ibanRandom
    currency = "EUR"
    bankEntity = "BBVA"
} | ConvertTo-Json

$account = Invoke-RestMethod -Uri "$BaseUrl/api/accounts" -Method Post -Body $accountReq -Headers $headers -ContentType "application/json"
$accountId = $account.id
Write-Host "   [OK] Cuenta bancaria creada: $($account.name) (ID: $accountId)" -ForegroundColor Green

# 3. Subir Extracto Bancario con movimientos opacos de Amazon y PayPal
Write-Host "`n3. Subiendo extracto bancario con compras opacas..." -ForegroundColor Yellow
$today = [DateTime]::UtcNow.ToString("dd/MM/yyyy")
$yesterday = [DateTime]::UtcNow.AddDays(-1).ToString("dd/MM/yyyy")
$twoDaysAgo = [DateTime]::UtcNow.AddDays(-2).ToString("dd/MM/yyyy")

$bbvaCsv = @"
Fecha;Fecha valor;Concepto;Importe;Divisa;Saldo;Observaciones
$today;$today;COMPRA EN AMAZON ES MADRID;-45,49;EUR;1454,51;Tarjeta 1234
$yesterday;$yesterday;RECIBO PAYPAL *SPOTIFY LUXEMBOURG;-12,99;EUR;1500,00;
$twoDaysAgo;$twoDaysAgo;TRANSFERENCIA NOMINA EMPRESA;1512,99;EUR;1512,99;
"@

$tempBbvaPath = [System.IO.Path]::GetTempFileName() + ".csv"
[System.IO.File]::WriteAllText($tempBbvaPath, $bbvaCsv, [System.Text.Encoding]::UTF8)

# Form-data upload para Preview
$httpClient = New-Object System.Net.Http.HttpClient
$httpClient.DefaultRequestHeaders.Authorization = New-Object System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", $token)

$multipartContent = New-Object System.Net.Http.MultipartFormDataContent
$fileBytes = [System.IO.File]::ReadAllBytes($tempBbvaPath)
$fileContent = New-Object System.Net.Http.ByteArrayContent($fileBytes, 0, $fileBytes.Length)
$fileContent.Headers.ContentType = New-Object System.Net.Http.Headers.MediaTypeHeaderValue("text/csv")
$multipartContent.Add($fileContent, "File", "bbva_extracto.csv")

$previewResponse = $httpClient.PostAsync("$BaseUrl/api/accounts/$accountId/statements/preview", $multipartContent).Result
$previewJson = $previewResponse.Content.ReadAsStringAsync().Result
if (-not $previewResponse.IsSuccessStatusCode) {
    Write-Error "Fallo en preview de extracto: $previewJson"
}
$previewData = $previewJson | ConvertFrom-Json
$previewToken = $previewData.previewToken
Write-Host "   [OK] Extracto previsualizado. previewToken emitido: $previewToken" -ForegroundColor Green

# Confirmar importación
$confirmReq = @{ previewToken = $previewToken } | ConvertTo-Json
$confirmRes = Invoke-RestMethod -Uri "$BaseUrl/api/accounts/$accountId/statements/confirm" -Method Post -Body $confirmReq -Headers $headers -ContentType "application/json"
Write-Host "   [OK] Extracto confirmado. Transacciones importadas: $($confirmRes.importedTransactionsCount)" -ForegroundColor Green

# 4. Importar Reporte Oficial de Pedidos de Amazon (CSV)
Write-Host "`n4. Importando reporte oficial de pedidos de Amazon (CSV)..." -ForegroundColor Yellow
$amazonOrderDate = [DateTime]::UtcNow.ToString("yyyy-MM-dd")
$amazonCsv = @"
Order ID;Order Date;Title;ASIN/ISBN;Category;Quantity;Unit Price;Item Total;Total
405-$runId-0001;$amazonOrderDate;Raton Inalambrico Ergonomico;B08XYZ123;Electronica;1;29.99;29.99;45.49
405-$runId-0001;$amazonOrderDate;Cable USB-C 2m Trenzado;B09ABC456;Accesorios;1;15.50;15.50;45.49
"@

$tempAmazonPath = [System.IO.Path]::GetTempFileName() + ".csv"
[System.IO.File]::WriteAllText($tempAmazonPath, $amazonCsv, [System.Text.Encoding]::UTF8)

$amazonMultipart = New-Object System.Net.Http.MultipartFormDataContent
$amazonBytes = [System.IO.File]::ReadAllBytes($tempAmazonPath)
$amazonContent = New-Object System.Net.Http.ByteArrayContent($amazonBytes, 0, $amazonBytes.Length)
$amazonContent.Headers.ContentType = New-Object System.Net.Http.Headers.MediaTypeHeaderValue("text/csv")
$amazonMultipart.Add($amazonContent, "File", "amazon_report.csv")

$amazonRes = $httpClient.PostAsync("$BaseUrl/api/commerce/amazon/import-report", $amazonMultipart).Result
$amazonJson = $amazonRes.Content.ReadAsStringAsync().Result
if (-not $amazonRes.IsSuccessStatusCode) {
    Write-Error "Fallo al importar reporte de Amazon: $amazonJson"
}
$amazonData = $amazonJson | ConvertFrom-Json
Write-Host "   [OK] Amazon CSV importado. Total pedidos: $($amazonData.totalOrdersImported), Nuevos: $($amazonData.newOrdersAdded)" -ForegroundColor Green

# 5. Importar Extracto Oficial de Actividad de PayPal (CSV)
Write-Host "`n5. Importando extracto de actividad de PayPal (CSV)..." -ForegroundColor Yellow
$paypalDate = [DateTime]::UtcNow.AddDays(-1).ToString("yyyy-MM-dd")
$paypalCsv = @"
Date;Time;TimeZone;Name;Type;Status;Currency;Gross;Fee;Net;Transaction ID
$paypalDate;18:30:00;CEST;Spotify AB;Express Checkout;Completed;EUR;-12.99;0.00;-12.99;PP-TX-$runId
"@

$tempPaypalPath = [System.IO.Path]::GetTempFileName() + ".csv"
[System.IO.File]::WriteAllText($tempPaypalPath, $paypalCsv, [System.Text.Encoding]::UTF8)

$paypalMultipart = New-Object System.Net.Http.MultipartFormDataContent
$paypalBytes = [System.IO.File]::ReadAllBytes($tempPaypalPath)
$paypalContent = New-Object System.Net.Http.ByteArrayContent($paypalBytes, 0, $paypalBytes.Length)
$paypalContent.Headers.ContentType = New-Object System.Net.Http.Headers.MediaTypeHeaderValue("text/csv")
$paypalMultipart.Add($paypalContent, "File", "paypal_activity.csv")

$paypalRes = $httpClient.PostAsync("$BaseUrl/api/commerce/paypal/import-activity", $paypalMultipart).Result
$paypalJson = $paypalRes.Content.ReadAsStringAsync().Result
if (-not $paypalRes.IsSuccessStatusCode) {
    Write-Error "Fallo al importar actividad de PayPal: $paypalJson"
}
$paypalData = $paypalJson | ConvertFrom-Json
Write-Host "   [OK] PayPal CSV importado. Total: $($paypalData.totalOrdersImported), Nuevos: $($paypalData.newOrdersAdded)" -ForegroundColor Green

# 6. Consultar Reconciliaciones Pendientes
Write-Host "`n6. Consultando candidatos de reconciliación pendientes..." -ForegroundColor Yellow
$reconciliations = Invoke-RestMethod -Uri "$BaseUrl/api/commerce/reconciliations" -Method Get -Headers $headers
Write-Host "   [OK] Reconciliaciones candidatas encontradas: $($reconciliations.Count)" -ForegroundColor Green

if ($reconciliations.Count -lt 2) {
    Write-Error "Se esperaban al menos 2 reconciliaciones candidatas (Amazon y PayPal), pero se obtuvieron $($reconciliations.Count)."
}

$amazonRec = $reconciliations | Where-Object { $_.provider -eq "Amazon" -or $_.provider -eq 1 } | Select-Object -First 1
$paypalRec = $reconciliations | Where-Object { $_.provider -eq "PayPal" -or $_.provider -eq 2 } | Select-Object -First 1

if (-not $amazonRec) {
    Write-Error "No se encontró reconciliación de Amazon."
}
if (-not $paypalRec) {
    Write-Error "No se encontró reconciliación de PayPal."
}

Write-Host "   [OK] Candidato Amazon: ID $($amazonRec.id), Confianza: $($amazonRec.confidence), Artículos: $($amazonRec.matchedItems.Count)" -ForegroundColor Green
$cleanName = if ($paypalRec.counterpartyName) { $paypalRec.counterpartyName } else { "PayPal" }
Write-Host "   [OK] Candidato PayPal: ID $($paypalRec.id), Comercio Desenmascarado: $cleanName" -ForegroundColor Green

# 7. Aceptar Reconciliación de Amazon (Validar Splits exactos RN-02 y CleanMerchant RN-06)
Write-Host "`n7. Aceptando propuesta de reconciliación de Amazon..." -ForegroundColor Yellow
try {
    $acceptRes = Invoke-RestMethod -Uri "$BaseUrl/api/commerce/reconciliations/$($amazonRec.id)/accept" -Method Post -Body "{}" -Headers $headers -ContentType "application/json"
} catch {
    if ($_.Exception.Response) {
        $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream())
        Write-Host "ERROR 400 DETAIL: $($reader.ReadToEnd())" -ForegroundColor Red
    }
    throw
}

Write-Host "   [OK] Reconciliación aceptada. Partidas creadas: $($acceptRes.createdSplitsCount), Comercio limpio: $($acceptRes.merchantName)" -ForegroundColor Green
if ($acceptRes.createdSplitsCount -ne 2) {
    Write-Error "Se esperaban 2 partidas divididas (Splits), pero se obtuvieron $($acceptRes.createdSplitsCount)."
}

# 8. Verificar Transacción en BD tras la Reconciliación
Write-Host "`n8. Verificando estado final de la transacción en la cuenta bancaria..." -ForegroundColor Yellow
$txDetail = Invoke-RestMethod -Uri "$BaseUrl/api/transactions/$($amazonRec.bankTransactionId)" -Method Get -Headers $headers

Write-Host "   [OK] Transacción: Importe: $($txDetail.amount) €, HasSplits: $($txDetail.hasSplits), IsManual: $($txDetail.isManual)" -ForegroundColor Green
Write-Host "   [OK] Nombre Limpio: $($txDetail.cleanMerchantName)" -ForegroundColor Green

if (-not $txDetail.hasSplits) {
    Write-Error "Invariante violada: HasSplits debe ser true."
}
if (-not $txDetail.isManual) {
    Write-Error "Invariante RN-01 violada: IsManual debe ser true para proteger contra sobreescrituras automáticas."
}

$splitSum = 0.00
foreach ($s in $txDetail.splitItems) {
    $splitSum += $s.amount
    Write-Host "      - Partida: $($s.description) -> $($s.amount) €" -ForegroundColor DarkCyan
}

$expectedAmount = [Math]::Abs($txDetail.amount)
$discrepancy = [Math]::Abs($splitSum - $expectedAmount)
Write-Host "   [OK] Suma de Partidas: $splitSum € (Esperado: $expectedAmount €, Discrepancia: $discrepancy €)" -ForegroundColor Green

if ($discrepancy -gt 0.001) {
    Write-Error "Invariante RN-02 violada: La discrepancia debe ser 0.00 €, pero es $discrepancy €"
}

# 9. Descartar Reconciliación de PayPal
Write-Host "`n9. Descartando propuesta de reconciliación de PayPal..." -ForegroundColor Yellow
$rejectReq = @{ reason = "Prueba de descarte manual E2E" } | ConvertTo-Json
$rejectRes = Invoke-RestMethod -Uri "$BaseUrl/api/commerce/reconciliations/$($paypalRec.id)/reject" -Method Post -Body $rejectReq -Headers $headers -ContentType "application/json"
Write-Host "   [OK] Reconciliación de PayPal descartada: $($rejectRes.message)" -ForegroundColor Green

# 10. Consultar listado de órdenes de comercio
Write-Host "`n10. Consultando listado de órdenes importadas..." -ForegroundColor Yellow
$orders = Invoke-RestMethod -Uri "$BaseUrl/api/commerce/orders" -Method Get -Headers $headers
Write-Host "   [OK] Total de órdenes consultadas: $($orders.Count)" -ForegroundColor Green

# Limpieza de temporales
Remove-Item -Path $tempBbvaPath -Force -ErrorAction SilentlyContinue
Remove-Item -Path $tempAmazonPath -Force -ErrorAction SilentlyContinue
Remove-Item -Path $tempPaypalPath -Force -ErrorAction SilentlyContinue

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host "   [PASS] TODOS LOS CRITERIOS E2E DE FEATURE 003 SUPERADOS" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
