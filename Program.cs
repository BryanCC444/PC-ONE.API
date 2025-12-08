using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PcOne.Infrastructure.Data;
using PcOne.Core.Repositories;
using PcOne.Infrastructure.Repositories;
using PcOne.Core.Services;
using PcOne.Infrastructure.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) =>
    lc.WriteTo.Console().ReadFrom.Configuration(ctx.Configuration));

// DbContext
builder.Services.AddDbContext<PcOneDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("PcOneDb")));

// Repositorios y servicios
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUserService, UserService>();
// Registra aquí otros servicios concretos si los tienes
// builder.Services.AddScoped<IProductService, ProductService>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

// Controllers + FluentValidation
builder.Services.AddControllers();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<UserService>();

// JWT
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt Key no configurada");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = true;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });

// Autorización
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PC ONE.API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer token authorization"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();


// Crear usuario Bryan con contraseña 123 si no existe
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PcOneDbContext>();

    var exists = await db.Users.FirstOrDefaultAsync(u => u.Username == "Bryan");
    if (exists == null)
    {
        // ⚠️ Esto es solo para pruebas: guarda la contraseña como texto base64
        string passwordHash = Convert.ToBase64String(Encoding.UTF8.GetBytes("123"));

        db.Users.Add(new PcOne.Core.Entities.User
        {
            Username = "Bryan",
            PasswordHash = passwordHash,
            Role = "Admin"
        });

        await db.SaveChangesAsync();
        Console.WriteLine("Usuario Bryan creado con contraseña 123");
    }
}


// Middleware de manejo simple de errores
app.Use(async (ctx, next) =>
{
    try { await next(); }
    catch (Exception ex)
    {
        ctx.Response.StatusCode = 500;
        await ctx.Response.WriteAsJsonAsync(new { error = ex.Message });
    }
});

// --- Static files and routing middleware (important order) ---
// Serve files from wwwroot (index.html, app.js, css, images, etc.)
app.UseStaticFiles();

// Routing must be enabled before auth and endpoints
app.UseRouting();

// Apply CORS policy (you registered "AllowAll")
app.UseCors("AllowAll");

// Habilitar Swagger en Development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// HTTPS redirection (opcional)
app.UseHttpsRedirection();

// Authentication / Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map controllers
app.MapControllers();

// If a request doesn't match any controller route, serve index.html (SPA fallback)
app.MapFallbackToFile("index.html");

app.Run();
