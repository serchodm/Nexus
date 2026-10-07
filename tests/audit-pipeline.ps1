<#
.SYNOPSIS
    Script de Auditoria Determinista de Fin de Feature (Fase 8 de Jarvis).
.DESCRIPTION
    Ejecuta las 3 verificaciones clave sin consumo de tokens de LLM:
    1. Base de Datos vs C# (has-pending-model-changes de EF Core).
    2. Dominio y Casos de Uso (xUnit Unit Tests al 100%).
    3. Contratos de API vs Frontend (TypeScript tsc -b y build de Vite).
.PARAMETER FeatureId
    Identificador opcional de la feature auditada (ej. 005-open-banking-enable-banking).
.EXAMPLE
    powershell -ExecutionPolicy Bypass -File tests/audit-pipeline.ps1
    powershell -ExecutionPolicy Bypass -File tests/audit-pipeline.ps1 "005-open-banking-enable-banking"
#>
param(
    [string]$FeatureId = ""
)

$StartTime = Get-Date

Write-Host "=================================================================" -ForegroundColor Cyan
Write-Host "  NEXUS - AUDITORIA DETERMINISTA DE FASE 8 (ZERO-TOKEN AUDIT)   " -ForegroundColor Cyan
if (![string]::IsNullOrWhiteSpace($FeatureId)) {
    Write-Host "  Feature ID: $FeatureId" -ForegroundColor Cyan
}
Write-Host "=================================================================" -ForegroundColor Cyan

# -----------------------------------------------------------------------------
# 1. Auditoria Base de Datos vs Modelos C# (EF Core)
# -----------------------------------------------------------------------------
Write-Host ""
Write-Host "[1/3] Auditando sincronizacion de Base de Datos (EF Core)..." -ForegroundColor Yellow
$migrationOutput = dotnet ef migrations has-pending-model-changes --project src/Nexus.Infrastructure --startup-project src/Nexus.Api
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Desfase detectado entre las entidades de C# y las migraciones de PostgreSQL." -ForegroundColor Red
    Write-Host $migrationOutput -ForegroundColor DarkRed
    exit 1
}
Write-Host "   [OK] Cero migraciones pendientes. Base de datos y entidades en sincronia total." -ForegroundColor Green

# -----------------------------------------------------------------------------
# 2. Auditoria de Invariantes y Tests de Dominio (xUnit)
# -----------------------------------------------------------------------------
Write-Host ""
Write-Host "[2/3] Auditando invariantes de negocio y suite unitaria (xUnit)..." -ForegroundColor Yellow
$testOutput = dotnet test tests/Nexus.UnitTests/Nexus.UnitTests.csproj --nologo -v q
if ($LASTEXITCODE -ne 0) {
    Write-Host "[ERROR] Uno o mas tests unitarios han fallado." -ForegroundColor Red
    Write-Host $testOutput -ForegroundColor DarkRed
    exit 1
}
Write-Host "   [OK] 100% de los tests unitarios superados con exito." -ForegroundColor Green

# -----------------------------------------------------------------------------
# 3. Auditoria de Contratos Frontend vs Backend (TypeScript y Vite)
# -----------------------------------------------------------------------------
Write-Host ""
Write-Host "[3/3] Auditando contratos espejo frontend (TypeScript y Vite)..." -ForegroundColor Yellow
Push-Location frontend
$buildOutput = cmd.exe /c "npm.cmd run build"
$buildExitCode = $LASTEXITCODE
Pop-Location

if ($buildExitCode -ne 0) {
    Write-Host "[ERROR] Discrepancia de contratos detectada en el frontend (error de tipos TypeScript)." -ForegroundColor Red
    Write-Host $buildOutput -ForegroundColor DarkRed
    exit 1
}
Write-Host "   [OK] Compilacion de TypeScript limpia. Cero discrepancias en contratos DTO." -ForegroundColor Green

# -----------------------------------------------------------------------------
# Resumen Final
# -----------------------------------------------------------------------------
$Duration = [Math]::Round(((Get-Date) - $StartTime).TotalSeconds, 2)
Write-Host ""
Write-Host "=================================================================" -ForegroundColor Green
Write-Host "  [AUDIT SUCCESS] Solucion verificada al 100% en $Duration segundos. " -ForegroundColor Green
Write-Host "  0 discrepancias de BD | 100% tests en verde | 0 errores TS     " -ForegroundColor Green
Write-Host "=================================================================" -ForegroundColor Green
exit 0
