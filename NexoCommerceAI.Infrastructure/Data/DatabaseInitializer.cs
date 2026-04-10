using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NexoCommerceAI.Infrastructure.Data.Seed;

namespace NexoCommerceAI.Infrastructure.Data
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services)
        {
            var context = services.GetRequiredService<ApplicationDbContext>();
            
            // Aplica migraciones
            await context.Database.MigrateAsync();

            // Seed inicial de datos
            await SeedData.InitializeAsync(context);
        }
    }
}