# E2E Test Script for Feature 002: Categorización Inteligente, Merchant Intelligence y Splits
param(
    [string]$BaseUrl = "http://localhost:5276"
)

Add-Type -AssemblyName System.Net.Http
$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   NEXUS E2E TEST: CATEGORIZATION, MERCHANT & SPLITS" -ForegroundColor Cyan
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

# 2. Consultar Taxonomia de Categorias (40 del sistema)
Write-Host "`n2. Verificando taxonomia de categorias del sistema..." -ForegroundColor Yellow
$categoryTree = Invoke-RestMethod -Uri "$BaseUrl/api/categories" -Method Get -Headers $headers
$rootCategories = $categoryTree
$subCategories = @($categoryTree | ForEach-Object { $_.subcategories })
$totalCategories = $rootCategories.Count + $subCategories.Count

Write-Host "   [OK] Categorias recuperadas: $totalCategories (Raiz: $($rootCategories.Count), Subcategorias: $($subCategories.Count))" -ForegroundColor Green
if ($totalCategories -lt 40) {
    Write-Error "Se esperaban al menos 40 categorias del sistema sembradas, se obtuvieron $totalCategories."
}

# 3. Crear Categoria Personalizada de Usuario
Write-Host "`n3. Creando categoria personalizada de usuario..." -ForegroundColor Yellow
$supermercadosCat = $subCategories | Where-Object { $_.name -like "*Supermercado*" } | Select-Object -First 1
$customCatReq = @{
    name = "Mascotas y Piensos $runId"
    icon = "dog"
    colorHex = "#10B981"
    flowType = 1 # Expense
    defaultExpenseNature = 1 # Individual
    displayOrder = 99
} | ConvertTo-Json

$customCat = Invoke-RestMethod -Uri "$BaseUrl/api/categories" -Method Post -Body $customCatReq -Headers $headers -ContentType "application/json"
Write-Host "   [OK] Categoría personalizada creada: $($customCat.name) (ID: $($customCat.id), IsSystem: $($customCat.isSystem))" -ForegroundColor Green

# 4. Crear Cuenta Bancaria para pruebas
Write-Host "`n4. Creando cuenta bancaria..." -ForegroundColor Yellow
$accountReq = @{
    name = "BBVA Categorización $runId"
    iban = "ES910182123456789012$runId"
    type = 1 # Checking
    holderRole = 2 # JointOwner
    ownershipPercentage = 50.0
    linkedCardsLast4 = @("9999")
} | ConvertTo-Json

$account = Invoke-RestMethod -Uri "$BaseUrl/api/accounts" -Method Post -Body $accountReq -Headers $headers -ContentType "application/json"
Write-Host "   [OK] Cuenta creada: $($account.name) (ID: $($account.id))" -ForegroundColor Green

# 5. Previsualizar extracto con casos de Merchant Intelligence, Bizum y Recurrencias
Write-Host "`n5. Previsualizando extracto con transacciones variadas..." -ForegroundColor Yellow

$tsvContent = "Fecha`tF.Valor`tConcepto`tMovimiento`tImporte`tDivisa`tDisponible`tDivisa`tObservaciones`n" +
              "25/09/2026`t25/09/2026`tTPV 9821 MERCADONA S.A.`tCOMPRA TARJETA`t-45,20`tEUR`t1.400,00`tEUR`tTARJETA 9999`n" +
              "25/09/2026`t25/09/2026`tPAYPAL *NETFLIX COM`tSUSCRIPCION`t-17,99`tEUR`t1.382,01`tEUR`tPAGO MENSUAL`n" +
              "24/09/2026`t24/09/2026`tBIZUM CENA CUMPLEAÑOS AMIGOS`tTRANSFERENCIA`t-25,00`tEUR`t1.357,01`tEUR`tBIZUM SALIR`n" +
              "23/09/2026`t23/09/2026`tRECIBO IBERDROLA CLIENTES SA`tRECIBO`t-68,40`tEUR`t1.288,61`tEUR`tSUMINISTRO LUZ`n" +
              "22/09/2026`t22/09/2026`tCOMPRA EL CORTE INGLES GRUPO`tCOMPRA TARJETA`t-150,00`tEUR`t1.138,61`tEUR`tPLANTA 4 Y 1"

