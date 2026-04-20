using FirstMvcApp.Services;
using FirstMvcApp.Repositories;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add MongoDB Service
builder.Services.AddSingleton<MongoDbService>();

// Add JWT Service
builder.Services.AddScoped<JwtService>();

// Add Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// Add Repository Layer
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add Service Layer
builder.Services.AddScoped<IUserService, UserService>();

// Add Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FirstMvcApp API",
        Version = "v1",
        Description = "A comprehensive API for FirstMvcApp featuring user authentication, JWT tokens, and MongoDB integration.",
        Contact = new OpenApiContact
        {
            Name = "FirstMvcApp Team",
            Email = "support@firstmvcapp.com"
        },
        License = new OpenApiLicense
        {
            Name = "Use under LICX"
        }
    });

    // Add JWT Bearer token authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme (Example: 'Bearer 12345abcdef')",
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
            new string[] { }
        }
    });

    // Only include API controllers (those in the API folder/naming pattern)
    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        var controllerName = apiDesc.ActionDescriptor?.RouteValues["controller"]?.ToString() ?? "";
        // Include only API controllers (exclude MVC controllers)
        return controllerName.EndsWith("Api") || 
               controllerName == "UsersApi" || 
               controllerName == "AccountApi";
    });

    // Include XML comments for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// Enable Swagger middleware
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "FirstMvcApp API v1");
    c.RoutePrefix = "swagger";
    c.DefaultModelsExpandDepth(2);
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
