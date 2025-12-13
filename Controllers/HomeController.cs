using HadımkoyAnkaraNakliyat_WEB.Models;
using Microsoft.AspNetCore.Mvc;
using MimeKit;
using System.Net;
using System.Net.Mail;

namespace HadımkoyAnkaraNakliyat_WEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly IConfiguration _config;

        public HomeController(IConfiguration config)
        {
            _config = config;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]   
        public IActionResult hakkimizda()
        {
            return View();
        }

        [HttpPost]
        public IActionResult hakkimizda(TeklifFormModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Form verileri geçersiz.";
                return RedirectToAction("hakkimizda");
            }
            try
            {
                var smtp = _config.GetSection("Smtp");

                var body = $@"
                    <b>Ad Soyad:</b> {model.AdSoyad}<br>
                    <b>Email:</b> {model.Email}<br>
                    <b>Telefon:</b> {model.Telefon}<br>
                    <b>Tarih:</b> {model.Tarih}<br>
                    <b>Ağırlık:</b> {model.Agirlik}<br>
                    <b>Alınacak Şehir:</b> {model.AlinanSehir}<br>
                    <b>Taşınacak Şehir:</b> {model.TasinanSehir}<br>
                ";

                var mail = new MailMessage
                {
                    From = new MailAddress(smtp["UserName"]),
                    Subject = "[hakkimizda Teklif] Yeni Talep",
                    Body = body,
                    IsBodyHtml = true
                };

                mail.To.Add(smtp["ToEmail"]);

                var smtpClient = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"]))
                {
                    Credentials = new NetworkCredential(smtp["UserName"], smtp["Password"]),
                    EnableSsl = bool.Parse(smtp["EnableSSL"])
                };

                smtpClient.Send(mail);

                TempData["SuccessMessage"] = "Teklif talebiniz başarıyla gönderildi.";
                return RedirectToAction("hakkimizda");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hata: " + ex.Message;
                return RedirectToAction("hakkimizda");
            }
        }
        public IActionResult blog()
        {
            return View();
        }
        public IActionResult istanbul_ankara_nakliyat_fiyatlari_2025_guncel_rehber()
        {
            return View();
        }
        public IActionResult parsiyel_nakliyat_nedir_istanbul_ankara_arasinda__avantajlari()
        {
            return View();
        }
        public IActionResult evden_eve_nakliyat_oncesi_hazirlik_rehberi()
        {
            return View();
        }
        public IActionResult asansorlu_nakliyatin_istanbul_ankara_tasimalarinda_sagladigi_kolayliklar()
        {
            return View();
        }
        public IActionResult sigortali_nakliyat_esyalarinizi_guvence_altina_alin()
        {
            return View();
        }
        public IActionResult hizmetlerimiz()
        {
            return View();
        }

        public IActionResult hadimkoy_ankara_nakliyat()
        {
            return View();
        }

        [Route("istanbul-ankara-nakliyat")]
        public IActionResult istanbul_ankara_nakliyat()
        {
            return View();
        }


        public IActionResult istanbul_ankara_kamyonet_nakliyat()
        {
            return View();
        }

        public IActionResult istanbul_ankara_ambar_nakliyat()
        {
            return View();
        }

        public IActionResult istanbul_ankara_parsiyel_nakliyat()
        {
            return View();
        }

        public IActionResult istanbul_ankara_sehirlerarasi_nakliyat()
        {
            return View();
        }

        public IActionResult istanbul_ankara_evden_eve_nakliyat()
        {
            return View();
        }

        public IActionResult istanbul_ankara_ceyiz_nakliyat()
        {
            return View();
        }

        public IActionResult istanbul_ankara_fuar_nakliyat()
        {
            return View();
        }
        public IActionResult ikitelli_ankara_nakliyat()
        {
            return View();
        }
        public IActionResult catalca_ankara_nakliyat()
        {
            return View();
        }
        public IActionResult beylikduzu_ankara_nakliyat()
        {
            return View();
        }
        public IActionResult zeytinburnu_ankara_nakliyat()
        {
            return View();
        }
        public IActionResult bayrampasa_ankara_nakliyat()
        {
            return View();
        }
        public IActionResult ostim_istanbul_nakliyat()
        {
            return View();
        }
        public IActionResult yenimahalle_istanbul_nakliyat()
        {
            return View();
        }
        public IActionResult kazan_istanbul_nakliyat()
        {
            return View();
        }
        public IActionResult sincan_istanbul_nakliyat()
        {
            return View();
        }
        public IActionResult temelli_istanbul_nakliyat()
        {
            return View();
        }
        
        public IActionResult gebze_ankara_nakliyat()
        {
            return View();
        }
        //Hizmet Türlerinin bulunduğu sayfalar BAŞLANGIÇ//
        public IActionResult hizmet_turleri()
        {
            return View();
        }
        public IActionResult sehirlerarasi_nakliyat()
        {
            return View();
        }
        public IActionResult evden_eve_nakliyat()
        {
            return View();
        }
        public IActionResult ambar_nakliyat()
        {
            return View();
        }
        public IActionResult ofis_tasimaciligi()
        {
            return View();
        }
        public IActionResult fuar_tasimaciligi()
        {
            return View();
        }
        public IActionResult parsiyel_nakliyat()
        {
            return View();
        }
        //Hizmet Türlerinin bulunduğu sayfalar BAŞLANGIÇ//
        //Şehir Bazlı Taşımacılık Hizmetlerinin bulunduğu sayfalar BAŞLANGIÇ//
        public IActionResult sehir_bazli_tasimacilik()
        {
            return View();
        }
        [Route("ankara-nakliyat")]
        public IActionResult ankara_nakliyat()
        {
            return View();
        }
        [Route("istanbul-nakliyat")]
        public IActionResult istanbul_nakliyat()
        {
            return View();
        }
        //Şehir Bazlı Taşımacılık Hizmetlerinin bulunduğu sayfalar SON//

        //ilce bazlı taşımacılık hizmetlerinin bulunduğu sayfalar BAŞLANGIÇ//
        public IActionResult ilce_bazli_tasimacilik()
        {
            return View();
        }
        // ==========================================
        // İSTANBUL - ANKARA NAKLİYAT SAYFALARI
        // ==========================================

        // 1. ADALAR
        [Route("adalar-nakliyat")]
        public IActionResult adalar_nakliyat()
        {
            return View();
        }

        // 2. ARNAVUTKÖY
        [Route("arnavutkoy-nakliyat")]
        public IActionResult arnavutkoy_nakliyat()
        {
            return View();
        }

        // 3. ATAŞEHİR
        [Route("atasehir-nakliyat")]
        public IActionResult atasehir_nakliyat()
        {
            return View();
        }

        // 4. AVCILAR
        [Route("avcilar-nakliyat")]
        public IActionResult avcilar_nakliyat()
        {
            return View();
        }

        // 5. BAĞCILAR
        [Route("bagcilar-nakliyat")]
        public IActionResult bagcilar_nakliyat()
        {
            return View();
        }

        // 6. BAHÇELİEVLER
        [Route("bahcelievler-nakliyat")]
        public IActionResult bahcelievler_nakliyat()
        {
            return View();
        }

        // 7. BAKIRKÖY
        [Route("bakirkoy-nakliyat")]
        public IActionResult bakirkoy_nakliyat()
        {
            return View();
        }

        // 8. BAŞAKŞEHİR
        [Route("basaksehir-nakliyat")]
        public IActionResult basaksehir_nakliyat()
        {
            return View();
        }

        // 9. BAYRAMPAŞA
        [Route("bayrampasa-nakliyat")]
        public IActionResult bayrampasa_nakliyat()
        {
            return View();
        }

        // 10. BEŞİKTAŞ
        [Route("besiktas-nakliyat")]
        public IActionResult besiktas_nakliyat()
        {
            return View();
        }

        // 11. BEYKOZ
        [Route("beykoz-nakliyat")]
        public IActionResult beykoz_nakliyat()
        {
            return View();
        }

        // 12. BEYLİKDÜZÜ
        [Route("beylikduzu-nakliyat")]
        public IActionResult beylikduzu_nakliyat()
        {
            return View();
        }

        // 13. BEYOĞLU
        [Route("beyoglu-nakliyat")]
        public IActionResult beyoglu_nakliyat()
        {
            return View();
        }

        // 14. BÜYÜKÇEKMECE
        [Route("buyukcekmece-nakliyat")]
        public IActionResult buyukcekmece_nakliyat()
        {
            return View();
        }

        // 15. ÇATALCA
        [Route("catalca-nakliyat")]
        public IActionResult catalca_nakliyat()
        {
            return View();
        }

        // 16. ÇEKMEKÖY
        [Route("cekmekoy-nakliyat")]
        public IActionResult cekmekoy_nakliyat()
        {
            return View();
        }

        // 17. ESENLER
        [Route("esenler-nakliyat")]
        public IActionResult esenler_nakliyat()
        {
            return View();
        }

        // 18. ESENYURT
        [Route("esenyurt-nakliyat")]
        public IActionResult esenyurt_nakliyat()
        {
            return View();
        }

        // 19. EYÜPSULTAN
        [Route("eyupsultan-nakliyat")]
        public IActionResult eyupsultan_nakliyat()
        {
            return View();
        }

        // 20. FATİH
        [Route("fatih-nakliyat")]
        public IActionResult fatih_nakliyat()
        {
            return View();
        }

        // 21. GAZİOSMANPAŞA
        [Route("gaziosmanpasa-nakliyat")]
        public IActionResult gaziosmanpasa_nakliyat()
        {
            return View();
        }

        // 22. GÜNGÖREN
        [Route("gungoren-nakliyat")]
        public IActionResult gungoren_nakliyat()
        {
            return View();
        }

        // 23. KADIKÖY
        [Route("kadikoy-nakliyat")]
        public IActionResult kadikoy_nakliyat()
        {
            return View();
        }

        // 24. KAĞITHANE
        [Route("kagithane-nakliyat")]
        public IActionResult kagithane_nakliyat()
        {
            return View();
        }

        // 25. KARTAL
        [Route("kartal-nakliyat")]
        public IActionResult kartal_nakliyat()
        {
            return View();
        }

        // 26. KÜÇÜKÇEKMECE
        [Route("kucukcekmece-nakliyat")]
        public IActionResult kucukcekmece_nakliyat()
        {
            return View();
        }

        // 27. MALTEPE
        [Route("maltepe-nakliyat")]
        public IActionResult maltepe_nakliyat()
        {
            return View();
        }

        // 28. PENDİK
        [Route("pendik-nakliyat")]
        public IActionResult pendik_nakliyat()
        {
            return View();
        }

        // 29. SANCAKTEPE
        [Route("sancaktepe-nakliyat")]
        public IActionResult sancaktepe_nakliyat()
        {
            return View();
        }

        // 30. SARIYER
        [Route("sariyer-nakliyat")]
        public IActionResult sariyer_nakliyat()
        {
            return View();
        }

        // 31. SİLİVRİ
        [Route("silivri-nakliyat")]
        public IActionResult silivri_nakliyat()
        {
            return View();
        }

        // 32. SULTANBEYLİ
        [Route("sultanbeyli-nakliyat")]
        public IActionResult sultanbeyli_nakliyat()
        {
            return View();
        }

        // 33. SULTANGAZİ
        [Route("sultangazi-nakliyat")]
        public IActionResult sultangazi_nakliyat()
        {
            return View();
        }

        // 34. ŞİLE
        [Route("sile-nakliyat")]
        public IActionResult sile_nakliyat()
        {
            return View();
        }

        // 35. ŞİŞLİ
        [Route("sisli-nakliyat")]
        public IActionResult sisli_nakliyat()
        {
            return View();
        }

        // 36. TUZLA
        [Route("tuzla-nakliyat")]
        public IActionResult tuzla_nakliyat()
        {
            return View();
        }

        // 37. ÜMRANİYE
        [Route("umraniye-nakliyat")]
        public IActionResult umraniye_nakliyat()
        {
            return View();
        }

        // 38. ÜSKÜDAR
        [Route("uskudar-nakliyat")]
        public IActionResult uskudar_nakliyat()
        {
            return View();
        }

        // 39. ZEYTİNBURNU
        [Route("zeytinburnu-nakliyat")]
        public IActionResult zeytinburnu_nakliyat()
        {
            return View();
        }

        //ilce bazlı taşımacılık hizmetlerinin bulunduğu sayfalar Bitiş//

        [HttpGet]
        public IActionResult iletisim()
        {
            return View();
        }

        [HttpPost]
        public IActionResult iletisim(ContactFormModel model)
        {
            if (!ModelState.IsValid)
                return Content("Form verileri geçersiz.");

            try
            {
                var smtp = _config.GetSection("Smtp");

                var mail = new MailMessage
                {
                    From = new MailAddress(smtp["UserName"]),
                    Subject = $"[İletişim] {model.Subject}",
                    Body = $"Ad: {model.Name}\nEmail: {model.Email}\nTelefon: {model.Phone}\n\nMesaj:\n{model.Message}",
                    IsBodyHtml = false
                };

                mail.To.Add(smtp["ToEmail"]);

                var smtpClient = new SmtpClient(smtp["Host"], int.Parse(smtp["Port"]))
                {
                    Credentials = new NetworkCredential(smtp["UserName"], smtp["Password"]),
                    EnableSsl = bool.Parse(smtp["EnableSSL"])
                };

                smtpClient.Send(mail);

                TempData["SuccessMessage"] = "Mesajınız başarıyla gönderildi.";
                return RedirectToAction("iletisim");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hata: " + ex.Message;
                return RedirectToAction("iletisim");
            }
        }
        public IActionResult sehirlerarasi_hizmetlerimiz()
        {
            return View();
        }
        public IActionResult istanbul_izmir_nakliyat()
        {
            return View();
        }
        public IActionResult istanbul_bursa_nakliyat()
        {
            return View();
        }
        public IActionResult istanbul_eskisehir_nakliyat()
        {
            return View();
        }
        public IActionResult istanbul_antalya_nakliyat()
        {
            return View();
        }
        public IActionResult nakliyat_hizmet_fiyati()
        {
            return View();
        }
        public IActionResult fiyat_al()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> fiyat_al(TeklifFormModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse("hadimkoyankaranakliyat@gmail.com"));
            email.To.Add(MailboxAddress.Parse("hadimkoyankaranakliyat@gmail.com"));
            email.Subject = "Yeni Teklif Formu";

            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = $@"
            <h2>Teklif Talebi</h2>
            <p><strong>Ad Soyad:</strong> {model.AdSoyad}</p>
            <p><strong>Email:</strong> {model.Email}</p>
            <p><strong>Telefon:</strong> {model.Telefon}</p>
            <p><strong>Tarih:</strong> {model.Tarih}</p>
            <p><strong>Ağırlık:</strong> {model.Agirlik}</p>
            <p><strong>Alınacak Şehir:</strong> {model.AlinanSehir}</p>
            <p><strong>Taşınacak Şehir:</strong> {model.TasinanSehir}</p>"
            };
            email.Body = bodyBuilder.ToMessageBody();

            try
            {
                using (var smtp = new MailKit.Net.Smtp.SmtpClient())
                {
                    await smtp.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
                    await smtp.AuthenticateAsync("hadimkoyankaranakliyat@gmail.com", "qxco wuch zvtl rbxe"); // Uygulama şifresi
                    await smtp.SendAsync(email);
                    await smtp.DisconnectAsync(true);
                }
                TempData["SuccessMessage"] = "Teklifiniz başarıyla iletildi! En kısa sürede sizinle iletişime geçeceğiz.";
                return RedirectToAction("fiyat_al");
            }
            catch (Exception ex)
            {
                // İstersen ViewBag veya TempData ile hata mesajı dönebilirsin
                TempData["ErrorMessage"] = "Bir hata oluştu. Lütfen tekrar deneyin veya bizimle iletişime geçin.";
                // Hatanın detayını loglaman önerilir!
                return View(model);
            }
        }
    }
}