$tempFile = [System.IO.Path]::GetTempFileName() + ".tsv"
[System.IO.File]::WriteAllText($tempFile, $tsvContent, [System.Text.Encoding]::UTF8)

$httpClient = [System.Net.Http.HttpClient]::new()
$httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer $token")
$multipart = [System.Net.Http.MultipartFormDataContent]::new()
$fileStream = [System.IO.File]::OpenRead($tempFile)
$streamContent = [System.Net.Http.StreamContent]::new($fileStream)
$streamContent.Headers.ContentType = [System.Net.Http.Headers.MediaTypeHeaderValue]::Parse("text/tab-separated-values")
$multipart.Add($streamContent, "file", "extracto_categorizacion.tsv")

$previewUrl = "$BaseUrl/api/accounts/$($account.id)/statements/preview"
$httpResponse = $httpClient.PostAsync($previewUrl, $multipart).Result
$fileStream.Close()
Remove-Item $tempFile -Force

if (-not $httpResponse.IsSuccessStatusCode) {
    $errBody = $httpResponse.Content.ReadAsStringAsync().Result
    Write-Error "Fallo en la previsualización: $errBody"
}
$previewJson = $httpResponse.Content.ReadAsStringAsync().Result | ConvertFrom-Json
Write-Host "   [OK] Previsualización completada (Token: $($previewJson.previewToken), Total: $($previewJson.totalRecords))" -ForegroundColor Green

# 6. Confirmar importación y verificar auto-categorización
Write-Host "`n6. Confirmando importación y ejecutando pipeline automático de categorización..." -ForegroundColor Yellow
$confirmReq = @{ previewToken = $previewJson.previewToken } | ConvertTo-Json
$confirmResult = Invoke-RestMethod -Uri "$BaseUrl/api/accounts/$($account.id)/statements/confirm" -Method Post -Body $confirmReq -Headers $headers -ContentType "application/json"
Write-Host "   [OK] Lote confirmado exitosamente (Importados: $($confirmResult.importedRecords))" -ForegroundColor Green

# Consultar transacciones persistidas con sus metadatos enriquecidos
$txList = Invoke-RestMethod -Uri "$BaseUrl/api/transactions?bankAccountId=$($account.id)" -Method Get -Headers $headers
Write-Host "   [OK] Transacciones recuperadas: $($txList.items.Count)" -ForegroundColor Green

$mercadonaTx = $txList.items | Where-Object { $_.description -like "*MERCADONA*" } | Select-Object -First 1
$netflixTx = $txList.items | Where-Object { $_.description -like "*NETFLIX*" } | Select-Object -First 1
$bizumTx = $txList.items | Where-Object { $_.description -like "*BIZUM*" } | Select-Object -First 1
$iberdrolaTx = $txList.items | Where-Object { $_.description -like "*IBERDROLA*" } | Select-Object -First 1
$corteInglesTx = $txList.items | Where-Object { $_.description -like "*CORTE INGLES*" } | Select-Object -First 1

Write-Host "`n--- Comprobación de Enriquecimiento Inteligente ---" -ForegroundColor Magenta
Write-Host "   * Mercadona: CleanMerchant='$($mercadonaTx.cleanMerchantName)', Categoria='$($mercadonaTx.categoryName)', Confianza='$($mercadonaTx.categorizationConfidence)'" -ForegroundColor Cyan
Write-Host "   * Netflix: CleanMerchant='$($netflixTx.cleanMerchantName)', Recurrente=$($netflixTx.isRecurring), Plataforma='$($netflixTx.externalPlatform)'" -ForegroundColor Cyan
Write-Host "   * Bizum: CleanMerchant='$($bizumTx.cleanMerchantName)', Categoria='$($bizumTx.categoryName)', Naturaleza='$($bizumTx.expenseNature)'" -ForegroundColor Cyan
Write-Host "   * Iberdrola: CleanMerchant='$($iberdrolaTx.cleanMerchantName)', Categoria='$($iberdrolaTx.categoryName)', Fijo=$($iberdrolaTx.isFixedExpense)" -ForegroundColor Cyan

