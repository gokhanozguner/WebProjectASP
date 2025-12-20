using GokhanOzgunerWEB.Data;
using GokhanOzgunerWEB.Models;
using GokhanOzgunerWEB.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace FitnessYonetimi.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AntrenorController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AntrenorController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Antrenor/Index
        public async Task<IActionResult> Index()
        {
            var antrenorler = await _context.Antrenorler
                .Include(a => a.Salon)
                .Include(a => a.AntrenorHizmetleri)
                    .ThenInclude(ah => ah.Hizmet)
                .ToListAsync();
            return View(antrenorler);
        }

        // GET: Antrenor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.Salon)
                .Include(a => a.Musaitlikler)
                .Include(a => a.AntrenorHizmetleri)
                    .ThenInclude(ah => ah.Hizmet)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (antrenor == null) return NotFound();

            return View(antrenor);
        }

        // GET: Antrenor/Create
        public IActionResult Create()
        {
            var viewModel = new AntrenorCreateViewModel
            {
                Musaitlikler = new List<MusaitlikViewModel>
                {
                    new MusaitlikViewModel { Gun = DayOfWeek.Monday, GunAdi = "Pazartesi" },
                    new MusaitlikViewModel { Gun = DayOfWeek.Tuesday, GunAdi = "Salı" },
                    new MusaitlikViewModel { Gun = DayOfWeek.Wednesday, GunAdi = "Çarşamba" },
                    new MusaitlikViewModel { Gun = DayOfWeek.Thursday, GunAdi = "Perşembe" },
                    new MusaitlikViewModel { Gun = DayOfWeek.Friday, GunAdi = "Cuma" },
                    new MusaitlikViewModel { Gun = DayOfWeek.Saturday, GunAdi = "Cumartesi" },
                    new MusaitlikViewModel { Gun = DayOfWeek.Sunday, GunAdi = "Pazar" }
                }
            };

            ViewData["SalonId"] = new SelectList(_context.Salonlar.Where(s => s.Aktif), "Id", "Ad");
            return View(viewModel);
        }

        // POST: Antrenor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AntrenorCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var antrenor = new Antrenor
                {
                    AdSoyad = viewModel.AdSoyad,
                    UzmanlikAlanlari = viewModel.UzmanlikAlanlari,
                    Telefon = viewModel.Telefon,
                    Email = viewModel.Email,
                    Aktif = viewModel.Aktif,
                    SalonId = viewModel.SalonId
                };

                _context.Add(antrenor);
                await _context.SaveChangesAsync();

                // Müsaitleri ekle
                foreach (var musaitlik in viewModel.Musaitlikler.Where(m => m.Secildi))
                {
                    var antrenorMusaitlik = new AntrenorMusaitlik
                    {
                        AntrenorId = antrenor.Id,
                        Gun = musaitlik.Gun,
                        BaslangicSaati = musaitlik.BaslangicSaati,
                        BitisSaati = musaitlik.BitisSaati,
                        Aktif = true
                    };
                    _context.AntrenorMusaitlikler.Add(antrenorMusaitlik);
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = "Antrenör ve müsaitlikler başarıyla oluşturuldu!";
                return RedirectToAction(nameof(Index));
            }

            ViewData["SalonId"] = new SelectList(_context.Salonlar.Where(s => s.Aktif), "Id", "Ad", viewModel.SalonId);
            return View(viewModel);
        }

        // GET: Antrenor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor == null) return NotFound();

            ViewData["SalonId"] = new SelectList(_context.Salonlar.Where(s => s.Aktif), "Id", "Ad", antrenor.SalonId);
            return View(antrenor);
        }

        // POST: Antrenor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,AdSoyad,UzmanlikAlanlari,Telefon,Email,Aktif,SalonId")] Antrenor antrenor)
        {
            if (id != antrenor.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(antrenor);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Antrenör başarıyla güncellendi!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AntrenorExists(antrenor.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["SalonId"] = new SelectList(_context.Salonlar.Where(s => s.Aktif), "Id", "Ad", antrenor.SalonId);
            return View(antrenor);
        }

        // GET: Antrenor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var antrenor = await _context.Antrenorler
                .Include(a => a.Salon)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (antrenor == null) return NotFound();

            return View(antrenor);
        }

        // POST: Antrenor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var antrenor = await _context.Antrenorler.FindAsync(id);
            if (antrenor != null)
            {
                _context.Antrenorler.Remove(antrenor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Antrenör başarıyla silindi!";
            }
            return RedirectToAction(nameof(Index));
        }

        private bool AntrenorExists(int id)
        {
            return _context.Antrenorler.Any(e => e.Id == id);
        }
    }
}