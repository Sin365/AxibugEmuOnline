using AxibugEmuOnline.Web.Common;
using Microsoft.AspNetCore.StaticFiles;

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

            // 添加自定义 MIME 类型映射
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".sav"] = "application/octet-stream";  // 显式关联扩展名

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider  // 注入自定义类型提供程序
            });

            app.Run();
        }
    }
}
