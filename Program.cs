using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Trillix.Data;
using Trillix.Modules.Catalog;
using Trillix.Modules.Catalog.Storage;
using Trillix.Modules.Identity;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));

var imagesPath = Path.Combine(builder.Environment.ContentRootPath,
    builder.Configuration["Storage:ImagesPath"] ?? "uploads/products");
builder.Services.AddSingleton<IImageStorage>(new LocalImageStorage(imagesPath));
builder.Services.AddSingleton<IPasswordService, Argon2PasswordService>();
builder.Services.AddTrillixIdentity(builder.Configuration);

var app = builder.Build();

Directory.CreateDirectory(imagesPath);
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = "/media/products"
});

app.UseAuthentication();
app.UseAuthorization();

app.MapAdminAuthEndpoints();
app.MapProductEndpoints();
app.Run();
