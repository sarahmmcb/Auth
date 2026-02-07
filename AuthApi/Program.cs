using System.Text;
using AuthApi.Data;
using AuthApi.Email;
using AuthApi.Helpers;
using AuthApi.Logging;
using AuthApi.Logic;
using AuthApi.Models;
using AuthApi.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

var configuration = builder.Configuration;

builder.Services.AddLogging();

builder.Services.Configure<SmtpSettings>(
    configuration.GetSection("SmtpSettings"));

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:4200")  // Replace with your frontend URL
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                          //policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                      });


});

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtConfig = builder.Configuration.GetSection("JwtConfig");
        var secret = jwtConfig.GetSection("Secret").Value;
        var issuer = jwtConfig.GetSection("ValidIssuer").Value;
        var audience = jwtConfig.GetSection("ValidAudiences").Get<List<string>>();

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudiences = audience,
            IssuerSigningKey = signingKey
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin",
         policy => policy.RequireRole("Admin"));
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter '<your-token>'"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            new List<string>()
        }
    });
});

builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ISessionService, SessionService>();
builder.Services.AddTransient<IVerificationCodeService, VerificationCodeService>();
builder.Services.AddSingleton<EmailManager>();
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, LocalAuthMiddleware>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    DbInitializer.Seed(scope.ServiceProvider);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
        options.RoutePrefix = string.Empty;
    });
}

ConfigurationHelper.Initialize(app.Services.GetRequiredService<IConfiguration>());

app.UseHttpsRedirection();
app.UseCustomHttpLogging();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
