using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace NexoCommerceAI.Infrastructure.Persistence;

internal sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<AppDbContext>();

        // Try environment variable first, otherwise use a sensible default for local development
        var conn = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION")
            ?? "Host=localhost;Database=nexocommerceai;Username=postgres;Password=P@bl0*2026";

        builder.UseNpgsql(conn);

        // No interceptors at design time
        return new AppDbContext(builder.Options, Array.Empty<ISaveChangesInterceptor>());
    }
}
