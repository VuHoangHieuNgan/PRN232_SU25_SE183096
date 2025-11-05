using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.ModelBuilder;
using Microsoft.OpenApi.Models;
using PRN232_SU25_SE183096.api.ExceptionHandler;
using Repositories.Entities;
using Services;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// OData EDM Model
var modelBuilder = new ODataConventionModelBuilder();  
modelBuilder.EntitySet<Handbag>("Handbags");

// Add services to the container.

// ** Add OData services **
builder.Services.AddControllers()
    .AddOData(options =>
    {
        options
            .Select()
            .Filter()
            .OrderBy()
            .Expand()
            .SetMaxTop(null)
            .Count()
            .AddRouteComponents("odata", modelBuilder.GetEdmModel());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var msg = context.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid input" : e.ErrorMessage)
                .FirstOrDefault() ?? "Missing/invalid input";

            return new BadRequestObjectResult(new { errorCode = "HB40001", message = msg });
        };
    });

// ** Add scoped services **
builder.Services.AddScoped<HandbagService>();
builder.Services.AddScoped<BrandService>();
builder.Services.AddScoped<AccountsService>();


// ** JSON Serializer Options to handle reference loops **
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
});

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();

// ** JWT Authentication Configuration **
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

// ** Swagger Configuration with JWT Support **
builder.Services.AddSwaggerGen(option =>
{
    option.DescribeAllParametersInCamelCase();
    option.ResolveConflictingActions(conf => conf.First());
    option.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter a valid token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="bearer"
                }
            },
            new string[]{}
        }
    });
});

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

app.UseMiddleware<ErrorHandlingMiddleware>();

app.MapControllers();

app.Run();
