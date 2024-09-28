using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Polly;
using Portal.Fornecedor.Api.Auth;
using Portal.Fornecedor.Api.Services;
using Portal.Fornecedor.Api.Util;
using System.Net.Http.Headers;
using System.Reflection;
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

// Add services to the container.
var appSettingsSection = builder.Configuration.GetSection("AppSettings");

builder.Services.AddControllers();
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

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
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

//app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthorization();

app.MapControllers();

app.Run();
