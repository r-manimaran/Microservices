using Microsoft.AspNetCore.RateLimiting;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "API Gateway", 
        Version = "v1",
        Description = "API Gateway using YARP"
    });

    // Add security definition for Bearer token
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    // Add security requirement for Bearer token
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


builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

// Logging
builder.Logging.AddFilter("Microsoft.AspNetCore.Routing", LogLevel.Debug);
builder.Logging.AddFilter("Yarp.ReverseProxy", LogLevel.Debug);

// Add Health Checks to destination URLs
builder.Services.AddHealthChecks()
     .AddUrlGroup(new Uri("http://localhost:5198/Users"), name: "Users API");

//Rate Limiting
builder.Services.AddRateLimiter(opt =>
{
    opt.AddFixedWindowLimiter("fixed", options =>
    {
        options.Window = TimeSpan.FromSeconds(5);
        options.PermitLimit = 2;
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Gateway V1");
    });
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.MapReverseProxy();

app.Run();
