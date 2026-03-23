using driving_school_management.Models;
using driving_school_management.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleDb")));

//thêm services
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<AdminDashboardService>();
builder.Services.AddScoped<AdminExamService>();
builder.Services.AddScoped<KhoaHocService>();
builder.Services.AddScoped<HocService>();
builder.Services.AddScoped<LyThuyetService>();
builder.Services.AddScoped<AdminUserService>();
builder.Services.AddScoped<IHomeService, HomeService>();
//
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
