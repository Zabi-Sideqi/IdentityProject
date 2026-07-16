using Microsoft.AspNetCore.Identity;

namespace IdentityProject.Data
{
    public class SeedData
    {
        private static ApplicationDbContext _context = default!;
        private static RoleManager<IdentityRole> _roleManager = default!;
        private static UserManager<ApplicationUser> _userManager = default!;

        public static async Task Init(ApplicationDbContext context, IServiceProvider service)
        {
            _context = context;

            _roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();
            _userManager = service.GetRequiredService<UserManager<ApplicationUser>>();

            var roleNames = new[] { "Admin", "User" };

            string adminEmail = "admin@admin.com";
            string userEmail = "user@user.com";

            await AddRolesAsync(roleNames);

            var admin = await AddAccountAsync(
                adminEmail,
                "Admin",
                "Adminsson",
                "Admin123!",
                30);

            var user = await AddAccountAsync(
                userEmail,
                "User",
                "Usersson",
                "User123!",
                25);

            await AddUserToRolesAsync(admin, "Admin");
            await AddUserToRolesAsync(user, "User");
        }

        private static async Task AddUserToRolesAsync(ApplicationUser user, string roleName)
        {
            if (await _userManager.IsInRoleAsync(user, roleName))
                return;

            var result = await _userManager.AddToRoleAsync(user, roleName);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to add user '{user.Email}' to role '{roleName}': " +
                    string.Join(
                        Environment.NewLine,
                        result.Errors.Select(e => e.Description)));
            }
        }

        private static async Task<ApplicationUser> AddAccountAsync(
            string accountEmail,
            string fName,
            string lName,
            string password,
            int age)
        {
            var existingUser = await _userManager.FindByEmailAsync(accountEmail);

            if (existingUser != null)
                return existingUser;

            var user = new ApplicationUser
            {
                UserName = accountEmail,
                Email = accountEmail,
                FirstName = fName,
                LastName = lName,
                Age = age,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create user '{accountEmail}': " +
                    string.Join(
                        Environment.NewLine,
                        result.Errors.Select(e => e.Description)));
            }

            return user;
        }

        private static async Task AddRolesAsync(string[] roleNames)
        {
            foreach (string roleName in roleNames)
            {
                if (await _roleManager.RoleExistsAsync(roleName))
                    continue;

                var role = new IdentityRole
                {
                    Name = roleName
                };

                var result = await _roleManager.CreateAsync(role);

                if (!result.Succeeded)
                {
                    throw new InvalidOperationException(
                        $"Failed to create role '{roleName}': " +
                        string.Join(
                            Environment.NewLine,
                            result.Errors.Select(e => e.Description)));
                }
            }
        }
    }
}