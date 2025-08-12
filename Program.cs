namespace HadımkoyAnkaraNakliyat_WEB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.Use(async (context, next) =>
            {
                var redirects = new Dictionary<string, string>
                {
                    ["/Home/Fiyat_Al"] = "/Home/fiyat_al",
                    ["/Home/İstanbul_Ankara_Fuar_Nakliyat"] = "/Home/istanbul_ankara_fuar_nakliyat",
                    ["/Home/İletisim"] = "/Home/iletisim",
                    ["/Home/İstanbul_Ankara_Nakliyat"] = "/Home/istanbul_ankara_nakliyat"
                };

                var path = context.Request.Path.Value;
                if (redirects.ContainsKey(path))
                {
                    context.Response.Redirect(redirects[path], permanent: true);
                    return;
                }

                await next();
            });
            app.UseHttpsRedirection();
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
