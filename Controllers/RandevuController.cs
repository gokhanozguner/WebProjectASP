using GokhanOzgunerWEB.Data;
using GokhanOzgunerWEB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FitnessYonetimi.Controllers
{
    [Authorize]
    public class RandevuController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RandevuController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Admin: Tüm randevular
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var randevular = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .OrderByDescending(r => r.RandevuTarihi)
                .ToListAsync();
            return View(randevular);
        }

        // Üye: Kendi randevuları
        public async Task<IActionResult> MyAppointments()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var randevular = await _context.Randevular
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Where(r => r.UyeId == userId)
                .OrderByDescending(r => r.RandevuTarihi)
                .ToListAsync();
            return View(randevular);
        }

        // GET: Randevu/Create
        public IActionResult Create()
        {
            ViewData["SalonId"] = new SelectList(_context.Salonlar.Where(s => s.Aktif), "Id", "Ad");
            return View();
        }

        // AJAX: Salona göre hizmetleri getir
        [HttpGet]
        public async Task<IActionResult> GetHizmetlerBySalon(int salonId)
        {
            var hizmetler = await _context.Hizmetler
                .Where(h => h.SalonId == salonId && h.Aktif)
                .Select(h => new { h.Id, h.Ad, h.Sure, h.Ucret })
                .ToListAsync();
            return Json(hizmetler);
        }

        // AJAX: Salona göre antrenörleri getir
        [HttpGet]
        public async Task<IActionResult> GetAntrenorlerBySalon(int salonId)
        {
            var antrenorler = await _context.Antrenorler
                .Where(a => a.SalonId == salonId && a.Aktif)
                .Select(a => new { a.Id, a.AdSoyad })
                .ToListAsync();
            return Json(antrenorler);
        }

        // AJAX: Uygun saatleri getir
        [HttpGet]
        public async Task<IActionResult> GetUygunSaatler(int antrenorId, DateTime tarih, int hizmetId)
        {
            var hizmet = await _context.Hizmetler.FindAsync(hizmetId);
            if (hizmet == null) return Json(new { success = false });

            var gun = tarih.DayOfWeek;
            var musaitlikler = await _context.AntrenorMusaitlikler
                .Where(m => m.AntrenorId == antrenorId && m.Gun == gun && m.Aktif)
                .ToListAsync();

            if (!musaitlikler.Any())
                return Json(new { success = false, message = "Bu gün için müsaitlik yok" });

            // Mevcut randevuları al
            var mevcutRandevular = await _context.Randevular
                .Where(r => r.AntrenorId == antrenorId
                    && r.RandevuTarihi.Date == tarih.Date
                    && r.Durum != RandevuDurum.IptalEdildi
                    && r.Durum != RandevuDurum.Reddedildi)
                .ToListAsync();

            var uygunSaatler = new List<string>();

            foreach (var musaitlik in musaitlikler)
            {
                var baslangic = musaitlik.BaslangicSaati;
                var bitis = musaitlik.BitisSaati;
                var hizmetSuresi = TimeSpan.FromMinutes(hizmet.Sure);

                var suankiSaat = baslangic;
                while (suankiSaat.Add(hizmetSuresi) <= bitis)
                {
                    var randevuBitis = suankiSaat.Add(hizmetSuresi);

                    // Çakışma kontrolü
                    var cakismaVar = mevcutRandevular.Any(r =>
                        (suankiSaat >= r.BaslangicSaati && suankiSaat < r.BitisSaati) ||
                        (randevuBitis > r.BaslangicSaati && randevuBitis <= r.BitisSaati) ||
                        (suankiSaat <= r.BaslangicSaati && randevuBitis >= r.BitisSaati)
                    );

                    if (!cakismaVar)
                    {
                        uygunSaatler.Add(suankiSaat.ToString(@"hh\:mm"));
                    }

                    suankiSaat = suankiSaat.Add(TimeSpan.FromMinutes(30)); // 30 dk aralıklarla
                }
            }

            return Json(new { success = true, saatler = uygunSaatler });
        }

        // POST: Randevu/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int salonId, int hizmetId, int antrenorId, DateTime randevuTarihi, string baslangicSaati)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!TimeSpan.TryParse(baslangicSaati, out TimeSpan baslangic))
            {
                TempData["Error"] = "Geçersiz saat formatı!";
                return RedirectToAction(nameof(Create));
            }

            var hizmet = await _context.Hizmetler.FindAsync(hizmetId);
            if (hizmet == null)
            {
                TempData["Error"] = "Hizmet bulunamadı!";
                return RedirectToAction(nameof(Create));
            }

            var bitis = baslangic.Add(TimeSpan.FromMinutes(hizmet.Sure));

            // Çakışma kontrolü
            var cakismaVar = await _context.Randevular.AnyAsync(r =>
                r.AntrenorId == antrenorId &&
                r.RandevuTarihi.Date == randevuTarihi.Date &&
                r.Durum != RandevuDurum.IptalEdildi &&
                r.Durum != RandevuDurum.Reddedildi &&
                ((baslangic >= r.BaslangicSaati && baslangic < r.BitisSaati) ||
                 (bitis > r.BaslangicSaati && bitis <= r.BitisSaati))
            );

            if (cakismaVar)
            {
                TempData["Error"] = "Bu saatte randevu zaten var!";
                return RedirectToAction(nameof(Create));
            }

            var randevu = new Randevu
            {
                UyeId = userId!,
                AntrenorId = antrenorId,
                HizmetId = hizmetId,
                RandevuTarihi = randevuTarihi,
                BaslangicSaati = baslangic,
                BitisSaati = bitis,
                Durum = RandevuDurum.Beklemede,
                OlusturmaTarihi = DateTime.UtcNow
            };

            _context.Randevular.Add(randevu);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Randevu başarıyla oluşturuldu! Onay bekleniyor.";
            return RedirectToAction(nameof(MyAppointments));
        }

        // Admin: Randevu Onayla
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                randevu.Durum = RandevuDurum.Onaylandi;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Randevu onaylandı!";
            }
            return RedirectToAction(nameof(Index));
        }

        // Admin: Randevu Reddet
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Reject(int id)
        {
            var randevu = await _context.Randevular.FindAsync(id);
            if (randevu != null)
            {
                randevu.Durum = RandevuDurum.Reddedildi;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Randevu reddedildi!";
            }
            return RedirectToAction(nameof(Index));
        }

        // Randevu İptal Et
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var randevu = await _context.Randevular.FindAsync(id);

            if (randevu != null && (randevu.UyeId == userId || User.IsInRole("Admin")))
            {
                randevu.Durum = RandevuDurum.IptalEdildi;
                await _context.SaveChangesAsync();
                TempData["Success"] = "Randevu iptal edildi!";
            }

            return User.IsInRole("Admin") ? RedirectToAction(nameof(Index)) : RedirectToAction(nameof(MyAppointments));
        }
    }
}