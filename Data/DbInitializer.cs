using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using 打球啊.Models;

namespace 打球啊.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
            var config = services.GetRequiredService<IConfiguration>();

            await context.Database.MigrateAsync();

            string adminEmail = config["SeedAdmin:Email"] ?? "";
            string adminPassword = config["SeedAdmin:Password"] ?? "";

            if (string.IsNullOrWhiteSpace(adminEmail) || string.IsNullOrWhiteSpace(adminPassword))
            {
                return;
            }

            string adminRole = "Admin";
            string userRole = "User";

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            if (!await roleManager.RoleExistsAsync(userRole))
            {
                await roleManager.CreateAsync(new IdentityRole(userRole));
            }

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new IdentityUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(admin, adminPassword);
            }

            if (!await userManager.IsInRoleAsync(admin, adminRole))
            {
                await userManager.AddToRoleAsync(admin, adminRole);
            }

            if (!await context.Courts.AnyAsync())
            {
                context.Courts.AddRange(
                    new Court
                    {
                        Name = "台北和平籃球場",
                        City = "台北市",
                        District = "大安區",
                        Address = "台北市大安區和平東路",
                        CourtType = "戶外",
                        HasLighting = true,
                        Description = "預設測試球場"
                    },
                    new Court
                    {
                        Name = "台中市民籃球場",
                        City = "台中市",
                        District = "西屯區",
                        Address = "台中市西屯區",
                        CourtType = "戶外",
                        HasLighting = true,
                        Description = "預設測試球場"
                    }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}