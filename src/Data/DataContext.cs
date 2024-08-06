using Microsoft.EntityFrameworkCore;
using OpenDispatchHub.Routing;

namespace OpenDispatchHub.Data;

public class DataContext : DbContext
{
    public DbSet<RoutePlan> Routes { get; private set; }

    public DbSet<RouteStop> RouteStops { get; private set; }

    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }
}
