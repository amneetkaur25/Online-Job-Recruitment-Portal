using Microsoft.AspNetCore.Identity;

namespace Online_Recruitment_Portal.Seeder
{
    public static class SeedData
    {
        public static async Task SeedAdminUserAsync(UserManager<IdentityUser> userManager)
        {
            if (await userManager.FindByEmailAsync("admin@gmail.com") == null)
            {
                var adminUser = new IdentityUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(adminUser, "Admin123!");

                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
    }
}
