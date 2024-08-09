using Microsoft.AspNetCore.Mvc;
using OpenDispatchHub.Routing;
using System.Collections;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace OpenDispatchHub.Controllers;
[Route("api/[controller]")]
[ApiController]
public class RouteController : ControllerBase
{
    private IRouteManager Manager { get; }

    public RouteController(IRouteManager manager)
    {
        Manager = manager;
    }


    // GET: api/<RouteController>
    [HttpGet]
    [ProducesResponseType<IEnumerable<IRoute>>(404)]
    [ProducesResponseType<IEnumerable<IRoute>>(200)]
    public async IAsyncEnumerable<IRoute> Get()
    {
        IAsyncEnumerable<IRoute>? routes = Manager.Search(null);

        if (routes == null)
        {
            HttpContext.Response.StatusCode = 404;
            yield break;
        }

        IAsyncEnumerator<IRoute> plans = routes.GetAsyncEnumerator(HttpContext.RequestAborted);

        while (await plans.MoveNextAsync())
        {
            yield return plans.Current;
        }
    }

    // GET api/<RouteController>/5
    [HttpGet("{id}")]
    [ProducesResponseType<RoutePlan>(200)]
    [ProducesResponseType(404)]
    public async Task<ActionResult<RoutePlan?>> Get(Guid id)
    {
        IRoute? plan = await Manager.Get(id);
        
        if (plan == null)
        {
            return NotFound();
        }

        return Ok((RoutePlan)plan);
    }

    // POST api/<RouteController>
    [HttpPost]
    [ProducesResponseType<IRoute>(200)]
    public async Task<ActionResult<IRoute>> Post([FromBody] RouteCreation route)
    {
        IRoute plan = await Manager.Create();
        bool updated = false;

        if (!string.IsNullOrWhiteSpace(route.Name))
        {
            plan.Name = route.Name;
            updated = true;
        }

        if (route.StartTime.HasValue)
        {
            plan.StartTime = route.StartTime.Value;
            updated = true;
        }

        if (updated)
        {
            await Manager.Update(plan);
        }

        return Ok(plan);
    }

    // PUT api/<RouteController>/5
    [HttpPut("{id}")]
    [ProducesResponseType(404)]
    [ProducesResponseType<IRoute>(200)]
    public async Task<ActionResult<IRoute>> Put(Guid id, [FromBody] RouteCreation route)
    {
        IRoute? plan = await Manager.Get(id);

        if (plan == null)
        {
            return NotFound();
        }

        plan.Name = route.Name ?? plan.Id.ToString("N");
        
        if (route.StartTime.HasValue)
        {
            plan.StartTime = route.StartTime.Value;
        }

        await Manager.Update(plan);

        return Ok(plan);
    }

    // DELETE api/<RouteController>/5
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult> Delete(Guid id)
    {
        bool result = await Manager.Delete(id);
        return result ? Ok() : NotFound();
    }

    #region Types

    public struct RouteCreation
    {
        public string? Name { get; init; }

        public DateTime? StartTime { get; init; }
    }

    #endregion
}
