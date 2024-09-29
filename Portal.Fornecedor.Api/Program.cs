using Asp.Versioning;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Portal.Fornecedor.Api.Auth;
using Portal.Fornecedor.Api.Services;
using Portal.Fornecedor.Api.Util;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration; // allows both to access and to set up the config
IWebHostEnvironment environment = builder.Environment;

builder.Configuration.AddJsonFile("appsettings.json", true, true)
                    .SetBasePath(environment.ContentRootPath)
                    .AddJsonFile($"appsettings.{environment.EnvironmentName}.json", true, true)
                    .AddEnvironmentVariables();
;

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<IUser, AspNetUser>();

// Configuração CSRF
builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN"; // Nome do cabeçalho CSRF
});

// Add services to the container.
var appSettingsSection = builder.Configuration.GetSection("AppSettings");

// Registre o antiforgery e os controladores
builder.Services.AddControllersWithViews();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter JWT Bearer token **_only_**",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer", // must be lower case
        BearerFormat = "JWT",
        Reference = new OpenApiReference
        {
            Id = JwtBearerDefaults.AuthenticationScheme,
            Type = ReferenceType.SecurityScheme
        }
    };
    // To Enable authorization using Swagger (JWT)  
    option.AddSecurityDefinition(securityScheme.Reference.Id, securityScheme);
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {securityScheme, new string[] { }}
                });

    option.SwaggerDoc("v1", new OpenApiInfo()
    {
        Title = "Portal Fornecedor Zuritch",
        Description = "Esta API é uma Enterprise Applications.",
        Contact = new OpenApiContact() { Name = "Osmar Gonçalves Vieira", Email = "osmargv100@gmail.com" },
        License = new OpenApiLicense() { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") }
    });

    var xmlCommentsFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlCommentsFullPath = Path.Combine(AppContext.BaseDirectory, xmlCommentsFile);

    if (File.Exists(xmlCommentsFullPath))
        option.IncludeXmlComments(xmlCommentsFullPath);

});

builder.Services.AddTransient<HttpClientAuthorizationDelegatingHandler>();

builder.Services.AddHttpClient<IUserService, UserService>(opt =>
{
    opt.BaseAddress = new Uri(configuration.GetSection("AppSettings:UsersApiUrl").Value);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler()
{
    ServerCertificateCustomValidationCallback =
        HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
})
.AddHttpMessageHandler<HttpClientAuthorizationDelegatingHandler>()
.AddPolicyHandler(PollyExtensions.PollyWaitAndRetryAsync())
.AddTransientHttpErrorPolicy(
p => p.CircuitBreakerAsync(5, TimeSpan.FromSeconds(30)));


builder.Services.AddHttpClient<TempConvertClient>(http => {
    http.BaseAddress = new Uri("https://www.w3schools.com/xml/tempconvert.asmx");

});

builder.Services.AddHttpClient<IServiceGoogleCaptcha, ServiceGoogleCaptcha>(http => {
    http.BaseAddress = new Uri("https://www.google.com/recaptcha/api/siteverify");
    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddScoped<JwtBuilder>();
builder.Services.AddScoped<LNotifications>();

/*proteção aos cookies*/
builder.Services.AddDataProtection();

builder.Services.AddCors(options =>
{

    options.AddPolicy("Development",
          builder =>
              builder
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowAnyOrigin()
              ); // allow credentials

    options.AddPolicy("Production",
        builder =>
            builder
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowAnyOrigin()
              ); // allow credentials
});

builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true;
}).AddApiExplorer(
options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.Configure<AppJwtSettings>(appSettingsSection);

var appSettings = appSettingsSection.Get<AppJwtSettings>();
var key = Encoding.ASCII.GetBytes(appSettings?.SecretKey ?? "");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Pegando o token criptografado do cookie
            var encryptedToken = context.Request.Cookies["X-Access-Token"];

            if (!string.IsNullOrEmpty(encryptedToken))
            {
                // Usando o serviço de proteção de dados para descriptografar o token
                var dataProtector = context.HttpContext.RequestServices.GetRequiredService<IDataProtectionProvider>();
                var protector = dataProtector.CreateProtector("X-Access-Token");

                try
                {
                    // Descriptografando o token
                    var decryptedToken = protector.Unprotect(encryptedToken);

                    // Definindo o token descriptografado no contexto para validação JWT
                    context.Token = decryptedToken;
                }
                catch (CryptographicException)
                {
                    // Se falhar a descriptografia, não processa o token
                    context.NoResult();
                }
            }
            return Task.CompletedTask;
        }
    };
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = appSettings.Audience,
        ValidIssuer = appSettings.Issuer
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Development");
}
else
{
    app.UseCors("Production");
}

// Configuração de CSRF
app.Use(next => context =>
{
    if (context.Request.Path.StartsWithSegments("/api") &&
        (context.Request.Method == "POST" ||
         context.Request.Method == "PUT" ||
        
         context.Request.Method == "DELETE"))
    {
        var antiForgery = context.RequestServices.GetRequiredService<IAntiforgery>();
        var tokenSet = antiForgery.GetTokens(context);
        context.Response.Cookies.Append("XSRF-TOKEN", tokenSet.RequestToken,
            new CookieOptions
            {
                HttpOnly = false, // Permite o acesso ao cookie via JavaScript
                Secure = true, // Envia o cookie apenas via HTTPS
                SameSite = SameSiteMode.Strict // Restrição ao mesmo site
            });
    }
    return next(context);
});

// Permitir o uso de arquivos estáticos
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseAuthorization();

app.MapControllers();

// Configuração de CSP
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("Content-Security-Policy", "default-src 'self'; script-src 'self'; style-src 'self'; img-src 'self' data:; connect-src 'self';");
    await next();
});

app.Run();
