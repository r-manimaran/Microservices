using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using JwtAuthenticationManager.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.SetBasePath(builder.Environment.ContentRootPath)
                    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerForOcelot(builder.Configuration);
builder.Services.AddCustomJwtAuthentication();

builder.Services.AddOcelot(builder.Configuration);
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseSwaggerForOcelotUI(options => {
    options.PathToSwaggerGenerator ="/swagger/docs";
});

await app.UseOcelot();
app.UseAuthentication();
app.UseAuthorization();

await app.RunAsync();


