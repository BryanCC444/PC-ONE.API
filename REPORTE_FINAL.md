# REPORTE FINAL - PC ONE

**Alumno:** Bryan  
**Fecha:** [YYYY-MM-DD]

## Resumen
Entrega local del backend .NET y frontend estático servido desde `wwwroot` del proyecto API. La aplicación se ejecuta localmente y el frontend consume la API mediante JWT.

## Cómo ejecutar local (pasos exactos)
1. Abrir PowerShell en la carpeta del proyecto:


cd "C:\Users\Dell\source\repos\Pc One Tijuana\PC ONE.API"


2. Publicar (opcional):
dotnet publish ".\PC ONE.API.csproj" -c Release -o .\publish

Código
3. Ejecutar (modo desarrollo o publicado):
- Desde Visual Studio: F5 o Ctrl+F5.
- Desde PowerShell (publicado):
  ```
  dotnet ".\publish\PC ONE.API.dll"
  ```
4. Abrir frontend:
- Si ejecutas desde Visual Studio o dotnet en la raíz: `http://localhost:5000/index.html` (o `https://localhost:44395/index.html` si tu VS usa ese puerto).

## Variables de entorno y configuración
- `Jwt__Key`: [valor en desarrollo]  
- `ConnectionStrings__PcOneDb`: [cadena de conexión local]



## Ejemplos de requests
```bash
# Login
curl -X POST "http://localhost:5000/api/auth/login" -H "Content-Type: application/json" -d '{"username":"admin","password":"admin123"}'

# List products (con token)
curl -X GET "http://localhost:5000/api/products" -H "Authorization: Bearer <TOKEN>"
Notas
El frontend está en wwwroot y se sirve desde la misma app, por lo que no se requiere CORS.

Si el backend se ejecuta en otro puerto, actualizar host en wwwroot/app.js.

Código

---

### Comprobaciones finales antes de ejecutar
- **Asegúrate** de que `wwwroot/index.html` y `wwwroot/app.js` aparecen en Solution Explorer y su **Build Action** es `Content`.  
- **Confirma** que `app.UseStaticFiles()` está presente en `Program.cs`.  
- **Ejecuta** el proyecto desde Visual Studio y abre `http://localhost:5000/index.htm