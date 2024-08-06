using Microsoft.EntityFrameworkCore;
using OpenDispatchHub.Data;
using OpenDispatchHub.Routing;

internal class Program
{
    private static void Main(string[] args) => Start(args)
        .GetAwaiter()
        .GetResult();

    private async static Task Start(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Configuration.AddEnvironmentVariables(options =>
        {
            options.Prefix = "ODH:";
        });
        // Add services to the container.

        builder.Services.AddControllers();
        builder.Services.AddDbContext<DataContext>(options =>
        {
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
        });
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IRouteManager, RouteManager>();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseAuthorization();

        app.MapControllers();

        using var scope = app.Services.CreateScope();
        IRouteManager manager = scope.ServiceProvider.GetRequiredService<IRouteManager>();
        IRoute rt = await manager.Create();
        rt.Name = "2121";
        rt.StartTime = new DateTime(2024, 8, 5, 22, 30, 0, DateTimeKind.Local);
        IRouteStop sp = await manager.CreateStop(rt);
        sp.Address = "4002 Galvin Rd Centralia, WA 98531 US";
        sp.Notes = "UNFI Distribution Center";
        sp = await manager.CreateStop(rt);
        sp.Address = "7200 WILLIAMS HIGHWAY, GRANT PASS OR 97527 US";
        sp.Notes = "Hidden Valley Market";
        sp = await manager.CreateStop(rt);
        sp.Address = "4685 HIGHWAY 234, WHITE CITY OR 97503 US";
        sp.Notes = "Rainey's Corner";
        sp = await manager.CreateStop(rt);
        sp.Address = "143 DAVIS ROAD, HAPPY CAMP CA 96039 US";
        sp.Notes = "Kingfisher Market";
        sp = await manager.CreateStop(rt);
        sp.Address = "4002 Galvin Rd Centralia, WA 98531 US";
        sp.Notes = "UNFI Distribution Center";
        await manager.Update(rt);
        app.Run();
    }
}