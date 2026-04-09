using Parking_project.Models.DTO;
using Parking_project.Services;

public static class SuperAdminSeeder
{
    public static async Task SeedAsync(IAuthService authService)
    {
        string email = "superadmin@gmail.com";

        var exists = await authService.IsEmailExistsAsync(email);
        if (exists)
            return;

        var superAdmin = new UserCreateDTO
        {
            Email = email,
            Passwordi = "1234",
            Emri = "Super",
            Mbiemri = "Admin"
        };

        await authService.RegisterAsync(superAdmin, "Super Admin");
    }
}