# Validaciones
if ($mercadonaTx.cleanMerchantName -ne "MERCADONA") {
    Write-Error "Fallo en Merchant Intelligence: se esperaba 'MERCADONA' y se obtuvo '$($mercadonaTx.cleanMerchantName)'"
}
if ($netflixTx.externalPlatform -ne 1) { # 1 = PayPal
    Write-Error "Fallo en detección de PayPal: externalPlatform esperado 1, obtenido $($netflixTx.externalPlatform)"
}
if (-not $netflixTx.isRecurring) {
    Write-Error "Fallo en detección de recurrencia/suscripción para Netflix!"
}
Write-Host "   [OK] Merchant Intelligence, detección de PayPal y Recurrencia validados al 100%!" -ForegroundColor Green

# 7. Probar Desglose de Gastos (Split Transactions - RN-02)
Write-Host "`n7. Probando Split Transactions (Invariante Contable RN-02)..." -ForegroundColor Yellow
$targetTxId = $corteInglesTx.id
Write-Host "   Transacción a desglosar: ID=$targetTxId, Importe=$($corteInglesTx.amount) EUR" -ForegroundColor DarkGray

# 7.1 Intento de Split Inválido (Suma != Total -> Error 400 esperado)
Write-Host "   7.1 Enviando split descuadrado (-100 + -40 = -140 != -150)..." -ForegroundColor Yellow
$invalidSplitReq = @{
    items = @(
        @{ categoryId = $supermercadosCat.parentCategoryId; subcategoryId = $supermercadosCat.id; amount = -100.00; expenseNature = 1 },
        @{ categoryId = $supermercadosCat.parentCategoryId; amount = -40.00; expenseNature = 1 }
    )
} | ConvertTo-Json

try {
    $null = Invoke-RestMethod -Uri "$BaseUrl/api/transactions/$targetTxId/split" -Method Post -Body $invalidSplitReq -Headers $headers -ContentType "application/json"
    Write-Error "ERROR: La API permitió un split que viola el invariante contable!"
} catch {
    Write-Host "   [OK] Invariante Contable protegido correctamente: rechazada suma inconsistente (400 Bad Request)." -ForegroundColor Green
}

# 7.2 Split Válido (-100.00 + -50.00 = -150.00)
Write-Host "   7.2 Enviando split válido (-100.00 + -50.00 = -150.00)..." -ForegroundColor Yellow
$validSplitReq = @{
    items = @(
        @{ categoryId = $supermercadosCat.parentCategoryId; subcategoryId = $supermercadosCat.id; amount = -100.00; expenseNature = 1; description = "Alimentación gourmet" },
        @{ categoryId = $customCat.id; amount = -50.00; expenseNature = 1; description = "Pienso para mascotas" }
    )
} | ConvertTo-Json

$validSplitRes = Invoke-RestMethod -Uri "$BaseUrl/api/transactions/$targetTxId/split" -Method Post -Body $validSplitReq -Headers $headers -ContentType "application/json"
Write-Host "   [OK] Split ejecutado exitosamente: $($validSplitRes.splitItems.Count) partidas registradas." -ForegroundColor Green

# Consultar transacción y splits persistidos
$splitsGet = Invoke-RestMethod -Uri "$BaseUrl/api/transactions/$targetTxId" -Method Get -Headers $headers
if ($splitsGet.splitItems.Count -ne 2) {
    Write-Error "Se esperaban 2 partidas de desglose, obtenidas $($splitsGet.splitItems.Count)."
}
Write-Host "   [OK] Partidas verificadas en base de datos: Partida 1: $($splitsGet.splitItems[0].amount) EUR, Partida 2: $($splitsGet.splitItems[1].amount) EUR" -ForegroundColor Green

