using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OpenDispatchHub.Data;
using System.Data;

namespace OpenDispatchHub.Routing;

public class RouteManager : IRouteManager
{
    protected DataContext Context { get; }

    protected ILogger Logger { get; }

    protected IHttpContextAccessor Http { get; }

    public RouteManager(DataContext context, ILogger<RouteManager> logger, IHttpContextAccessor http)
    {
        Context = context;
        Logger = logger;
        Http = http;
    }

    public async Task<IRoute> Create()
    {
        using IDbContextTransaction transaction = await Context.Database.BeginTransactionAsync();

        try
        {
            RoutePlan plan = new RoutePlan();
            await Context.Routes.AddAsync(plan);
            await Context.SaveChangesAsync();
            await transaction.CommitAsync();
            if (Http.HttpContext == null)
            {
                Logger.LogInformation("Created Route {ID}", plan.Id);
            }
            else
            {
                Logger.LogInformation("Created Route {ID} | {Address}", plan.Id, Http.HttpContext.Connection.Id);
            }
            return plan;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> Delete(Guid id)
    {
        using IDbContextTransaction transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            RoutePlan plan = await Context.Routes
            .Include(r => r.Stops)
            .FirstAsync(r => r.Id.Equals(id));

            Context.RouteStops.RemoveRange(plan.Stops);
            Context.Routes.Remove(plan);

            int changes = await Context.SaveChangesAsync();
            await transaction.CommitAsync();

            if (changes > 0)
            {
                if (Http.HttpContext == null)
                {
                    Logger.LogInformation("Deleted Route {ID} - {COUNT} rows affected", plan.Id, changes);
                }
                else
                {
                    Logger.LogInformation("Deleted Route {ID} - {COUNT} rows affected | {Address}", plan.Id, changes, Http.HttpContext.Connection.Id);
                }

                return true;
            }
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        return false;
    }

    public async Task<IRoute?> Get(Guid id)
    {
        return await Context.Routes.Include(r => r.Stops)
            .FirstOrDefaultAsync(r => r.Id.Equals(id));
    }

    public async Task<IRouteStop> CreateStop(IRoute route)
    {
        if (route is not RoutePlan plan)
        {
            throw new InvalidDataException("DatabaseTypeMismatch");
        }

        using IDbContextTransaction transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            RouteStop stop = new RouteStop()
            {
                RouteId = route.Id,
                Route = plan
            };

            stop.Position = (await Context.RouteStops.CountAsync(s => s.RouteId == route.Id)) + 1;
            await Context.RouteStops.AddAsync(stop);
            plan.TotalStops = stop.Position;
            Context.Entry(plan).State = EntityState.Modified;
            await Context.SaveChangesAsync();
            await transaction.CommitAsync();

            if (Http.HttpContext == null)
            {
                Logger.LogInformation("Created Stop {ID} for route {RID}", stop.Id, plan.Id);
            }
            else
            {
                Logger.LogInformation("Created Route {ID} for route {RID} | {Address}", stop.Id, plan.Id, Http.HttpContext.Connection.Id);
            }

            plan.Stops = GetStops(plan.Id)
                .ToBlockingEnumerable()
                .Cast<RouteStop>()
                .ToList();

            return stop;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async IAsyncEnumerable<IRouteStop> GetStops(Guid id)
    {
        var stops = Context.RouteStops
            .Where(s => s.RouteId == id)
            .OrderBy(s => s.Position)
            .AsAsyncEnumerable();

        await foreach (RouteStop stop in stops)
        {
            yield return stop;
        }
    }

    public IAsyncEnumerable<IRoute> Search(object? query)
    {
        if (query == null || (query is string str && str.Equals("*", StringComparison.Ordinal)))
        {
            return Context.Routes.AsAsyncEnumerable();
        }

        throw new NotImplementedException();
    }

    public async Task Update(IRoute route)
    {
        if (route is not RoutePlan plan)
        {
            throw new InvalidDataException("DatabaseTypeMismatch");
        }

        using IDbContextTransaction transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            Context.Routes.Update(plan);
            Context.RouteStops.UpdateRange(plan.Stops);
            await Context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task Update(IRouteStop stop)
    {
        if (stop is not RouteStop routeStop)
        {
            throw new InvalidDataException("DatabaseTypeMismatch");
        }

        using IDbContextTransaction transaction = await Context.Database.BeginTransactionAsync();
        try
        {
            Context.RouteStops.Update(routeStop);
            await Context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
