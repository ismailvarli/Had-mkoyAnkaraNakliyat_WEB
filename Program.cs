using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.ResponseCompression;

namespace HadımkoyAnkaraNakliyat_WEB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // MVC
            builder.Services.AddControllersWithViews();

            // İsteğe bağlı: Brotli + Gzip sıkıştırma
            builder.Services.AddResponseCompression(o =>
            {
                o.EnableForHttps = true;
                o.Providers.Add<GzipCompressionProvider>();
                o.Providers.Add<BrotliCompressionProvider>();
            });
            builder.Services.Configure<BrotliCompressionProviderOptions>(o => o.Level = System.IO.Compression.CompressionLevel.Fastest);
            builder.Services.Configure<GzipCompressionProviderOptions>(o => o.Level = System.IO.Compression.CompressionLevel.Fastest);

            var app = builder.Build();

            // Hata yakalama + HSTS
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseResponseCompression();

            // --- 301 sabit yönlendirmeler (senin liste) ---
            app.Use(async (context, next) =>
            {
                var redirects = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["/Home/Fiyat_Al"] = "/Home/fiyat_al",
                    ["/Home/İstanbul_Ankara_Fuar_Nakliyat"] = "/Home/istanbul_ankara_fuar_nakliyat",
                    ["/Home/İletisim"] = "/Home/iletisim",
                    ["/Home/İstanbul_Ankara_Nakliyat"] = "/Home/istanbul_ankara_nakliyat",
                };

                var path = context.Request.Path.Value ?? "/";
                if (redirects.TryGetValue(path, out var dest))
                {
                    context.Response.Redirect(dest, permanent: true);
                    return;
                }

                await next();
            });

            // --- URL normalizasyonu (sadece sayfa isteklerine, dosyalara DOKUNMAZ) ---
            app.Use(async (ctx, next) =>
            {
                var original = ctx.Request.Path.Value ?? "/";

                // 1) Uzantılı istekleri (png, jpg, css, js, svg, webp, ico, map, woff2 vs.) BYPASS
                if (Path.HasExtension(original))
                {
                    await next();
                    return;
                }

                // 2) Belirli kök klasörleri BYPASS (statik içerik)
                if (original.StartsWith("/assets", StringComparison.OrdinalIgnoreCase) ||
                    original.StartsWith("/lib", StringComparison.OrdinalIgnoreCase) ||
                    original.StartsWith("/fonts", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(original, "/robots.txt", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(original, "/sitemap.xml", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(original, "/favicon.ico", StringComparison.OrdinalIgnoreCase))
                {
                    await next();
                    return;
                }

                var normalized = SlugifyPath(original);

                // sonda / kaldır (kök hariç)
                if (normalized.EndsWith("/") && normalized != "/")
                    normalized = normalized.TrimEnd('/');

                if (!string.Equals(original, normalized, StringComparison.Ordinal))
                {
                    var q = ctx.Request.QueryString.HasValue ? ctx.Request.QueryString.Value : "";
                    ctx.Response.Redirect(normalized + q, permanent: true);
                    return;
                }

                await next();
            });

            // Statik dosyalar + uzun cache
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    // 1 yıl cache – dosya adında versiyon kullanırsan süper olur
                    ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=31536000,immutable";
                }
            });

            app.UseRouting();
            app.UseAuthorization();

            // robots.txt
            app.MapGet("/robots.txt", () =>
                Results.Text(
@"User-agent: *
Allow: /
Sitemap: https://www.hadimkoyankaranakliyat.com/sitemap.xml", "text/plain"));
            // ---- (5) sitemap.xml ----
            app.MapGet("/sitemap.xml", (HttpContext http) =>
            {
                http.Response.ContentType = "application/xml; charset=utf-8";
                var host = "https://www.hadimkoyankaranakliyat.com";

                // Tüm sayfaların listesi:
                var urls = new[]
                {
        $"{host}/",
        $"{host}/Home/Index",
        $"{host}/Home/hakkimizda",
        $"{host}/Home/iletisim",
        $"{host}/Home/fiyat_al",
        $"{host}/Home/hizmetlerimiz",
        $"{host}/Home/hizmet_turleri",
        $"{host}/Home/blog",

        // Hizmet sayfaları
        $"{host}/Home/hadimkoy_ankara_nakliyat",
        $"{host}/Home/sehirlerarasi_hizmetlerimiz",
        $"{host}/Home/evden_eve_nakliyat",
        $"{host}/Home/ambar_nakliyat",
        $"{host}/Home/fuar_tasimaciligi",
        $"{host}/Home/ofis_tasimaciligi",
        $"{host}/Home/parsiyel_nakliyat",
        $"{host}/Home/asansorlu_nakliyatin_istanbul_ankara_iliskisi",
        $"{host}/Home/evden_eve_nakliyat_oncesi_hazirlik_rehberi",

        // Şehir bazlı
        $"{host}/Home/istanbul_ankara_nakliyat",
        $"{host}/Home/istanbul_ankara_evden_eve_nakliyat",
        $"{host}/Home/istanbul_ankara_fuar_nakliyat",
        $"{host}/Home/istanbul_ankara_kamyonet_nakliyat",
        $"{host}/Home/istanbul_ankara_ambar_nakliyat",
        $"{host}/Home/istanbul_ankara_parsiyel_nakliyat",
        $"{host}/Home/istanbul_ankara_sehirlerarasi_nakliyat",
        $"{host}/Home/istanbul_ankara_ceyiz_nakliyat",
        $"{host}/Home/istanbul_ankara_nakliyat_fiyatlari_2025_guncel_rehber",

        // Diğer şehirler
        $"{host}/Home/istanbul_antalya_nakliyat",
        $"{host}/Home/istanbul_bursa_nakliyat",
        $"{host}/Home/istanbul_eskisehir_nakliyat",
        $"{host}/Home/istanbul_izmir_nakliyat",
        $"{host}/Home/kazan_istanbul_nakliyat",
        $"{host}/Home/catalca_ankara_nakliyat",
        $"{host}/Home/beylikduzu_ankara_nakliyat",
        $"{host}/Home/bayrampasa_ankara_nakliyat",
        $"{host}/Home/ikitelli_ankara_nakliyat",
        $"{host}/Home/sincan_istanbul_nakliyat",
        $"{host}/Home/temelli_istanbul_nakliyat",
        $"{host}/Home/yenimahalle_istanbul_nakliyat",
        $"{host}/Home/zeytinburnu_ankara_nakliyat",
        $"{host}/Home/ostim_istanbul_nakliyat",

        // Ekstra
        $"{host}/Home/nakliyat_hizmet_fiyati",
        $"{host}/Home/sigortali_nakliyat_esyalarinizi_guvence_altina_alin",
        $"{host}/Home/parsiyel_nakliyat_nedir_istanbul_ankara_arasinda__avantajlari"
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

            // MVC route
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();

            // --- Yardımcı: Türkçe karakterleri ve büyük harfleri normalize eder ---
            static string SlugifyPath(string path)
            {
                var lower = path.ToLowerInvariant();

                // Türkçe karakterleri ASCII'ye çevir
                lower = lower
                    .Replace('ğ', 'g')
                    .Replace('ü', 'u')
                    .Replace('ş', 's')
                    .Replace('ö', 'o')
                    .Replace('ç', 'c')
                    .Replace('ı', 'i');

                // Unicode işaretlerini temizle
                var normalized = lower.Normalize(NormalizationForm.FormD);
                var sb = new StringBuilder(normalized.Length);
                foreach (var ch in normalized)
                {
                    var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
                    if (cat != UnicodeCategory.NonSpacingMark) sb.Append(ch);
                }
                var noDiacritics = sb.ToString().Normalize(NormalizationForm.FormC);

                // Boşluk -> '-' ; yalnızca güvenli karakterler
                noDiacritics = noDiacritics.Replace(' ', '-');

                var safe = new StringBuilder(noDiacritics.Length);
                foreach (var c in noDiacritics)
                {
                    if (char.IsLetterOrDigit(c) || c == '/' || c == '-' || c == '_')
                        safe.Append(c);
                }
                return safe.ToString();
            }
        }
    }
}
