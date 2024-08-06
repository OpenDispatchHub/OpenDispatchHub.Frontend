using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OpenDispatchHub.Routing;

[Index(nameof(Name), IsUnique = false, Name = nameof(Name))]
[Table("Routes")]
public class RoutePlan : IRoute
{
    [Key]
    public Guid Id { get; private set; }

    [Required]
    public string Name { get; set; }

    public DateTimeOffset StartTime { get; set; }

    public int TotalStops { get; internal set; }

    public ICollection<RouteStop> Stops { get; internal set; }

    public IEnumerable<IRouteStop> GetStops() => Stops;

    public RoutePlan()
    {
        Id = Guid.NewGuid();
        Name = Id.ToString("N").Substring(20, 12);
        StartTime = DateTimeOffset.UtcNow;
        TotalStops = 0;
        Stops = new List<RouteStop>();
    }
}
