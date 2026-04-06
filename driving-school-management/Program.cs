using driving_school_management.Helpers;
using driving_school_management.Models;
using driving_school_management.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleDb")));
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<SignOracleHelper>();

//thêm services Admin
builder.Services.AddScoped<AdminDashboardService>();
builder.Services.AddScoped<AdminExamService>();
builder.Services.AddScoped<AdminKhoaHocService>();
//thêm services
builder.Services.AddScoped<KhoaHocService>();
builder.Services.AddScoped<HocService>();
builder.Services.AddScoped<LyThuyetService>();
builder.Services.AddScoped<AdminUserService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<IThiMoPhongService, ThiMoPhongService>();
builder.Services.AddScoped<IMoPhongService, MoPhongService>();
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPayPalService, PayPalService>();
builder.Services.AddScoped<IMomoService, MomoService>();
builder.Services.AddScoped<PaymentHistoryService>();
builder.Services.AddScoped<PaymentInvoiceService>();
builder.Services.AddScoped<HoSoService>();
builder.Services.AddScoped<IPhotoValidationService, PhotoValidationService>();
builder.Services.AddScoped<AiChatService>();
//
builder.Services.AddHttpClient();
//
builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
var app = builder.Build();
//cap nhat trang thai khi chay project
using (var scope = app.Services.CreateScope())
{
    var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
    var dbInit = new DbInitializer(config);
    dbInit.Init();
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
