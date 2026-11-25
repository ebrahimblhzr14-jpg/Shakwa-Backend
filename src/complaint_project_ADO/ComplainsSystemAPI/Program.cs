using ComplaintManagement.Access.Interfaces;
using ComplaintManagement.Access.Repositories;
using ComplaintManagement.Business.Interfaces;
using ComplaintManagement.Business.Services;
using ComplaintManagement.Business.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using ComplainsSystemAPI.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using System.Security.Claims;
using Serilog;
using Microsoft.AspNetCore.HttpLogging;
using Serilog.Events;
using ComplaintsSystemAPI.Middleware;

using ComplaintManagement.Access.Database;


var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "DefaultConnection is missing.");

builder.Services.AddSingleton(
    new SqlConnectionFactory(connectionString));

//builder.Host.UseSerilog();
builder.Host.UseSerilog((context, services, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext();
});
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields =
        HttpLoggingFields.RequestMethod |
        HttpLoggingFields.RequestPath |
        HttpLoggingFields.ResponseStatusCode |
        HttpLoggingFields.Duration;
});
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type = SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT",

            In = ParameterLocation.Header,

            Description =
                "Enter your JWT token. Example: Bearer {token}"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference =
                        new OpenApiReference
                        {
                            Type =
                                ReferenceType.SecurityScheme,

                            Id = "Bearer"
                        }
                },
                Array.Empty<string>()
            }
        });
});

builder.Services.AddAuthentication(
    JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer =
                    builder.Configuration[
                        "Jwt:Issuer"],

                ValidAudience =
                    builder.Configuration[
                        "Jwt:Audience"],

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            builder.Configuration[
                                "Jwt:Key"]!))
            };
    });





builder.Services.AddAuthorization();




//builder.Services.AddSingleton<SqlConnectionFactory>(sp =>
//{
//    string connectionString =
//        builder.Configuration.GetConnectionString("DefaultConnection")!;

//    return new SqlConnectionFactory(connectionString);
//});


builder.Services.AddScoped<IUsersRepository, UsersRepository>();

builder.Services.AddScoped<IRolesRepository, RolesRepository>();


builder.Services.AddScoped<IDepartmentsRepository, DepartmentsRepository>();

builder.Services.AddScoped<ICategoriesRepository, CategoriesRepository>();

builder.Services.AddScoped<IStatusesRepository, StatusesRepository>();

builder.Services.AddScoped<IPrioritiesRepository, PrioritiesRepository>();

builder.Services.AddScoped<IComplaintsRepository, ComplaintsRepository>();

builder.Services.AddScoped<IAssignmentsRepository, AssignmentsRepository>();

builder.Services.AddScoped<ICommentsRepository, CommentsRepository>();

builder.Services.AddScoped<IAttachmentsRepository, AttachmentsRepository>();

builder.Services.AddScoped<
    IComplaintHistoryRepository,
    ComplaintHistoryRepository>();
builder.Services.AddScoped<IFeedbackRepository,FeedbackRepository>();
builder.Services.AddScoped<IDashboardRepository,DashboardRepository>();

builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<
    IRefreshTokensRepository,
    RefreshTokensRepository>();

builder.Services.AddScoped<
    IDepartmentService,
    DepartmentService>();

builder.Services.AddScoped<
    ICategoryService,
    CategoryService>();

builder.Services.AddScoped<
    IStatusService,
    StatusService>();

builder.Services.AddScoped<
    IPriorityService,
    PriorityService>();

builder.Services.AddScoped<
    IComplaintService,
    ComplaintService>();

builder.Services.AddScoped<
    IAssignmentService,
    AssignmentService>();

builder.Services.AddScoped<
    ICommentService,
    CommentService>();

builder.Services.AddScoped<
    IAttachmentService,
    AttachmentService>();

builder.Services.AddScoped<
    IComplaintHistoryService,
    ComplaintHistoryService>();

builder.Services.AddScoped<
    IFeedbackService,
    FeedbackService>();

builder.Services.AddScoped<
    IDashboardService,
    DashboardService>();

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.AddScoped<PasswordHasher>();

builder.Services.AddSingleton<JwtSettings>();

builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddScoped<RefreshTokenGenerator>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IRefreshTokensRepository,
    RefreshTokensRepository>();



builder.Services.AddSingleton<IAuthorizationHandler, OwnerOrSuperAdminHandler>();
builder.Services.AddSingleton<IAuthorizationHandler, OwnerOrAdminOrSuperAdminHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OwnerOrSuperAdmin", policy =>
        policy.Requirements.Add(new OwnerOrSuperAdminRequirement()));

    options.AddPolicy("OwnerOrAdminOrSuper", policy =>
    policy.Requirements.Add(new OwnerOrAdminOrSuperAdminRequirement()));

    
});



builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .WithOrigins(
               "https://localhost:7208", 
               "http://localhost:5199"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("AuthLimiter", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(2),
                QueueLimit = 0
            });
    });


    options.AddPolicy("SingInLimiter", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 1,
                Window = TimeSpan.FromMinutes(15),
                QueueLimit = 0
            });
    });


    options.AddPolicy("GeneralApiLimiter", httpContext =>
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? httpContext.Connection.RemoteIpAddress?.ToString()
                  ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"General_{userId}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,                // 5 ÿ·»
                Window = TimeSpan.FromMinutes(1), // ·ﬂ· œﬁÌﬁ… Ê«Õœ…
                QueueLimit = 0
            });
    });

    options.AddPolicy("CreateInApiLimiter", httpContext =>
    {
        var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                  ?? httpContext.Connection.RemoteIpAddress?.ToString()
                  ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"Create_{userId}",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromMinutes(3),
                QueueLimit = 0
            });
    });

    options.OnRejected = (context, cancellationToken) =>
    {
        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("RateLimiting");

        var policyName = context.HttpContext.GetEndpoint()?
            .Metadata
            .GetMetadata<EnableRateLimitingAttribute>()?
            .PolicyName;

        var userId = context.HttpContext.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? "Anonymous";

        var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString()
            ?? "Unknown";

        var method = context.HttpContext.Request.Method;

        var path = context.HttpContext.Request.Path.ToString();

        var statusCode = StatusCodes.Status429TooManyRequests;

        logger.LogWarning(
      "Rate limit exceeded. UserId: {UserId}, Policy: {PolicyName}, Method: {Method}, Path: {Path}, IP: {IpAddress}, StatusCode: {StatusCode}",
      userId,
      policyName,
      method,
      path,
      ipAddress,
      statusCode);

        return ValueTask.CompletedTask;
    };
});







var app = builder.Build();



app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set(
            "RequestHost",
            httpContext.Request.Host.Value);

        diagnosticContext.Set(
            "RequestScheme",
            httpContext.Request.Scheme);
    };

    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (ex != null)
            return LogEventLevel.Error;

        if (httpContext.Response.StatusCode >= 500)
            return LogEventLevel.Error;

        if (httpContext.Response.StatusCode >= 400)
            return LogEventLevel.Warning;

        return LogEventLevel.Information;
    };
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("CorsPolicy");
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseRateLimiter();


app.MapControllers();
app.UseStaticFiles(); //  ›⁄Ì· Œœ„…  ﬁœÌ„ «·„·›«  „‰ wwwroot

app.Run();
