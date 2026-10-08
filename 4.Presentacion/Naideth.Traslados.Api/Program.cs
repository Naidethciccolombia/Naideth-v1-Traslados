using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Naideth.Traslados.Api.Autorizaciones;
using Naideth.Traslados.Api.Kernel.GlobalExceptions;
using Naideth.Traslados.Aplicacion;
using Naideth.Traslados.Aplicacion.CacheRedis.DTO;
using Naideth.Traslados.Aplicacion.Kernel;
using Naideth.Traslados.Infrastructura;
using Serilog;
using StackExchange.Redis;
using System.IO.Compression;
using System.Text;

var builder = WebApplication.CreateBuilder(args);


builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Fastest;
});

builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/json" });
});



// Configurar Serilog desde appsettings.json
builder.Host.UseSerilog((context, services, loggerConfiguration) =>
    loggerConfiguration.ReadFrom.Configuration(context.Configuration)

    .Filter.ByExcluding(logEvent =>
        logEvent.Properties.ContainsKey("SourceContext") &&
        (logEvent.Properties["SourceContext"].ToString().StartsWith("\"Microsoft.") ||
         logEvent.Properties["SourceContext"].ToString().StartsWith("\"System.") ||
         logEvent.Properties["SourceContext"].ToString().StartsWith("\"Serilog."))
     ||
    (logEvent.MessageTemplate.Text.Contains("Request starting") ||
     logEvent.MessageTemplate.Text.Contains("Request finished") ||
     logEvent.MessageTemplate.Text.Contains("Application started") ||
     logEvent.MessageTemplate.Text.Contains("Now listening on"))
      )
    );

builder.Services.AddHttpContextAccessor();

// Agregar servicios al contenedor.
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
// Health check
builder.Services.AddHealthChecks();

// Manejador de excepciones globales
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddControllers().ConfigureApiBehaviorOptions(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState.LastOrDefault().Value.Errors.First().ErrorMessage;

        return new BadRequestObjectResult(new
        {
            Title = "Alerta",
            Status = 400,
            Mensaje = $"Revisar campos en peticion, {errors}"
        });

    };
});

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Configurar Swagger/OpenAPI con soporte para JWT
builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Naideth Traslado Api", Version = "v1" });
    c.OperationFilter<SwaggerFileUploadOperationFilter>();

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Introduce tu JWT token en el formato 'Bearer {token}'",
    };

    c.AddSecurityDefinition("Bearer", securityScheme);

    c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        { new OpenApiSecuritySchemeReference("Bearer"), new List<string>() }        
    });

});

var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]);

builder.Services.AddAuthentication(x =>
{
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
      builder =>
      {
          builder
              .WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials() // Permitir el envío de credenciales
              .WithExposedHeaders("X-Requested-With", "X-SignalR-User-Agent"); // Encabezados expuestos

      });
});

builder.Services.AddSignalR();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MyPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
        // Otros requisitos de la política...
    });
});

builder.Services.AddTransient<AuthorizationMiddleware>();

builder.Services.AddMemoryCache(options => {
    options.SizeLimit = 800 * 1024 * 1024; // 500 MB
});

builder.Services.Configure<CacheSettings>(
    builder.Configuration.GetSection("CacheSettings"));

 
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = builder.Configuration["Redis:ConnectionString"];
    return ConnectionMultiplexer.Connect(configuration);
});

var app = builder.Build();

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});


if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    // Enable Swagger in production
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Naideth Traslado Api");
        c.RoutePrefix = string.Empty; // Serves Swagger UI at application's root
    });
}


if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// 🔹 Middleware de logs para ASP.NET Core
app.UseSerilogRequestLogging();

// Use health check
var healthCheckEndpointName = builder.Configuration["HealthCheckEndpointName"];
app.MapHealthChecks(healthCheckEndpointName ?? "/health");

// Utilice el controlador de excepciones global
app.UseExceptionHandler();

app.UseRouting();
// Aplicar la política de CORS
app.UseCors("AllowSpecificOrigin");

// Asegúrate de que el middleware de autenticación esté antes del middleware de autorización
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<AuthorizationMiddleware>();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();

// Asegúrate de cerrar correctamente el logger al finalizar la aplicación
Log.CloseAndFlush();
