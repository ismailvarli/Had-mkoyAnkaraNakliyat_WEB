using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using WebOptimizer;

namespace HadımkoyAnkaraNakliyat_WEB
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // MVC Servisleri
            builder.Services.AddControllersWithViews();

            // Bundle + Minify (43 CSS + 26 JS → 1+1 dosya)
            builder.Services.AddWebOptimizer(pipeline =>
            {
                // ── CSS BUNDLE ──────────────────────────────────────────────
                // Orijinal _Layout.cshtml yükleme sırası korundu
                pipeline.AddCssBundle("/css/bundle.css",
                    // Module CSS
                    "assets/css/module-css/01-slider.css",
                    "assets/css/module-css/02-about.css",
                    "assets/css/module-css/03-services.css",
                    "assets/css/module-css/04-testimonial.css",
                    "assets/css/module-css/05-team.css",
                    "assets/css/module-css/06-blog.css",
                    "assets/css/module-css/07-brand.css",
                    "assets/css/module-css/08-contact.css",
                    "assets/css/module-css/09-counter.css",
                    "assets/css/module-css/10-error.css",
                    "assets/css/module-css/11-faq.css",
                    "assets/css/module-css/12-footer.css",
                    "assets/css/module-css/13-page-header.css",
                    "assets/css/module-css/14-shop.css",
                    "assets/css/module-css/15-video.css",
                    "assets/css/module-css/awards.css",
                    "assets/css/module-css/banner.css",
                    "assets/css/module-css/cta.css",
                    "assets/css/module-css/design-interior.css",
                    "assets/css/module-css/feature.css",
                    "assets/css/module-css/pricing.css",
                    "assets/css/module-css/projects.css",
                    "assets/css/module-css/quote.css",
                    "assets/css/module-css/skill.css",
                    "assets/css/module-css/sliding-text.css",
                    "assets/css/module-css/why-choose.css",
                    "assets/css/module-css/working-process.css",
                    // Library CSS
                    "assets/css/swiper.min.css",
                    "assets/css/style.css",
                    "assets/css/responsive.css",
                    "assets/css/01-bootstrap.min.css",
                    "assets/css/02-animate.min.css",
                    "assets/css/03-custom-animate.css",
                    "assets/css/05-flaticon.css",
                    "assets/css/06-font-awesome-all.css",
                    "assets/css/07-jarallax.css",
                    "assets/css/08-jquery.magnific-popup.css",
                    "assets/css/09-nice-select.css",
                    "assets/css/11-owl.carousel.min.css",
                    "assets/css/12-owl.theme.default.min.css",
                    "assets/css/13-jquery-ui.css"
                );

                // ── JS BUNDLE ───────────────────────────────────────────────
                // jQuery en başta, script.js en sonda — sıra kritik
                pipeline.AddJavaScriptBundle("/js/bundle.js",
                    "assets/js/jquery-3.6.0.min.js",
                    "assets/js/jquery.ajaxchimp.min.js",
                    "assets/js/jquery.validate.min.js",
                    "assets/js/swiper.min.js",
                    "assets/js/wNumb.min.js",
                    "assets/js/curved-text/jquery.circleType.js",
                    "assets/js/curved-text/jquery.fittext.js",
                    "assets/js/curved-text/jquery.lettering.min.js",
                    "assets/js/gsap/gsap.js",
                    "assets/js/gsap/ScrollTrigger.js",
                    "assets/js/gsap/SplitText.js",
                    "assets/js/01-bootstrap.bundle.min.js",
                    "assets/js/02-countdown.min.js",
                    "assets/js/03-jquery.appear.min.js",
                    "assets/js/04-jquery.nice-select.min.js",
                    "assets/js/05-jquery-sidebar-content.js",
                    "assets/js/06-marquee.min.js",
                    "assets/js/07-owl.carousel.min.js",
                    "assets/js/08-jarallax.min.js",
                    "assets/js/09-odometer.min.js",
                    "assets/js/10-jquery-ui.js",
                    "assets/js/11-jquery.magnific-popup.min.js",
                    "assets/js/12-wow.js",
                    "assets/js/13-isotope.js",
                    "assets/js/script.js"
                );
            });

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
            app.UseWebOptimizer();

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
                // OrdinalIgnoreCase: sadece Türkçe karakter farkı varsa redirect at,
                // /Home/ -> /home/ gibi sadece büyük/küçük harf farkı olunca atma.
                if (!string.Equals(original, normalized, StringComparison.OrdinalIgnoreCase))
                {
                    var q = ctx.Request.QueryString.HasValue ? ctx.Request.QueryString.Value : "";
                    ctx.Response.Redirect(normalized + q, permanent: true);
                    return;
                }

                await next();
            });

            // -----------------------------------------------------------------
            // 2. ADIM: WebP Middleware
            // Tarayıcı WebP destekliyorsa PNG/JPG yerine otomatik WebP servis eder.
            // .cshtml dosyalarına dokunmaya gerek yok.
            // -----------------------------------------------------------------
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value ?? "";

                if (path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    var accept = context.Request.Headers["Accept"].ToString();
                    if (accept.Contains("image/webp"))
                    {
                        var webpPath = Path.ChangeExtension(path, ".webp");
                        var webpFile = Path.Combine(app.Environment.WebRootPath,
                                                    webpPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                        if (File.Exists(webpFile))
                        {
                            context.Request.Path = webpPath;
                            context.Response.Headers["Vary"] = "Accept";
                        }
                    }
                }

                await next();
            });

            // -----------------------------------------------------------------
            // 3. ADIM: Statik Dosyalar (CSS, Resimler çalışsın diye ŞART)
            // -----------------------------------------------------------------
            // Statik dosyalar (resim, css, js) için Gelişmiş Cache Ayarı
            app.UseStaticFiles(new StaticFileOptions
            {
                OnPrepareResponse = ctx =>
                {
                    // Dosyaları tarayıcıda 1 yıl (31536000 saniye) önbelleğe al
                    // "immutable" sayesinde dosya değişmedikçe tarayıcı tekrar sormaz.
                    ctx.Context.Response.Headers["Cache-Control"] = "public,max-age=31536000,immutable";
                }
            });

            // -----------------------------------------------------------------
            // 4. ADIM: URL Yönlendirmeleri
            // -----------------------------------------------------------------
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value;

                // YÖNLENDİRME SÖZLÜĞÜ: Eski URL -> Yeni URL
                // Buraya tüm eski URL'leri ve yeni karşılıklarını ekleyin.
                var redirects = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        // 1. ANA HİZMETLER
        {"/home/istanbul_ankara_nakliyat", "/istanbul-ankara-nakliyat"},
        {"/home/zeytinburnu_nakliyat", "/zeytinburnu-nakliyat"},
        {"/home/istanbul_nakliyat", "/istanbul-nakliyat"},
        {"/home/esenyurt_nakliyat", "/esenyurt-nakliyat"},
        {"/home/ankara_nakliyat", "/ankara-nakliyat"},
    };

                if (redirects.TryGetValue(path, out var newUrl))
                {
                    // 301 Kalıcı Yönlendirme
                    context.Response.Redirect(newUrl, permanent: true);
                    return;
                }

                await next();
            });
            app.UseRouting();
            app.UseAuthorization();

            // -----------------------------------------------------------------
            // 5. ADIM: robots.txt
            // -----------------------------------------------------------------
            app.MapGet("/robots.txt", () => Results.Text(
@"User-agent: *
Allow: /
Sitemap: https://www.hadimkoyankaranakliyat.com/sitemap.xml", "text/plain"));

            // -----------------------------------------------------------------
            // 6. ADIM: SITEMAP (Google için Harita)
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
        $"{host}/home/nakliyat_hizmet_fiyati",
        $"{host}/home/musteriyorumlari",
        $"{host}/home/sehirlerarasi_hizmetlerimiz",
        $"{host}/parca-esya-tasima",
        $"{host}/istanbul-ankara-parca-esya-tasima",
        $"{host}/sehirlerarasi-parca-esya-tasima-fiyatlari",

        //blog
        $"{host}/home/istanbul_ankara_nakliyat_fiyatlari_2025_guncel_rehber",
        $"{host}/istanbul-ankara-nakliyat-fiyatlari-2026-guncel-rehber",
        $"{host}/home/parsiyel_nakliyat_nedir_istanbul_ankara_arasinda__avantajlari",
        $"{host}/home/evden_eve_nakliyat_oncesi_hazirlik_rehberi",
        $"{host}/home/asansorlu_nakliyatin_istanbul_ankara_tasimalarinda_sagladigi_kolayliklar",
        $"{host}/home/sigortali_nakliyat_esyalarinizi_guvence_altina_alin",
        $"{host}/home/nakliyat_firmasi_nasil_secilir_7_kritik_soru",
        $"{host}/home/tasinirken_yapilan_10_hata",
        $"{host}/home/parca_esya_mi_tam_arac_mi",
        $"{host}/home/nakliyat_dolandiriciligi",
        $"{host}/home/tasinma_gunu_kontrol_listesi",


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