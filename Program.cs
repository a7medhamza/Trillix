using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Trillix.Data;
using Trillix.Modules.Catalog;
using Trillix.Modules.Catalog.Storage;
using Trillix.Modules.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();   // keep whatever the template generated (Swagger/OpenAPI)

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

var imagesPath = Path.Combine(builder.Environment.ContentRootPath,
    builder.Configuration["Storage:ImagesPath"] ?? "uploads/products");
builder.Services.AddSingleton<IImageStorage>(new LocalImageStorage(imagesPath));
builder.Services.AddSingleton<IPasswordService, Argon2PasswordService>();

var app = builder.Build();
await AdminSeeder.SeedAsync(app.Services);

Directory.CreateDirectory(imagesPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = "/media/products"
});

app.MapProductEndpoints();
app.Run();