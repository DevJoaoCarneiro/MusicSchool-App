using Domain.Entities;
using Domain.Interfaces;
using Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seeding
{
    public static class AdminSeed
    {
        public static async Task SeedAsync(
            AppDbContext context,
            ISecurityService securityService)
        {
            if (await context.Users.AnyAsync(user => user.Role == UserRole.Admin))
                return;

            var password = Environment.GetEnvironmentVariable(
                "ADMIN_INITIAL_PASSWORD");

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(
                    "ADMIN_INITIAL_PASSWORD must be configured when no Admin user exists.");
            }

            var name = Environment.GetEnvironmentVariable("ADMIN_INITIAL_NAME")
                ?? "Administrator";
            var email = Environment.GetEnvironmentVariable("ADMIN_INITIAL_EMAIL")
                ?? "admin@musicschool.local";

            var admin = new User(
                name,
                email,
                securityService.HashPassword(password),
                UserRole.Admin);

            context.Users.Add(admin);
            await context.SaveChangesAsync();
        }
    }
}
