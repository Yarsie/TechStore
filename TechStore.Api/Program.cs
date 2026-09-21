using Serilog;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TechStore.Infrastructure.Persistence;
using TechStore.Application.Services;
using TechStore.Infrastructure.Services;
using TechStore.Application.Validators;
using TechStore.Application.DTOs;
using FluentValidation;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using StackExchange.Redis;
using MassTransit;
using TechStore.Infrastructure.Consumers;
using Microsoft.OpenApi.Models;
using TechStore.Api;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/techstore-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting TechStore Web API...");

    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.

    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "TechStore API",
            Version = "v1"
        });

        c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());

        // Hide OData metadata endpoints from Swagger
        c.DocInclusionPredicate((docName, apiDesc) =>
        {
            // Exclude OData metadata endpoints
            if (apiDesc.RelativePath != null && apiDesc.RelativePath.Contains("$metadata"))
            {
                return false;
            }
            // Exclude OData controllers from Swagger
            if (apiDesc.RelativePath != null && 
                (apiDesc.RelativePath.Contains("ODataProducts") || apiDesc.RelativePath.Contains("ODataCategories") || apiDesc.RelativePath.StartsWith("api/odata")))
            {
                return false;
            }
            return true;
        });

        // Remove OData content types from Swagger
        c.OperationFilter<RemoveODataContentTypesOperationFilter>();

        c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
            Name = "Authorization",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
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
                Array.Empty<string>()
            }
        });
    });
    builder.Host.UseSerilog();

    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    {
        var configuration = builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379";
        var options = ConfigurationOptions.Parse(configuration);
        options.AbortOnConnectFail = false;
        return ConnectionMultiplexer.Connect(options);
    });

    builder.Services.AddScoped<ICacheService, CacheService>();

    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"] ?? "TechStore",
            ValidAudience = jwtSettings["Audience"] ?? "TechStoreUsers",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

    builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IProductService, ProductService>();
    builder.Services.AddScoped<ICategoryService, CategoryService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<ICartService, CartService>();

    builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestDtoValidator>();
    builder.Services.AddValidatorsFromAssemblyContaining<CreateProductDtoValidator>();
    builder.Services.AddValidatorsFromAssemblyContaining<AddToCartDtoValidator>();

    // MassTransit configuration
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumer<OrderCreatedConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            var rabbitConn = builder.Configuration.GetConnectionString("RabbitMQ") ?? "amqp://guest:guest@localhost:5672";
            cfg.Host(rabbitConn);

            cfg.ConfigureEndpoints(context);
        });
    });

    builder.Services.AddScoped<IOrderService, OrderService>();

    // OData configuration
    var modelBuilder = new ODataConventionModelBuilder();
    modelBuilder.EntitySet<ProductResponseDto>("ODataProducts");
    modelBuilder.EntitySet<CategoryDto>("ODataCategories");

    builder.Services.AddControllers()
        .AddOData(options => options
            .Select()
            .Filter()
            .OrderBy()
            .SkipToken()
            .Count()
            .Expand()
            .SetMaxTop(100)
            .AddRouteComponents("api/odata", modelBuilder.GetEdmModel()));

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    await DbInitializer.SeedAsync(app.Services);
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application start-up failed");
}
finally
{
    Log.CloseAndFlush();
}