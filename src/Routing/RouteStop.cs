using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenDispatchHub.Routing;

[Index(nameof(RouteId), IsUnique = false, Name = "Route")]
[Index(nameof(Address), IsUnique = false, Name = "Address")]
[Table("Route.Stops")]
public class RouteStop : IRouteStop
{
    [Key]
    public Guid Id { get; private set; }

    [Required]
    public int Position { get; set; }

    [Required]
    public Guid RouteId { get; internal set; }

    [ForeignKey(nameof(RouteId))]
    public RoutePlan Route { get; internal set; }

    [Required(ErrorMessage = "A address needs to be defined", AllowEmptyStrings = true)]
    public string Address { get; set; }

    public string Notes { get; set; }

    public IRoute GetRoute() => Route;

    public RouteStop()
    {
        Id = Guid.NewGuid();
        Position = 0;
        RouteId = Guid.Empty;
        Route = null!;
        Address = string.Empty;
        Notes = string.Empty;
    }
}