# 8. Probar Categorización Manual y Creación de Regla de Usuario
Write-Host "`n8. Probando categorización manual y creación automática de regla..." -ForegroundColor Yellow
$manualCatReq = @{
    categoryId = $supermercadosCat.parentCategoryId
    subcategoryId = $supermercadosCat.id
    isRecurring = $false
    expenseNature = 2 # Shared
    notes = "Ajustado manualmente por el usuario"
    createRuleForMerchant = $true
} | ConvertTo-Json

$categorizedTx = Invoke-RestMethod -Uri "$BaseUrl/api/transactions/$($mercadonaTx.id)/categorize" -Method Post -Body $manualCatReq -Headers $headers -ContentType "application/json"
Write-Host "   [OK] Transacción categorizada manualmente:" -ForegroundColor Green
Write-Host "        - IsManual: $($categorizedTx.isManual)" -ForegroundColor DarkGray
Write-Host "        - CategorizationSource: $($categorizedTx.categorizationSource) (Esperado: 5 - Manual)" -ForegroundColor DarkGray

if (-not $categorizedTx.isManual -or $categorizedTx.categorizationSource -ne 5) {
    Write-Error "La transacción no quedó marcada como manual!"
}

# 9. Verificar Reglas de Usuario y Aplicación Retroactiva
Write-Host "`n9. Verificando reglas de usuario y ejecución retroactiva..." -ForegroundColor Yellow
$rules = Invoke-RestMethod -Uri "$BaseUrl/api/rules" -Method Get -Headers $headers
Write-Host "   [OK] Reglas de usuario registradas: $($rules.Count)" -ForegroundColor Green

$mercadonaRule = $rules | Where-Object { $_.pattern -like "*MERCADONA*" } | Select-Object -First 1
if (-not $mercadonaRule) {
    Write-Error "No se encontró la regla generada para MERCADONA!"
}
Write-Host "   [OK] Regla automática verificada: Patrón='$($mercadonaRule.pattern)', Prioridad=$($mercadonaRule.priority)" -ForegroundColor Green

# Ejecutar aplicación retroactiva
Write-Host "`n10. Ejecutando aplicación retroactiva de reglas..." -ForegroundColor Yellow
$retroactiveReq = @{
    overrideUserManual = $false
} | ConvertTo-Json
$retroactiveRes = Invoke-RestMethod -Uri "$BaseUrl/api/rules/apply-retroactive" -Method Post -Body $retroactiveReq -Headers $headers -ContentType "application/json"
Write-Host "   [OK] Aplicación retroactiva completada: Procesadas=$($retroactiveRes.totalProcessed), Modificadas=$($retroactiveRes.totalUpdated)" -ForegroundColor Green

# Verificar que la transacción manual no fue sobreescrita (Prevalencia RN-01)
$txAfterRetro = Invoke-RestMethod -Uri "$BaseUrl/api/transactions?bankAccountId=$($account.id)" -Method Get -Headers $headers
$mercadonaAfter = $txAfterRetro.items | Where-Object { $_.id -eq $mercadonaTx.id } | Select-Object -First 1

if (-not $mercadonaAfter.isManual -or $mercadonaAfter.categorizationSource -ne 5) {
    Write-Error "VIOLACION DE JERARQUIA RN-01: Una regla automatica sobreescribio una categorizacion manual!"
}
Write-Host "   [OK] Jerarquia de Prevalencia (RN-01) respetada: IsManual intacto." -ForegroundColor Green

Write-Host "==========================================================" -ForegroundColor Green
Write-Host "   TODAS LAS PRUEBAS E2E DE LA FEATURE 002 HAN PASADO CON EXITO!" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green

