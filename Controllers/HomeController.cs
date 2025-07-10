using HadımkoyAnkaraNakliyat_WEB.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
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

        public IActionResult Hakkımızda()
        {
            return View();
        }

        public IActionResult Hizmetlerimiz()
        {
            return View();
        }
        public IActionResult Hadimköy_Ankara_Nakliyat()
        {
            return View();
        }
        public IActionResult İstanbul_Ankara_Nakliyat()
        {
            return View();
        }
        public IActionResult İstanbul_Ankara_Kamyonet_Nakliyat()
        {
            return View();
        }
        public IActionResult İstanbul_Ankara_Ambar_Nakliyat()
        {
            return View();
        }
        public IActionResult İstanbul_Ankara_Parsiyel_Nakliyat()
        {
            return View();
        }
        public IActionResult İstanbul_Ankara_Şehirlerarasi_Nakliyat()
        {
            return View();
        }
        public IActionResult İstanbul_Ankara_Evden_Eve_Nakliyat()
        {
            return View();
        }
        public IActionResult İstanbul_Ankara_Ceyiz_Nakliyat()
        {
            return View();
        }
        public IActionResult İstanbul_Ankara_Fuar_Nakliyat()
        {
            return View();
        }
        [HttpGet]
        public IActionResult İletisim()
        {
            return View();
        }
        [HttpPost]
        public IActionResult İletisim(ContactFormModel model)
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
                return RedirectToAction("İletisim");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Hata: " + ex.Message;
                return RedirectToAction("İletisim");
            }
        }
    }

}