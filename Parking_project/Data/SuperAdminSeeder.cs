using Parking_project.Models.DTO;
using Parking_project.Services;

public static class SuperAdminSeeder
{
    public static async Task SeedAsync(IAuthService authService)
    {
        string username = "superadmin";

        var exists = await authService.UsernameExistsAsync(username);
        if (exists)
            return;

        var superAdmin = new UserCreateDTO
        {
            Username = username,
            Passwordi = "super1234",
            Emri = "Super",
            Mbiemri = "Admin"
        };

        await authService.RegisterAsync(superAdmin, "Super Admin");
    }
}