using Service = OTP_Share.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IConfiguration>(provider =>
{
  var configuration = new ConfigurationBuilder()
      .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
      .AddEnvironmentVariables()
      .Build();

  // Read version from version.txt
  var versionFilePath = Path.Combine(AppContext.BaseDirectory, "version.txt");
  if(File.Exists(versionFilePath))
  {
    var version = File.ReadAllText(versionFilePath).Trim();
    configuration["AppVersion"] = version;
  }

  return configuration;
});

builder.WebHost.ConfigureKestrel(options =>
{
  options.ListenAnyIP(8080); // HTTP
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<Service.EnvironmentService.IEnvironmentService, Service.EnvironmentService.EnvironmentVariablesSrv>();
builder.Services.AddSingleton<Service.NTPService.INTPService, Service.NTPService.NTPService>();
builder.Services.AddSingleton<Service.VaultwardenService.IVaultService, Service.VaultwardenService.VaultService>();
builder.Services.AddSingleton<Service.StateDBService.IStateDBService, Service.StateDBService.StateDBService>();
builder.Services.AddSingleton<Service.DatalayerService>();

//builder.Services.AddAuthentication("BasicAuthentication")
//   .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("BasicAuthentication", null);
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if(!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

// This handles all clean root URL logic
app.MapControllerRoute(
   name: "clean",
   pattern: "",
   defaults: new { controller = "Router", action = "Index" }
);

// Optional: fallback for other controller routes
app.MapControllerRoute(
   name: "default",
   pattern: "{controller}/{action}/{id?}");

// Trigger the start of the DatalayerService
var dataLayerService = app.Services.GetRequiredService<Service.DatalayerService>();
dataLayerService.Init();

app.Run();