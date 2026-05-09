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
            // GÜVENLİK BAŞLIKLARI
            // -----------------------------------------------------------------
            app.Use(async (ctx, next) =>
            {
                ctx.Response.Headers["X-Content-Type-Options"] = "nosniff";
                ctx.Response.Headers["X-Frame-Options"]        = "SAMEORIGIN";
                ctx.Response.Headers["Referrer-Policy"]        = "strict-origin-when-cross-origin";
                ctx.Response.Headers["Permissions-Policy"]     = "camera=(), microphone=(), geolocation=()";
                await next();
            });

            // -----------------------------------------------------------------
            // 1. ADIM: www Yönlendirmesi (hadimkoyankaranakliyat.com → www.hadimkoyankaranakliyat.com)
            // Google non-www ve www'yu farklı site olarak görür; canonical karışıklığını önler.
            // -----------------------------------------------------------------
            app.Use(async (ctx, next) =>
            {
                var host = ctx.Request.Host.Host ?? "";
                if (host.Length > 0 && !host.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
                    && !host.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                    && !host.StartsWith("127.") && !host.StartsWith("::1"))
                {
                    var newUrl = $"https://www.{ctx.Request.Host.Value}{ctx.Request.Path}{ctx.Request.QueryString}";
                    ctx.Response.Redirect(newUrl, permanent: true);
                    return;
                }
                await next();
            });

            // -----------------------------------------------------------------
            // 2. ADIM: URL Normalizasyonu (Büyük harf -> Küçük harf çevirici)
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
                // Ordinal (büyük/küçük harf duyarlı): /Home/Index → /home/index redirect atar.
                if (!string.Equals(original, normalized, StringComparison.Ordinal))
                {
                    var q = ctx.Request.QueryString.HasValue ? ctx.Request.QueryString.Value : "";
                    ctx.Response.Redirect(normalized + q, permanent: true);
                    return;
                }

                await next();
            });

            // -----------------------------------------------------------------
            // 3. ADIM: WebP Middleware
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
            // 4. ADIM: Statik Dosyalar (CSS, Resimler çalışsın diye ŞART)
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
            // 5. ADIM: URL Yönlendirmeleri
            // -----------------------------------------------------------------
            app.Use(async (context, next) =>
            {
                var path = context.Request.Path.Value;

                // YÖNLENDİRME SÖZLÜĞÜ: Eski URL -> Yeni URL
                var redirects = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        // Index canonical fix
        {"/home/index", "/"},
        {"/home/", "/"},
        {"/home/privacy", "/kvkk"},

        // Ana hizmetler (eski /home/ → clean slug)
        {"/home/istanbul_ankara_nakliyat", "/istanbul-ankara-nakliyat"},
        {"/home/istanbul_nakliyat", "/istanbul-nakliyat"},
        {"/home/ankara_nakliyat", "/ankara-nakliyat"},
        {"/home/zeytinburnu_nakliyat", "/zeytinburnu-nakliyat"},
        {"/home/esenyurt_nakliyat", "/esenyurt-nakliyat"},
        {"/home/istanbul_ankara_nakliyat_fiyatlari_2026_guncel_rehber", "/istanbul-ankara-nakliyat-fiyatlari-2026-guncel-rehber"},

        // 39 İlçe sayfaları (hepsi [Route] ile tanımlı, /home/xxx 404 veriyordu)
        {"/home/adalar_nakliyat", "/adalar-nakliyat"},
        {"/home/arnavutkoy_nakliyat", "/arnavutkoy-nakliyat"},
        {"/home/atasehir_nakliyat", "/atasehir-nakliyat"},
        {"/home/avcilar_nakliyat", "/avcilar-nakliyat"},
        {"/home/bagcilar_nakliyat", "/bagcilar-nakliyat"},
        {"/home/bahcelievler_nakliyat", "/bahcelievler-nakliyat"},
        {"/home/bakirkoy_nakliyat", "/bakirkoy-nakliyat"},
        {"/home/basaksehir_nakliyat", "/basaksehir-nakliyat"},
        {"/home/bayrampasa_nakliyat", "/bayrampasa-nakliyat"},
        {"/home/besiktas_nakliyat", "/besiktas-nakliyat"},
        {"/home/beykoz_nakliyat", "/beykoz-nakliyat"},
        {"/home/beylikduzu_nakliyat", "/beylikduzu-nakliyat"},
        {"/home/beyoglu_nakliyat", "/beyoglu-nakliyat"},
        {"/home/buyukcekmece_nakliyat", "/buyukcekmece-nakliyat"},
        {"/home/catalca_nakliyat", "/catalca-nakliyat"},
        {"/home/cekmekoy_nakliyat", "/cekmekoy-nakliyat"},
        {"/home/esenler_nakliyat", "/esenler-nakliyat"},
        {"/home/eyupsultan_nakliyat", "/eyupsultan-nakliyat"},
        {"/home/fatih_nakliyat", "/fatih-nakliyat"},
        {"/home/gaziosmanpasa_nakliyat", "/gaziosmanpasa-nakliyat"},
        {"/home/gungoren_nakliyat", "/gungoren-nakliyat"},
        {"/home/kadikoy_nakliyat", "/kadikoy-nakliyat"},
        {"/home/kagithane_nakliyat", "/kagithane-nakliyat"},
        {"/home/kartal_nakliyat", "/kartal-nakliyat"},
        {"/home/kucukcekmece_nakliyat", "/kucukcekmece-nakliyat"},
        {"/home/maltepe_nakliyat", "/maltepe-nakliyat"},
        {"/home/pendik_nakliyat", "/pendik-nakliyat"},
        {"/home/sancaktepe_nakliyat", "/sancaktepe-nakliyat"},
        {"/home/sariyer_nakliyat", "/sariyer-nakliyat"},
        {"/home/silivri_nakliyat", "/silivri-nakliyat"},
        {"/home/sultanbeyli_nakliyat", "/sultanbeyli-nakliyat"},
        {"/home/sultangazi_nakliyat", "/sultangazi-nakliyat"},
        {"/home/sile_nakliyat", "/sile-nakliyat"},
        {"/home/sisli_nakliyat", "/sisli-nakliyat"},
        {"/home/tuzla_nakliyat", "/tuzla-nakliyat"},
        {"/home/umraniye_nakliyat", "/umraniye-nakliyat"},
        {"/home/uskudar_nakliyat", "/uskudar-nakliyat"},

        // Parça eşya ve özel sayfalar
        {"/home/parca_esya_tasima", "/parca-esya-tasima"},
        {"/home/istanbul_ankara_parca_esya_tasima", "/istanbul-ankara-parca-esya-tasima"},
        {"/home/sehirlerarasi_parca_esya_tasima_fiyatlari", "/sehirlerarasi-parca-esya-tasima-fiyatlari"},
        {"/home/istanbul_ankara_nakliye", "/istanbul-ankara-nakliye"},
        {"/home/ankara_istanbul_nakliyat", "/ankara-istanbul-nakliyat"},
        {"/home/asansorlu_nakliyat", "/asansorlu-nakliyat"},
        {"/home/sigortali_nakliyat", "/sigortali-nakliyat"},
        {"/home/nakliyat_fiyat_hesaplama", "/nakliyat-fiyat-hesaplama"},

        // İlçe fiyat blog sayfaları
        {"/home/kadikoy_evden_eve_nakliyat_fiyatlari", "/kadikoy-evden-eve-nakliyat-fiyatlari"},
        {"/home/besiktas_evden_eve_nakliyat_fiyatlari", "/besiktas-evden-eve-nakliyat-fiyatlari"},
        {"/home/uskudar_evden_eve_nakliyat_fiyatlari", "/uskudar-evden-eve-nakliyat-fiyatlari"},
        {"/home/sisli_evden_eve_nakliyat_fiyatlari", "/sisli-evden-eve-nakliyat-fiyatlari"},
        {"/home/bakirkoy_evden_eve_nakliyat_fiyatlari", "/bakirkoy-evden-eve-nakliyat-fiyatlari"},
        {"/home/maltepe_evden_eve_nakliyat_fiyatlari", "/maltepe-evden-eve-nakliyat-fiyatlari"},
        {"/home/umraniye_evden_eve_nakliyat_fiyatlari", "/umraniye-evden-eve-nakliyat-fiyatlari"},
        {"/home/beylikduzu_evden_eve_nakliyat_fiyatlari", "/beylikduzu-evden-eve-nakliyat-fiyatlari"},
        {"/home/pendik_evden_eve_nakliyat_fiyatlari", "/pendik-evden-eve-nakliyat-fiyatlari"},
        {"/home/fatih_evden_eve_nakliyat_fiyatlari", "/fatih-evden-eve-nakliyat-fiyatlari"},

        // Eşya depolama
        {"/home/esya_depolama", "/esya-depolama"},

        // İlçe fiyat blog sayfaları (29 ilçe)
        {"/home/adalar_evden_eve_nakliyat_fiyatlari", "/adalar-evden-eve-nakliyat-fiyatlari"},
        {"/home/arnavutkoy_evden_eve_nakliyat_fiyatlari", "/arnavutkoy-evden-eve-nakliyat-fiyatlari"},
        {"/home/atasehir_evden_eve_nakliyat_fiyatlari", "/atasehir-evden-eve-nakliyat-fiyatlari"},
        {"/home/avcilar_evden_eve_nakliyat_fiyatlari", "/avcilar-evden-eve-nakliyat-fiyatlari"},
        {"/home/bagcilar_evden_eve_nakliyat_fiyatlari", "/bagcilar-evden-eve-nakliyat-fiyatlari"},
        {"/home/bahcelievler_evden_eve_nakliyat_fiyatlari", "/bahcelievler-evden-eve-nakliyat-fiyatlari"},
        {"/home/basaksehir_evden_eve_nakliyat_fiyatlari", "/basaksehir-evden-eve-nakliyat-fiyatlari"},
        {"/home/bayrampasa_evden_eve_nakliyat_fiyatlari", "/bayrampasa-evden-eve-nakliyat-fiyatlari"},
        {"/home/beykoz_evden_eve_nakliyat_fiyatlari", "/beykoz-evden-eve-nakliyat-fiyatlari"},
        {"/home/beyoglu_evden_eve_nakliyat_fiyatlari", "/beyoglu-evden-eve-nakliyat-fiyatlari"},
        {"/home/buyukcekmece_evden_eve_nakliyat_fiyatlari", "/buyukcekmece-evden-eve-nakliyat-fiyatlari"},
        {"/home/catalca_evden_eve_nakliyat_fiyatlari", "/catalca-evden-eve-nakliyat-fiyatlari"},
        {"/home/cekmekoy_evden_eve_nakliyat_fiyatlari", "/cekmekoy-evden-eve-nakliyat-fiyatlari"},
        {"/home/esenler_evden_eve_nakliyat_fiyatlari", "/esenler-evden-eve-nakliyat-fiyatlari"},
        {"/home/esenyurt_evden_eve_nakliyat_fiyatlari", "/esenyurt-evden-eve-nakliyat-fiyatlari"},
        {"/home/eyupsultan_evden_eve_nakliyat_fiyatlari", "/eyupsultan-evden-eve-nakliyat-fiyatlari"},
        {"/home/gaziosmanpasa_evden_eve_nakliyat_fiyatlari", "/gaziosmanpasa-evden-eve-nakliyat-fiyatlari"},
        {"/home/gungoren_evden_eve_nakliyat_fiyatlari", "/gungoren-evden-eve-nakliyat-fiyatlari"},
        {"/home/kagithane_evden_eve_nakliyat_fiyatlari", "/kagithane-evden-eve-nakliyat-fiyatlari"},
        {"/home/kartal_evden_eve_nakliyat_fiyatlari", "/kartal-evden-eve-nakliyat-fiyatlari"},
        {"/home/kucukcekmece_evden_eve_nakliyat_fiyatlari", "/kucukcekmece-evden-eve-nakliyat-fiyatlari"},
        {"/home/sancaktepe_evden_eve_nakliyat_fiyatlari", "/sancaktepe-evden-eve-nakliyat-fiyatlari"},
        {"/home/sariyer_evden_eve_nakliyat_fiyatlari", "/sariyer-evden-eve-nakliyat-fiyatlari"},
        {"/home/silivri_evden_eve_nakliyat_fiyatlari", "/silivri-evden-eve-nakliyat-fiyatlari"},
        {"/home/sultanbeyli_evden_eve_nakliyat_fiyatlari", "/sultanbeyli-evden-eve-nakliyat-fiyatlari"},
        {"/home/sultangazi_evden_eve_nakliyat_fiyatlari", "/sultangazi-evden-eve-nakliyat-fiyatlari"},
        {"/home/sile_evden_eve_nakliyat_fiyatlari", "/sile-evden-eve-nakliyat-fiyatlari"},
        {"/home/tuzla_evden_eve_nakliyat_fiyatlari", "/tuzla-evden-eve-nakliyat-fiyatlari"},
        {"/home/zeytinburnu_evden_eve_nakliyat_fiyatlari", "/zeytinburnu-evden-eve-nakliyat-fiyatlari"},
        {"/home/kentsel_donusum_nakliyat", "/kentsel-donusum-nakliyat"},
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
            // 6. ADIM: robots.txt
            // -----------------------------------------------------------------
            app.MapGet("/robots.txt", () => Results.Text(
@"User-agent: *
Allow: /
Sitemap: https://www.hadimkoyankaranakliyat.com/sitemap.xml", "text/plain"));

            // -----------------------------------------------------------------
            // 7. ADIM: SITEMAP (Google için Harita)
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
        $"{host}/kvkk",
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
        $"{host}/home/ofis_tasima_rehberi_2026",
        $"{host}/home/esya_paketleme_teknikleri",
        $"{host}/home/nakliyat_sozlesmesi_nedir",
        $"{host}/home/ankaraya_tasinmak_rehberi",
        $"{host}/home/tasinma_maliyetleri_nasil_dusurulur",

        // İlçe bazlı fiyat blog sayfaları (39 ilçe tam liste)
        $"{host}/kadikoy-evden-eve-nakliyat-fiyatlari",
        $"{host}/besiktas-evden-eve-nakliyat-fiyatlari",
        $"{host}/uskudar-evden-eve-nakliyat-fiyatlari",
        $"{host}/sisli-evden-eve-nakliyat-fiyatlari",
        $"{host}/bakirkoy-evden-eve-nakliyat-fiyatlari",
        $"{host}/maltepe-evden-eve-nakliyat-fiyatlari",
        $"{host}/umraniye-evden-eve-nakliyat-fiyatlari",
        $"{host}/beylikduzu-evden-eve-nakliyat-fiyatlari",
        $"{host}/pendik-evden-eve-nakliyat-fiyatlari",
        $"{host}/fatih-evden-eve-nakliyat-fiyatlari",
        $"{host}/adalar-evden-eve-nakliyat-fiyatlari",
        $"{host}/arnavutkoy-evden-eve-nakliyat-fiyatlari",
        $"{host}/atasehir-evden-eve-nakliyat-fiyatlari",
        $"{host}/avcilar-evden-eve-nakliyat-fiyatlari",
        $"{host}/bagcilar-evden-eve-nakliyat-fiyatlari",
        $"{host}/bahcelievler-evden-eve-nakliyat-fiyatlari",
        $"{host}/basaksehir-evden-eve-nakliyat-fiyatlari",
        $"{host}/bayrampasa-evden-eve-nakliyat-fiyatlari",
        $"{host}/beykoz-evden-eve-nakliyat-fiyatlari",
        $"{host}/beyoglu-evden-eve-nakliyat-fiyatlari",
        $"{host}/buyukcekmece-evden-eve-nakliyat-fiyatlari",
        $"{host}/catalca-evden-eve-nakliyat-fiyatlari",
        $"{host}/cekmekoy-evden-eve-nakliyat-fiyatlari",
        $"{host}/esenler-evden-eve-nakliyat-fiyatlari",
        $"{host}/esenyurt-evden-eve-nakliyat-fiyatlari",
        $"{host}/eyupsultan-evden-eve-nakliyat-fiyatlari",
        $"{host}/gaziosmanpasa-evden-eve-nakliyat-fiyatlari",
        $"{host}/gungoren-evden-eve-nakliyat-fiyatlari",
        $"{host}/kagithane-evden-eve-nakliyat-fiyatlari",
        $"{host}/kartal-evden-eve-nakliyat-fiyatlari",
        $"{host}/kucukcekmece-evden-eve-nakliyat-fiyatlari",
        $"{host}/sancaktepe-evden-eve-nakliyat-fiyatlari",
        $"{host}/sariyer-evden-eve-nakliyat-fiyatlari",
        $"{host}/silivri-evden-eve-nakliyat-fiyatlari",
        $"{host}/sultanbeyli-evden-eve-nakliyat-fiyatlari",
        $"{host}/sultangazi-evden-eve-nakliyat-fiyatlari",
        $"{host}/sile-evden-eve-nakliyat-fiyatlari",
        $"{host}/tuzla-evden-eve-nakliyat-fiyatlari",
        $"{host}/zeytinburnu-evden-eve-nakliyat-fiyatlari",
        $"{host}/kentsel-donusum-nakliyat",

        // Eşya Depolama
        $"{host}/esya-depolama",

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
                var lastmod = "2026-05-10";
                foreach (var u in urls)
                {
                    // Ana sayfa: 1.0, ilce sayfalari: 0.9, diger: 0.8
                    string priority;
                    if (u == host + "/")
                        priority = "1.0";
                    else if (u.Contains("-nakliyat") && !u.Contains("/home/"))
                        priority = "0.9";
                    else
                        priority = "0.8";

                    sb.AppendLine("  <url>");
                    sb.AppendLine($"    <loc>{u}</loc>");
                    sb.AppendLine($"    <lastmod>{lastmod}</lastmod>");
                    sb.AppendLine("    <changefreq>weekly</changefreq>");
                    sb.AppendLine($"    <priority>{priority}</priority>");
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