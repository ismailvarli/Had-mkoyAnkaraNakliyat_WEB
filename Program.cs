using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HadımkoyAnkaraNakliyat_WEB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // MVC Servisleri
            builder.Services.AddControllersWithViews();

            // Sıkıştırma (Performans için)
            builder.Services.AddResponseCompression(o =>
            {
                o.EnableForHttps = true;
                o.Providers.Add<GzipCompressionProvider>();
                o.Providers.Add<BrotliCompressionProvider>();
            });
            builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = System.IO.Compression.CompressionLevel.Fastest);
            builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = System.IO.Compression.CompressionLevel.Fastest);

            var app = builder.Build();

            // Hata Yakalama
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseResponseCompression();

            // -----------------------------------------------------------------
            // 1. ADIM: URL Normalizasyonu (Büyük harf -> Küçük harf çevirici)
            // Bu kod Google'ın sevdiği temiz URL yapısını sağlar.
            // -----------------------------------------------------------------
            app.Use(async (ctx, next) =>
            {
                var original = ctx.Request.Path.Value ?? "/";

                // Dosyalara ve sistem yollarına dokunma
                if (Path.HasExtension(original) ||
                    original.StartsWith("/assets") ||
                    original.StartsWith("/lib") ||
                    original == "/robots.txt" ||
                    original == "/sitemap.xml")
                {
                    await next();
                    return;
                }

                // URL'yi temizle (Küçük harf yap, Türkçe karakterleri düzelt)
                var normalized = SlugifyPath(original);

                // Eğer URL değişmişse yönlendir (301 Redirect)
                if (!string.Equals(original, normalized, StringComparison.Ordinal))
                {
                    var q = ctx.Request.QueryString.HasValue ? ctx.Request.QueryString.Value : "";
                    ctx.Response.Redirect(normalized + q, permanent: true);
                    return;
                }

                await next();
            });

            // -----------------------------------------------------------------
            // 2. ADIM: Statik Dosyalar (CSS, Resimler çalışsın diye ŞART)
            // -----------------------------------------------------------------
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    // Dosyaları tarayıcıda 1 yıl önbelleğe al (Hız için)
                    ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=31536000,immutable";
                }
            });

            app.UseRouting();
            app.UseAuthorization();

            // -----------------------------------------------------------------
            // 3. ADIM: robots.txt
            // -----------------------------------------------------------------
            app.MapGet("/robots.txt", () => Results.Text(
@"User-agent: *
Allow: /
Sitemap: https://www.hadimkoyankaranakliyat.com/sitemap.xml", "text/plain"));

            // -----------------------------------------------------------------
            // 4. ADIM: SITEMAP (Google için Harita)
            // DİKKAT: Buradaki linklerin hepsi KÜÇÜK HARFLİ olmalı.
            // -----------------------------------------------------------------
            app.MapGet("/sitemap.xml", (HttpContext http) =>
            {
                http.Response.ContentType = "application/xml; charset=utf-8";
                var host = "https://www.hadimkoyankaranakliyat.com";

                // Buraya tüm önemli sayfalarını KÜÇÜK HARFLE ekle
                var urls = new[]
                {
        $"{host}/",
        $"{host}/home/hakkimizda",
        $"{host}/home/iletisim",
        $"{host}/home/fiyat_al",
        $"{host}/home/hizmetlerimiz",
        $"{host}/home/hizmet_turleri",
        $"{host}/home/blog",
        $"{host}/home/iletisim",
        $"{host}/home/nakliyat_hizmet_fiyati",

        //blog
        $"{host}/home/istanbul_ankara_nakliyat_fiyatlari_2025_guncel_rehber",
        $"{host}/home/parsiyel_nakliyat_nedir_istanbul_ankara_arasinda__avantajlari",
        $"{host}/home/evden_eve_nakliyat_oncesi_hazirlik_rehberi",
        $"{host}/home/asansorlu_nakliyatin_istanbul_ankara_tasimalarinda_sagladigi_kolayliklar",
        $"{host}/home/sigortali_nakliyat_esyalarinizi_guvence_altina_alin",


        // Hizmetler (URL'ler middleware ile uyumlu, küçük harfli)        
        $"{host}/home/evden_eve_nakliyat",
        $"{host}/home/ambar_nakliyat",
        $"{host}/home/fuar_tasimaciligi",
        $"{host}/home/ofis_tasimaciligi",
        $"{host}/home/parsiyel_nakliyat",
        $"{host}/home/sehirlerarasi_nakliyat",
        // Şehirler arası (Var olanlar)
        $"{host}/istanbul-ankara-nakliyat",
        $"{host}/home/istanbul_ankara_evden_eve_nakliyat",
        $"{host}/home/istanbul_ankara_fuar_nakliyat",
        $"{host}/home/istanbul_ankara_kamyonet_nakliyat",
        $"{host}/home/istanbul_ankara_ambar_nakliyat",
        $"{host}/home/istanbul_ankara_parsiyel_nakliyat",
        $"{host}/home/istanbul_ankara_sehirlerarasi_nakliyat",
        $"{host}/home/istanbul_ankara_ceyiz_nakliyat",
        $"{host}/home/istanbul_izmir_nakliyat",
        $"{host}/home/istanbul_bursa_nakliyat",
        $"{host}/home/istanbul_eskisehir_nakliyat",
        $"{host}/home/istanbul_antalya_nakliyat",
         //ilçe-Şehir
          $"{host}/home/hadimkoy_ankara_nakliyat",
          $"{host}/home/sincan_istanbul_nakliyat",
          $"{host}/home/beylikduzu_ankara_nakliyat",
          $"{host}/home/ikitelli_ankara_nakliyat",
          $"{host}/home/catalca_ankara_nakliyat",
          $"{host}/home/zeytinburnu_ankara_nakliyat",
          $"{host}/home/bayrampasa_ankara_nakliyat",
          $"{host}/home/ostim_istanbul_nakliyat",
          $"{host}/home/yenimahalle_istanbul_nakliyat",
          $"{host}/home/kazan_istanbul_nakliyat",
          $"{host}/home/temelli_istanbul_nakliyat",
          $"{host}/home/gebze_ankara_nakliyat",
        //şehir bazlı taşımacılık
        $"{host}/home/sehir_bazli_tasimacilik",
        $"{host}/ankara-nakliyat",
        $"{host}/istanbul-nakliyat",
        // İlçe Bazlı Taşımacılık (39 İlçe - Tam Liste)
        $"{host}/home/ilce_bazli_tasimacilik",
        $"{host}/adalar-nakliyat",
        $"{host}/arnavutkoy-nakliyat",
        $"{host}/atasehir-nakliyat",
        $"{host}/avcilar-nakliyat",
        $"{host}/bagcilar-nakliyat",
        $"{host}/bahcelievler-nakliyat",
        $"{host}/bakirkoy-nakliyat",
        $"{host}/basaksehir-nakliyat",
        $"{host}/bayrampasa-nakliyat",
        $"{host}/besiktas-nakliyat",
        $"{host}/beykoz-nakliyat",
        $"{host}/beylikduzu-nakliyat",
        $"{host}/beyoglu-nakliyat",
        $"{host}/buyukcekmece-nakliyat",
        $"{host}/catalca-nakliyat",
        $"{host}/cekmekoy-nakliyat",
        $"{host}/esenler-nakliyat",
        $"{host}/esenyurt-nakliyat",
        $"{host}/eyupsultan-nakliyat",
        $"{host}/fatih-nakliyat",
        $"{host}/gaziosmanpasa-nakliyat",
        $"{host}/gungoren-nakliyat",
        $"{host}/kadikoy-nakliyat",
        $"{host}/kagithane-nakliyat",
        $"{host}/kartal-nakliyat",
        $"{host}/kucukcekmece-nakliyat",
        $"{host}/maltepe-nakliyat",
        $"{host}/pendik-nakliyat",
        $"{host}/sancaktepe-nakliyat",
        $"{host}/sariyer-nakliyat",
        $"{host}/silivri-nakliyat",
        $"{host}/sultanbeyli-nakliyat",
        $"{host}/sultangazi-nakliyat",
        $"{host}/sile-nakliyat",
        $"{host}/sisli-nakliyat",
        $"{host}/tuzla-nakliyat",
        $"{host}/umraniye-nakliyat",
        $"{host}/uskudar-nakliyat",
        $"{host}/zeytinburnu-nakliyat"
    };

                var sb = new StringBuilder();
                sb.AppendLine(@"<?xml version=""1.0"" encoding=""UTF-8""?>");
                sb.AppendLine(@"<urlset xmlns=""http://www.sitemaps.org/schemas/sitemap/0.9"">");
                foreach (var u in urls)
                {
                    sb.AppendLine("  <url>");
                    sb.AppendLine($"    <loc>{u}</loc>");
                    sb.AppendLine("    <changefreq>weekly</changefreq>");
                    sb.AppendLine("    <priority>0.8</priority>");
                    sb.AppendLine("  </url>");
                }
                sb.AppendLine("</urlset>");
                return Results.Text(sb.ToString(), "application/xml; charset=utf-8");
            });
            // MVC Rotaları
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

        // --- YARDIMCI METOT: URL TEMİZLEYİCİ ---
        static string SlugifyPath(string path)
        {
            if (string.IsNullOrEmpty(path) || path == "/") return path;

            var lower = path.ToLowerInvariant();

            // Türkçe karakterleri İngilizceye çevir
            lower = lower
                .Replace('ğ', 'g')
                .Replace('ü', 'u')
                .Replace('ş', 's')
                .Replace('ö', 'o')
                .Replace('ç', 'c')
                .Replace('ı', 'i');

            // Güvenli karakterleri ayıkla
            var normalized = lower.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            var noDiacritics = sb.ToString().Normalize(NormalizationForm.FormC);

            // Boşlukları tire yap
            noDiacritics = noDiacritics.Replace(' ', '-');

            var safe = new StringBuilder();
            foreach (var c in noDiacritics)
            {
                if (char.IsLetterOrDigit(c) || c == '/' || c == '-' || c == '_')
                    safe.Append(c);
            }

            // Çift tireleri temizle (istanbul--ankara olmasın diye)
            var result = safe.ToString();
            while (result.Contains("--")) result = result.Replace("--", "-");

            // Sondaki gereksiz slash'ı sil (kök dizin değilse)
            if (result.Length > 1 && result.EndsWith("/"))
                result = result.TrimEnd('/');

            return result;
        }
    }
}