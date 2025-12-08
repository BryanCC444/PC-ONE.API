param(
    [string]$ProjectPath = ".\PC ONE.API.csproj",
    [int]$HttpsPort = 44395,
    [string]$NgrokPath = "C:\Users\Dell\OneDrive\Desktop\ngrok.exe",
    [string]$SqlServerConnection = "Server=DESKTOP-4KJ4F1J\SQLEXPRESS;Database=PcOneDB;Trusted_Connection=True;TrustServerCertificate=True;",
    [string]$JwtKey = "clave-super-secreta-de-32-caracteres-minimo",
    [string]$JwtIssuer = "PCONE",
    [string]$JwtAudience = "PCONEUsers"
)

$root = Get-Location

Write-Host "==> 1) Creando estructura mínima si falta..."
New-Item -ItemType Directory -Path "$root\screens" -Force | Out-Null
New-Item -ItemType Directory -Path "$root\sql" -Force | Out-Null

Write-Host "==> 2) Definiendo variables de entorno en la sesión actual..."
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:Jwt__Key = $JwtKey
$env:Jwt__Issuer = $JwtIssuer
$env:Jwt__Audience = $JwtAudience
$env:ConnectionStrings__PcOneDb = $SqlServerConnection

Write-Host "  - ASPNETCORE_ENVIRONMENT = $env:ASPNETCORE_ENVIRONMENT"
Write-Host "  - ConnectionStrings__PcOneDb = $env:ConnectionStrings__PcOneDb"

Write-Host "==> 3) Publicando el proyecto (dotnet publish)..."
# Publicar usando la ruta relativa al .csproj en la carpeta actual
dotnet publish $ProjectPath -c Release -o .\publish
if ($LASTEXITCODE -ne 0) { Write-Error "Publicación fallida. Aborta."; exit 1 }

Write-Host "==> 4) Iniciando la app publicada en segundo plano..."
$publishDir = (Resolve-Path .\publish).Path
$dllName = "PC ONE.API.dll"
$dllPath = Join-Path $publishDir $dllName
if (-not (Test-Path $dllPath)) {
    Write-Error "No encontré $dllName en $publishDir. Revisa el nombre del ensamblado."
    exit 1
}
Start-Process -FilePath "dotnet" -ArgumentList "`"$dllPath`"" -WorkingDirectory $publishDir
Start-Sleep -Seconds 4

Write-Host "==> 5) Iniciando ngrok (si existe en la ruta configurada)..."
if (Test-Path $NgrokPath) {
    Start-Process -FilePath $NgrokPath -ArgumentList "http https://localhost:$HttpsPort -host-header=localhost" -WorkingDirectory (Split-Path $NgrokPath) -WindowStyle Normal
    Start-Sleep -Seconds 3
    Write-Host "ngrok iniciado. Abre http://127.0.0.1:4040 para ver la URL pública."
    try {
        $tunnels = Invoke-RestMethod -Uri "http://127.0.0.1:4040/api/tunnels" -UseBasicParsing -ErrorAction Stop
        if ($tunnels.tunnels.Count -gt 0) {
            $public = $tunnels.tunnels[0].public_url
            Write-Host "URL pública (ngrok): $public"
        } else {
            Write-Warning "ngrok arrancó pero no hay túneles listados aún."
        }
    } catch {
        Write-Warning "No pude consultar la API local de ngrok. Abre http://127.0.0.1:4040 para ver la URL."
    }
} else {
    Write-Warning "ngrok no encontrado en $NgrokPath. Inicia ngrok manualmente o ajusta la ruta."
    Write-Host "Swagger local: https://localhost:$HttpsPort/swagger"
}

Write-Host "==> 6) Comandos curl de ejemplo (ajusta <HOST> por la URL pública o localhost)"
Write-Host "Login (obtener token):"
Write-Host "curl -s -X POST `"<HOST>/api/auth/login`" -H `"Content-Type: application/json`" -d '{""username"":""admin"",""password"":""admin123""}' | ConvertFrom-Json"
