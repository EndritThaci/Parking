using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using OfficeOpenXml;
using Parking_project.Data;
using Parking_project.Models;
using Parking_project.Models.DTO;
using Parking_project.Services;
using Scalar.AspNetCore;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

ExcelPackage.License.SetNonCommercialPersonal("localhost");

var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtSettings")["Secret"]);

builder.Services.AddAuthentication(option =>
{
    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    option.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
        context => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.User.Identity?.Name ?? context.Connection.RemoteIpAddress?.ToString() ?? "global",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

builder.Services.AddCors();
// Add services to the container.
builder.Services.AddDbContext<AplicationDbContext>(option =>
{
    option.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi( options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>
        {
            ["Bearer"] = new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Enter JWT Bearer token"
            }
        };
        document.Security =
        [
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer"), new List<string>()
                }
            }
        ];
        return Task.CompletedTask;
    });
});

builder.Services.AddAutoMapper(o =>
{
    o.CreateMap<Organizata, OrgCreateDTO>().ReverseMap();
    o.CreateMap<Organizata, OrgUpdateDTO>().ReverseMap();

    o.CreateMap<Useri, UserReadDTO>().ReverseMap();
    o.CreateMap<UserCreateDTO, Useri>().ReverseMap();
    o.CreateMap<UserUpdateDTO, Useri>().ReverseMap();

    o.CreateMap<NjesiOrg, NjesiOrgDto>().ReverseMap();
    o.CreateMap<NjesiOrg, NjesiUpdateDto>().ReverseMap();
    o.CreateMap<NjesiOrg, NjesiReadDto>().ReverseMap();

    o.CreateMap<CilsimetParkimit, CilsimetReadDto>().ReverseMap();
    o.CreateMap<CilsimetParkimit, CilsimetCreateDto>().ReverseMap();
    o.CreateMap<CilsimetParkimit, CilsimetUpdateDto>().ReverseMap();   
    
    o.CreateMap<Detajet, DetajetReadDto>().ReverseMap();
    o.CreateMap<Detajet, DetajetCreateDto>().ReverseMap();
    o.CreateMap<Detajet, DetajetUpdateDto>().ReverseMap();
  
    o.CreateMap<Lokacioni, LokacioniCreateDTO>().ReverseMap();
    o.CreateMap<Lokacioni, LokacioniUpdateDTO>().ReverseMap();

    o.CreateMap<Vendi, VendiCreateDTO>().ReverseMap();
    o.CreateMap<Vendi, VendiUpdateDTO>().ReverseMap();

    o.CreateMap<Sherbimi, SherbimiCreateDTO>().ReverseMap();
    o.CreateMap<Sherbimi, SherbimiUpdateDTO>().ReverseMap();

    o.CreateMap<TransaksionParkimi, TransaksionetCreateDto>().ReverseMap();
    o.CreateMap<TransaksionParkimi, TransaksionUpdateDto>().ReverseMap();
    o.CreateMap<TransaksionParkimi, TransaksionRead>().ReverseMap();
});

builder.Services.AddScoped<IAuthService, AuthService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseCors(o => o.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("*"));

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();



app.MapControllers();

app.Run();
