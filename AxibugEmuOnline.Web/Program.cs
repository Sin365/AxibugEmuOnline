using AxibugEmuOnline.Web.Common;

namespace AxibugEmuOnline.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Config.LoadConfig();
            AESHelper.LoadKeyIVCfg(Config.cfg.AesKey, Config.cfg.AesIv);
            SQLRUN.InitConnMgr();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
