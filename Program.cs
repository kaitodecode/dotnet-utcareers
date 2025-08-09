using Microsoft.EntityFrameworkCore;
using dotnet_utcareers.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Amazon.S3;
using dotnet_utcareers.Services;
using Amazon.Runtime;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options => {
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});

var connectionString = builder.Configuration.GetConnectionString("default");

builder.Services.AddDbContext<UTCareersContext>(options =>
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// Add AWS S3 service
var awsSection = builder.Configuration.GetSection("AWS");
var accessKey = awsSection["AccessKey"];
var secretKey = awsSection["SecretKey"];
var serviceUrl = awsSection["ServiceURL"];
var region = awsSection["Region"] ?? "ap-southeast-1";

// Buat config khusus untuk IDCloudHost
var s3Config = new AmazonS3Config
{
    ServiceURL = serviceUrl, // endpoint IDCloudHost
    ForcePathStyle = true    // penting untuk non-AWS S3
};

var credentials = new BasicAWSCredentials(accessKey, secretKey);
var s3Client = new AmazonS3Client(credentials, s3Config);

// Register AmazonS3Client sebagai singleton
builder.Services.AddSingleton<IAmazonS3>(s3Client);
builder.Services.AddScoped<ImageUploadService>();

builder.Services.AddControllers();

// JWT Configuration
var jwtKey = builder.Configuration["Jwt:Key"] ?? "your-super-secret-jwt-key-that-is-at-least-32-characters-long";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "dotnet-utcareers";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "dotnet-utcareers-users";

// Add Authentication
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
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "UT Careers API", Version = "v1" });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
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
            new string[] {}
        }
    });
});

var app = builder.Build();

app.UseCors(policy =>
    policy
        .WithOrigins("http://localhost:5173") // Bisa ditambah .WithOrigins("http://localhost:5173") dll
        .WithOrigins("http://localhost:3000") // Bisa ditambah .WithOrigins("http://localhost:5173") dll
        .AllowAnyMethod()
        .AllowAnyHeader()
);


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

app.Run();
