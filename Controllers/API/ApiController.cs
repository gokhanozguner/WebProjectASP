using GokhanOzgunerWEB.Data;
using GokhanOzgunerWEB.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GokhanOzgunerWEB.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class FitnessApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public FitnessApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/FitnessApi/Antrenorler
        [HttpGet("Antrenorler")]
        public async Task<ActionResult<IEnumerable<object>>> GetAntrenorler()
        {
            var antrenorler = await _context.Antrenorler
                .Include(a => a.Salon)
                .Where(a => a.Aktif)
                .OrderBy(a => a.AdSoyad)
                .Select(a => new
                {
                    a.Id,
                    a.AdSoyad,
                    a.UzmanlikAlanlari,
                    a.Email,
                    a.Telefon,
                    Salon = new
                    {
                        a.Salon.Id,
                        a.Salon.Ad
                    }
                })
                .ToListAsync();

            return Ok(antrenorler);
        }

        // GET: api/FitnessApi/AntrenorlerBySalon/1
        [HttpGet("AntrenorlerBySalon/{salonId}")]
        public async Task<ActionResult<IEnumerable<object>>> GetAntrenorlerBySalon(int salonId)
        {
            var antrenorler = await _context.Antrenorler
                .Include(a => a.Salon)
                .Include(a => a.Musaitlikler)
                .Where(a => a.SalonId == salonId && a.Aktif)
                .OrderBy(a => a.AdSoyad)
                .Select(a => new
                {
                    a.Id,
                    a.AdSoyad,
                    a.UzmanlikAlanlari,
                    a.Email,
                    a.Telefon,
                    MusaitlikSayisi = a.Musaitlikler.Count(m => m.Aktif),
                    Musaitlikler = a.Musaitlikler
                        .Where(m => m.Aktif)
                        .Select(m => new
                        {
                            Gun = m.Gun.ToString(),
                            BaslangicSaati = m.BaslangicSaati.ToString(@"hh\:mm"),
                            BitisSaati = m.BitisSaati.ToString(@"hh\:mm")
                        })
                })
                .ToListAsync();

            if (!antrenorler.Any())
                return NotFound(new { message = "Bu salonda antrenör bulunamadı." });

            return Ok(antrenorler);
        }

        // GET: api/FitnessApi/UygunAntrenorler?tarih=2024-01-15&hizmetId=1
        [HttpGet("UygunAntrenorler")]
        public async Task<ActionResult<IEnumerable<object>>> GetUygunAntrenorler(DateTime tarih, int hizmetId)
        {
            var gun = tarih.DayOfWeek;

            var uygunAntrenorler = await _context.Antrenorler
                .Include(a => a.Salon)
                .Include(a => a.Musaitlikler)
                .Include(a => a.AntrenorHizmetleri)
                    .ThenInclude(ah => ah.Hizmet)
                .Where(a => a.Aktif &&
                    a.AntrenorHizmetleri.Any(ah => ah.HizmetId == hizmetId) &&
                    a.Musaitlikler.Any(m => m.Gun == gun && m.Aktif))
                .Select(a => new
                {
                    a.Id,
                    a.AdSoyad,
                    a.UzmanlikAlanlari,
                    Salon = a.Salon.Ad,
                    Musaitlikler = a.Musaitlikler
                        .Where(m => m.Gun == gun && m.Aktif)
                        .Select(m => new
                        {
                            BaslangicSaati = m.BaslangicSaati.ToString(@"hh\:mm"),
                            BitisSaati = m.BitisSaati.ToString(@"hh\:mm")
                        })
                })
                .ToListAsync();

            if (!uygunAntrenorler.Any())
                return NotFound(new { message = "Bu tarih için uygun antrenör bulunamadı." });

            return Ok(uygunAntrenorler);
        }

        // GET: api/FitnessApi/Randevular
        [Authorize(Roles = "Admin")]
        [HttpGet("Randevular")]
        public async Task<ActionResult<IEnumerable<object>>> GetRandevular()
        {
            var randevular = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .OrderByDescending(r => r.RandevuTarihi)
                .Select(r => new
                {
                    r.Id,
                    RandevuTarihi = r.RandevuTarihi.ToString("yyyy-MM-dd"),
                    BaslangicSaati = r.BaslangicSaati.ToString(@"hh\:mm"),
                    BitisSaati = r.BitisSaati.ToString(@"hh\:mm"),
                    Uye = new
                    {
                        r.Uye.AdSoyad,
                        r.Uye.Email
                    },
                    Antrenor = new
                    {
                        r.Antrenor.AdSoyad,
                        r.Antrenor.UzmanlikAlanlari
                    },
                    Hizmet = new
                    {
                        r.Hizmet.Ad,
                        r.Hizmet.Sure,
                        r.Hizmet.Ucret
                    },
                    Durum = r.Durum.ToString(),
                    r.OlusturmaTarihi
                })
                .ToListAsync();

            return Ok(randevular);
        }

        // GET: api/FitnessApi/RandevularByDurum?durum=0
        [Authorize(Roles = "Admin")]
        [HttpGet("RandevularByDurum")]
        public async Task<ActionResult<IEnumerable<object>>> GetRandevularByDurum(RandevuDurum durum)
        {
            var randevular = await _context.Randevular
                .Include(r => r.Uye)
                .Include(r => r.Antrenor)
                .Include(r => r.Hizmet)
                .Where(r => r.Durum == durum)
                .OrderByDescending(r => r.RandevuTarihi)
                .Select(r => new
                {
                    r.Id,
                    RandevuTarihi = r.RandevuTarihi.ToString("yyyy-MM-dd"),
                    BaslangicSaati = r.BaslangicSaati.ToString(@"hh\:mm"),
                    BitisSaati = r.BitisSaati.ToString(@"hh\:mm"),
                    Uye = r.Uye.AdSoyad,
                    Antrenor = r.Antrenor.AdSoyad,
                    Hizmet = r.Hizmet.Ad,
                    Ucret = r.Hizmet.Ucret,
                    Durum = r.Durum.ToString()
                })
                .ToListAsync();

            return Ok(new
            {
                Durum = durum.ToString(),
                ToplamRandevu = randevular.Count,
                Randevular = randevular
            });
        }

        // GET: api/FitnessApi/RandevuIstatistikleri
        [Authorize(Roles = "Admin")]
        [HttpGet("RandevuIstatistikleri")]
        public async Task<ActionResult<object>> GetRandevuIstatistikleri()
        {
            var toplamRandevu = await _context.Randevular.CountAsync();
            var bekleyenRandevu = await _context.Randevular.CountAsync(r => r.Durum == RandevuDurum.Beklemede);
            var onaylananRandevu = await _context.Randevular.CountAsync(r => r.Durum == RandevuDurum.Onaylandi);
            var tamamlananRandevu = await _context.Randevular.CountAsync(r => r.Durum == RandevuDurum.Tamamlandi);
            var iptalEdilenRandevu = await _context.Randevular.CountAsync(r => r.Durum == RandevuDurum.IptalEdildi);

            var toplamGelir = await _context.Randevular
                .Include(r => r.Hizmet)
                .Where(r => r.Durum == RandevuDurum.Tamamlandi)
                .SumAsync(r => r.Hizmet.Ucret);

            var enCokRandevuAlanAntrenorler = await _context.Randevular
                .Include(r => r.Antrenor)
                .GroupBy(r => new { r.AntrenorId, r.Antrenor.AdSoyad })
                .Select(g => new
                {
                    Antrenor = g.Key.AdSoyad,
                    RandevuSayisi = g.Count()
                })
                .OrderByDescending(x => x.RandevuSayisi)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                ToplamRandevu = toplamRandevu,
                BekleyenRandevu = bekleyenRandevu,
                OnaylananRandevu = onaylananRandevu,
                TamamlananRandevu = tamamlananRandevu,
                IptalEdilenRandevu = iptalEdilenRandevu,
                ToplamGelir = toplamGelir,
                EnCokRandevuAlanAntrenorler = enCokRandevuAlanAntrenorler
            });
        }

        // GET: api/FitnessApi/Salonlar
        [HttpGet("Salonlar")]
        public async Task<ActionResult<IEnumerable<object>>> GetSalonlar()
        {
            var salonlar = await _context.Salonlar
                .Where(s => s.Aktif)
                .Select(s => new
                {
                    s.Id,
                    s.Ad,
                    s.Adres,
                    s.Telefon,
                    AcilisSaati = s.AcilisSaati.ToString(@"hh\:mm"),
                    KapanisSaati = s.KapanisSaati.ToString(@"hh\:mm"),
                    AntrenorSayisi = s.Antrenorler.Count(a => a.Aktif),
                    HizmetSayisi = s.Hizmetler.Count(h => h.Aktif)
                })
                .ToListAsync();

            return Ok(salonlar);
        }

        // GET: api/FitnessApi/Hizmetler
        [HttpGet("Hizmetler")]
        public async Task<ActionResult<IEnumerable<object>>> GetHizmetler()
        {
            var hizmetler = await _context.Hizmetler
                .Include(h => h.Salon)
                .Where(h => h.Aktif)
                .OrderBy(h => h.Salon.Ad)
                .ThenBy(h => h.Ad)
                .Select(h => new
                {
                    h.Id,
                    h.Ad,
                    h.Aciklama,
                    Sure = h.Sure + " dakika",
                    h.Ucret,
                    Salon = h.Salon.Ad
                })
                .ToListAsync();

            return Ok(hizmetler);
        }
    }
}