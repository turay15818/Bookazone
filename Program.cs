using System.Security.Claims;
using System.Text;
using Bookazone.Api.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Bookazone.Infrastructure;
using Microsoft.Extensions.FileProviders;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using Bookazone.Application.Common.Shared.Utils;
using Bookazone.Infrastructure.Authorization;
using Bookazone.Infrastructure.Persistence;
using Bookazone.Infrastructure.Persistence.DbContext;
using Bookazone.Infrastructure.Persistence.Dependency;
using Bookazone.Infrastructure.Persistence.Seed;
using Bookazone.Infrastructure.Services.JWT;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllersWithViews();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddOpenApi();


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        policy => policy
            .SetIsOriginAllowed(_ => true) 
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
    );
});

builder.Services.AddSwaggerGen(d =>
{
    SwaggerConfig.SwaggerDocs.ForEach(docs =>
    {
        d.SwaggerDoc(docs.Slug, new OpenApiInfo
        {
            Title = docs.Title,
            Version = docs.Version,
            Description = docs.Description,
            Contact = new OpenApiContact { Name = docs.Name }
        });
    });
});
builder.Services.Configure<SwaggerGeneratorOptions>(options => { options.InferSecuritySchemes = true; });
builder.Services.AddAuthorization(auth =>
{
    auth.AddPolicy(JwtBearerDefaults.AuthenticationScheme, new AuthorizationPolicyBuilder()
        .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser().Build());
});
builder.Services.AddAuthorization(options =>
{
    foreach (var permission in typeof(Consts.Permissions).GetFields())
    {
        var permissionValue = permission.GetValue(null)?.ToString();
        if (!string.IsNullOrEmpty(permissionValue))
        {
            options.AddPolicy(permissionValue, policy =>
                policy.Requirements.Add(new PermissionRequirement(permissionValue)));
        }
    }
});


builder.Services.ConfigureOptions<JwtOptionsSetup>();
builder.Services.ConfigureOptions<JwtBearerOptionsSetup>();
builder.Services.AddMvc().AddJsonOptions(opt => { opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles; });


var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<BookazoneDbContext>();
    var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Seeder");
    await SubscriptionPlanSeeder.SeedAsync(context, logger);
}






app.UseStaticFiles();
var storagePath = Path.Combine(app.Environment.ContentRootPath, "storage");
Directory.CreateDirectory(storagePath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storagePath),
    RequestPath = "/storage"
});
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    SwaggerConfig.SwaggerDocs.ForEach(docs =>
    {
        c.SwaggerEndpoint($"/swagger/{docs.Slug}/swagger.json", docs.Slug);
    });
});
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;



if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
}
app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
//app.UseMiddleware<ClientIntegrityMiddleware>();
app.Run();
