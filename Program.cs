using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using SistemOptimasiWebAPI.Interfaces;
using SistemOptimasiWebAPI.Repositories;


var builder = WebApplication.CreateBuilder(args);

// 1. Konfigurasi Excel & Database
ExcelPackage.License.SetNonCommercialPersonal("Tijar");
var conn = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(conn));


// Buat Contrler -> Repo
builder.Services.AddScoped<IKantor, KantorRepository>();
builder.Services.AddScoped<IKiriman, KirimanRepository>();
builder.Services.AddScoped<IOptimization, OptimizationRepository>();
builder.Services.AddScoped<IPenugasan, PenugasanRepository>();
builder.Services.AddScoped<IProofOfDelivery, ProofOfDeliveryRepository>();
builder.Services.AddScoped<IRoles, RolesRepository>();
builder.Services.AddScoped<IPengguna, PenggunaRepository>();
builder.Services.AddScoped<IAuth, AuthRepository>();




// 2. Setup JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "KunciRahasiaDefaultYangSangatPanjang123";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// 3. Setup Controller & JSON (Agar relasi Role-User tidak error)
builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Masukkan token JWT dengan format: Bearer {token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});
builder.Services.AddHttpClient();

// 4. CORS Policy
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

var app = builder.Build();

// 5. Middleware Pipeline
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors("AllowAll");

app.UseAuthentication(); // CEK TOKEN
app.UseAuthorization();  // CEK HAK AKSES

app.MapControllers();
app.Run();