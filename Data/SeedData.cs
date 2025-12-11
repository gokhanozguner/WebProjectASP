using GokhanOzgunerWEB.Models;
using Microsoft.AspNetCore.Identity;

namespace GokhanOzgunerWEB.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Rolleri oluştur
            string[] roleNames = { "Admin", "Uye" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Admin kullanıcısı oluştur
            var adminEmail = "b221210012@sakarya.edu.tr"; // Öğrenci numarasını buradan değiştirebilirsin
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    AdSoyad = "Admin Kullanıcı",
                    KayitTarihi = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(admin, "sau");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}