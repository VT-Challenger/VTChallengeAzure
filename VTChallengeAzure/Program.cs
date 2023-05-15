using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using VTChallengeAzure.Helpers;
using VTChallengeAzure.Services;

var builder = WebApplication.CreateBuilder(args);

// Blobs
string azureKeys = builder.Configuration.GetValue<string>("AzureKeys:StorageAccount");
BlobServiceClient blobServiceClient = new BlobServiceClient(azureKeys);
builder.Services.AddTransient<BlobServiceClient>(x => blobServiceClient);

builder.Services.AddAuthentication(options => {
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie();

builder.Services.AddHttpContextAccessor();

builder.Services.AddTransient<ServiceVTChallenge>();
builder.Services.AddTransient<ServiceStorageBlob>();
builder.Services.AddSingleton<HelperMails>();
builder.Services.AddSingleton<HelperBlob>();
builder.Services.AddSingleton<HelperJson>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => {
    options.IdleTimeout = TimeSpan.FromMinutes(60);
});
builder.Services.AddControllersWithViews(options => options.EnableEndpointRouting = false);

builder.Services.AddAntiforgery();
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment()) {
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.UseMvc(routes => {
    routes.MapRoute(
        name: "default",
        template: "{controller=Landing}/{action=Index}/{id?}"
   );
});

app.Run();
