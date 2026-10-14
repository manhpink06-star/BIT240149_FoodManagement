using System.Globalization;
using BIT240149_FoodManagement.Data;
using BIT240149_FoodManagement.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    options.ModelBindingMessageProvider.SetValueMustBeANumberAccessor(value =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "vi"
            ? $"Giá trị '{value}' phải là một số hợp lệ."
            : $"The value '{value}' must be a valid number.");
    options.ModelBindingMessageProvider.SetAttemptedValueIsInvalidAccessor((value, field) =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "vi"
            ? $"Giá trị '{value}' không hợp lệ cho trường {field}."
            : $"The value '{value}' is not valid for {field}.");
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<UiText>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Keep number-form parsing consistent with HTML <input type="number"> (dot decimal),
// while independently translating the UI using CurrentUICulture.
var uiCultures = new[] { new CultureInfo("en"), new CultureInfo("vi") };
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("en", "en"),
    SupportedCultures = new[] { new CultureInfo("en") },
    SupportedUICultures = uiCultures,
    RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider()
    }
});

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dishes}/{action=Index}/{id?}");

// For this classroom project, create the SQL Server schema automatically on first run.
// Do not combine EnsureCreated with EF Core migrations on the same database.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
    await DemoSeeder.SeedAsync(db);
}

app.Run();
