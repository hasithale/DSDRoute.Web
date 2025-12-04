using DSDRoute.Data;
using DSDRoute.Models;
using DSDRoute.Hubs;
using DSDRoute.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlServerOptionsAction: sqlOptions =>
        {
            sqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            sqlOptions.CommandTimeout(60); // 60 seconds timeout
        });
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    
    // Signin settings
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
    
    // User settings
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8); // Increased from 30 minutes to 8 hours
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

// Add Authorization services
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization(options =>
{
    // Register all permissions as policies
    foreach (var permission in typeof(Permissions).GetFields()
        .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
        .Select(f => f.GetValue(null)?.ToString()))
    {
        if (!string.IsNullOrEmpty(permission))
        {
            options.AddPolicy(permission, policy =>
                policy.Requirements.Add(new PermissionRequirement(permission)));
        }
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();

app.MapHub<OrderHub>("/orderHub");

// Seed data with error handling
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Attempting to connect to database and seed data...");
        
        // Test database connection
        var context = services.GetRequiredService<ApplicationDbContext>();
        var canConnect = await context.Database.CanConnectAsync();
        
        if (!canConnect)
        {
            logger.LogError("Cannot connect to the database. Please check your connection string in appsettings.json");
            logger.LogError("Connection String: {ConnectionString}", 
                builder.Configuration.GetConnectionString("DefaultConnection")?.Replace("Password=Hazz@119", "Password=***"));
        }
        else
        {
            logger.LogInformation("Database connection successful!");
            
            // Apply pending migrations
            logger.LogInformation("Applying pending migrations...");
            await context.Database.MigrateAsync();
            
            // Seed initial data
            logger.LogInformation("Seeding initial data...");
            await SeedData.InitializeAsync(services);
            logger.LogInformation("Data seeding completed successfully!");
        }
    }
    catch (SqlException sqlEx)
    {
        logger.LogError(sqlEx, "SQL Server connection error occurred. Details: {Message}", sqlEx.Message);
        logger.LogError("Please verify:");
        logger.LogError("1. Database server is running and accessible");
        logger.LogError("2. Connection string is correct");
        logger.LogError("3. Firewall allows connections to the database server");
        logger.LogError("4. Network connectivity to: mssql-204152-0.cloudclusters.net:10061");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database initialization: {Message}", ex.Message);
    }
    
    logger.LogInformation("Application starting...");
}

app.Run();
