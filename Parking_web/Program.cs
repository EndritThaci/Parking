using Parking_web.Models;
using Parking_web.Models.DTO;
using Parking_web.Services;
using Parking_web.Services.IServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(60);
    option.Cookie.HttpOnly = true;
    option.Cookie.IsEssential = true;
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
    o.CreateMap<NjesiUpdateDto, NjesiReadDto>().ReverseMap();

    o.CreateMap<CilsimetParkimit, CilsimetReadDto>().ReverseMap();
    o.CreateMap<CilsimetParkimit, CilsimetCreateDto>().ReverseMap();
    o.CreateMap<CilsimetParkimit, CilsimetUpdateDto>().ReverseMap();
    o.CreateMap<CilsimetReadDto, CilsimetUpdateDto>().ReverseMap();
    o.CreateMap<CilsimetReadDto, CilsimetCreateDto>().ReverseMap();

    o.CreateMap<Detajet, DetajetReadDto>().ReverseMap();
    o.CreateMap<Detajet, DetajetCreateDto>().ReverseMap();
    o.CreateMap<Detajet, DetajetUpdateDto>().ReverseMap();
    o.CreateMap<DetajetReadDto, DetajetUpdateDto>().ReverseMap();

    o.CreateMap<Lokacioni, LokacioniCreateDTO>().ReverseMap();
    o.CreateMap<Lokacioni, LokacioniUpdateDTO>().ReverseMap();

    o.CreateMap<Vendi, VendiCreateDTO>().ReverseMap();
    o.CreateMap<Vendi, VendiUpdateDTO>().ReverseMap();

    o.CreateMap<Sherbimi, SherbimiCreateDTO>().ReverseMap();
    o.CreateMap<Sherbimi, SherbimiUpdateDTO>().ReverseMap();

    o.CreateMap<TransaksionParkimi, TransaksionetCreateDto>().ReverseMap();
    o.CreateMap<TransaksionParkimi, TransaksionUpdateDto>().ReverseMap();
    o.CreateMap<TransaksionParkimi, TransaksionRead>().ReverseMap();
    o.CreateMap<TransaksionUpdateDto, TransaksionRead>().ReverseMap();
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
        options.SlidingExpiration = true;
        options.LoginPath = "/auth/logIn";
        options.AccessDeniedPath = "/auth/accessdenied";
    });
builder.Services.AddHttpClient("OrganizataAPI", client =>
{
    var organizataUrl = builder.Configuration.GetValue<string>("ServiceUrls:OrganizataAPI");
    client.BaseAddress = new Uri(organizataUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddScoped<IOrganizataService, OrganizataService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INjesiaService, NjesiaService>();
builder.Services.AddScoped<ISherbimiService, SherbimiService>();
builder.Services.AddScoped<ICilsimiService, CilsimiService>();
builder.Services.AddScoped<IDetajetService, DetajetService>();
builder.Services.AddScoped<ILokacioniService, LokacioniService>();
builder.Services.AddScoped<IVendiService, VendiService>();
builder.Services.AddScoped<ITransaksionService, TransaksionService>();
builder.Services.AddScoped<ILibriService, LibriService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=login}")
    .WithStaticAssets();


app.Run();
