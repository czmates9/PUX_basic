using PUX.DirectoryChangeDetector.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddScoped<IFileHashService, FileHashService>();
builder.Services.AddScoped<ISnapshotStorage, JsonSnapshotStorage>();
builder.Services.AddScoped<IDirectoryScanService, DirectoryScanService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Scan/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Scan}/{action=Index}/{id?}");

app.Run();
