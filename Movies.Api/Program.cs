using System.Text;
using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Movies.Api.Auth;
using Movies.Api.Endpoints;
using Movies.Api.Health;
using Movies.Api.Logging;
using Movies.Api.Mapping;
using Movies.Api.Swagger;
using Movies.Application;
using Movies.Application.Database;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

builder.Services
    .AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = config["Jwt:Issuer"],
            ValidAudience = config["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!))
        };
    });

builder.Services
    .AddAuthorization(x =>
    {
        // x.AddPolicy(AuthConstants.AdminUserPolicyName, policy => policy.RequireClaim(AuthConstants.AdminUserClaimName, "true"));
        x.AddPolicy(AuthConstants.AdminUserPolicyName, policy =>
        {
            policy.AddRequirements(new AdminAuthRequirement(config["ApiKey"]!));
        });
        
        x.AddPolicy(AuthConstants.TrustedMemberPolicyName, policy =>
        {
            policy.RequireAssertion(context =>
                context.User.HasClaim(match => match is 
                        {Type: AuthConstants.AdminUserClaimName, Value: "true"} or 
                        {Type: AuthConstants.TrustedMemberClaimName, Value: "true"})
            );
        });
    });

builder.Services.AddScoped<AuthKeyAuthFilter>();

builder.Services.AddApiVersioning(x =>
{
    x.DefaultApiVersion = new ApiVersion(1.0);
    x.AssumeDefaultVersionWhenUnspecified = true;
    x.ReportApiVersions = true;
    x.ApiVersionReader = new MediaTypeApiVersionReader("api-version");
}).AddApiExplorer();

builder.Services.AddEndpointsApiExplorer();

// builder.Services.AddCors();
// builder.Services.AddResponseCaching();
builder.Services.AddOutputCache(x =>
{
    x.AddBasePolicy(c => c.Cache());
    
    x.AddPolicy("MovieCache", c => 
        c.Cache()
            .Expire(TimeSpan.FromMinutes(1))
            .SetVaryByQuery(new []{"title", "yearOfRelease", "sortBy", "page", "pageSize"})
            .Tag("movies"));
});

// builder.Services.AddControllers();

builder.Services
    .AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>(DatabaseHealthCheck.Name);

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen(x => x.OperationFilter<SwaggerDefaultValues>());

builder.Services.AddApplication();
builder.Services.AddDatabase(config["Database:ConnectionString"]!);

var app = builder.Build();

app.CreateApiVersionSet();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(x =>
    {
        foreach (var description in app.DescribeApiVersions())
        {
            x.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName);
        }
    });
}

app.MapHealthChecks("_health");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

//app.UseCors();
//app.UseResponseCaching();
app.UseOutputCache(); // By default it caches all GET (200 OK) responses for 1 hour.


app.UseMiddleware<ValidationMappingMiddleware>();
app.UseMiddleware<LoggingMiddleware>();

// app.MapControllers();
app.MapApiEndpoints();


await app.Services
    .GetRequiredService<DbInitializer>()
    .InitializeAsync();

app.Run();
