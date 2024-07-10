using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;
using Zggff.MaiPractice.Middleware;
using Zggff.MaiPractice.Models;

namespace Zggff.MaiPractice.Controllers;

[Route("store")]
[ApiController]
public class StoreController(AppDbContext context, IHttpContextAccessor accessor) : ControllerBase

{
    private AppDbContext context { get; } = context;
    private ClaimMiddleware claims { get; } = new ClaimMiddleware(accessor);


    [HttpPost("order"), Authorize]
    [SwaggerOperation("place an order. you must be logged in to do so")]
    [SwaggerResponse(StatusCodes.Status200OK, "ok")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "pet with this id was already ordered", typeof(void))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "pet with this id does not exist", typeof(void))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "you must be logged in to place an order", typeof(void))]
    public async Task<IActionResult> Order([Required] uint petId)
    {
        var userId = claims.Id();
        var pet = await context.Pets.FindAsync(petId);
        var user = await context.Users.FindAsync(userId);
        if (pet == null || user == null)
            return NotFound();

        if (pet.Status != PetStatus.Available)
            return BadRequest();

        var order = new Order
        {
            PetId = petId,
            UserId = userId,
            Status = OrderStatus.Placed,
            PlacedDate = DateTime.Now.ToUniversalTime()
        };
        pet.Status = PetStatus.Pending;
        await context.Orders.AddAsync(order);
        await context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("order/{orderId}"), Authorize]
    [SwaggerOperation("remove an order. You must be the user who placed the order")]
    [SwaggerResponse(StatusCodes.Status200OK, "ok")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "a completed order cannot be canceled", typeof(void))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "order with this id does not exist", typeof(void))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "you must be logged in to delete an order", typeof(void))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "this is not your order", typeof(void))]
    public async Task<IActionResult> Remove(uint orderId)
    {
        var order = await context.Orders.FindAsync(orderId);
        var userId = claims.Id();

        if (order == null)
            return NotFound();
        if (userId != order.UserId)
            return Forbid();
        if (order.Status == OrderStatus.Completed)
            return BadRequest();

        var pet = await context.Pets.FindAsync(order.PetId);
        if (pet == null)
            return BadRequest();

        pet.Status = PetStatus.Available;
        context.Orders.Remove(order);
        await context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("order/{orderId}"), Authorize]
    [SwaggerOperation("view an order. You must either be the user who placed the order or an admin")]
    [SwaggerResponse(StatusCodes.Status200OK, "ok", typeof(Order))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "order with this id does not exist", typeof(void))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "you must be logged in to view an order", typeof(void))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "this is not your order", typeof(void))]
    public async Task<IActionResult> Get(uint orderId)
    {
        var order = await context.Orders.FindAsync(orderId);
        if (order == null)
            return NotFound();

        if (claims.Id() != order.UserId && claims.Role() != UserRole.Admin)
            return Forbid();

        return Ok(order);
    }

    [HttpPut("order/advance/{orderId}"), Authorize(Roles = "Admin")]
    [SwaggerOperation("advance an order")]
    [SwaggerResponse(StatusCodes.Status200OK, "ok")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "order with this id does not exist", typeof(void))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "you must be logged in to advance an order", typeof(void))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "you must be an admin to advance an order", typeof(void))]
    public async Task<IActionResult> Advance(uint orderId)
    {
        var order = await context.Orders.FindAsync(orderId);
        if (order == null)
            return NotFound();

        if (order.Status == OrderStatus.Placed)
            order.Status = OrderStatus.Approved;
        else if (order.Status == OrderStatus.Approved)
        {
            order.Status = OrderStatus.Completed;
            order.CompletedDate = DateTime.Now.ToUniversalTime();
            var pet = await context.Pets.FindAsync(order.PetId);
            if (pet == null)
            {
                return BadRequest();
            }
            pet.Status = PetStatus.Sold;
        }
        await context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("orders"), Authorize(Roles = "Admin")]
    [SwaggerOperation("list all orders")]
    [SwaggerResponse(StatusCodes.Status200OK, "ok")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "you must be logged in to advance an order", typeof(void))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "you must be an admin to advance an order", typeof(void))]
    public async Task<IActionResult> List([Required, FromQuery] HashSet<OrderStatus> status)
    {
        return Ok(await context.Orders.Where(u => status.Contains(u.Status)).ToListAsync());
    }

    [HttpGet("my_orders"), Authorize()]
    [SwaggerOperation("list all orders of a user")]
    [SwaggerResponse(StatusCodes.Status200OK, "ok")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "you must be logged in to advance an order", typeof(void))]
    public async Task<IActionResult> ListMy([Required, FromQuery] HashSet<OrderStatus> status)
    {
        var id = claims.Id();
        return Ok(await context.Orders.Where(u => status.Contains(u.Status) && u.UserId == id).ToListAsync());
    }
}