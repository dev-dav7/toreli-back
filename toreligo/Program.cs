using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.CookiePolicy;
using toreligo.Configurations;
using toreligo.Domain.Authentication;
using toreligo.Domain.Database;
using toreligo.Domain.Group;
using toreligo.Domain.Tracking;
using toreligo.Domain.Tracking.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options => options.AddPolicy(name: AppConfigConstants.MyAllowSpecificOrigins,
    b =>
    {
        b.WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    }));    

builder.Services.AddLinqToDbContext<AppDataConnection>((provider, options) =>
{
    options
        .UsePostgreSQL(PostgressConfig.Local.FormatConnectionString())
        .UseDefaultLogging(provider);   
});

builder.Services.AddScoped<EntityStorage>();
builder.Services.AddScoped<TrackRepository>();
builder.Services.AddScoped<GroupRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<FillTrackingService>();
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(jwt =>
    {
        jwt.RequireHttpsMetadata = true;
        jwt.SaveToken = true;
        jwt.TokenValidationParameters = JwtConfig.TokenValidateParameters;
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseCors(AppConfigConstants.MyAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
    HttpOnly = HttpOnlyPolicy.Always,
    Secure = CookieSecurePolicy.SameAsRequest
});

app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Xss-Protection", "1");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    await next();
});

app.MapControllers();

app.Run();