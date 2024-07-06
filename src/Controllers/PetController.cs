using System.ComponentModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Annotations;

namespace Zggff.MaiPractice.Controllers;

[Route("pet")]
[ApiController]
public class PetController(AppDbContext context) : ControllerBase
{
    private AppDbContext _context { get; } = context;

    [SwaggerOperation("get pet by id")]
    [SwaggerResponse(StatusCodes.Status200OK, "the pet with id was found", typeof(Pet))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "no pet with such id was found", typeof(void))]
    [HttpGet("{id}")]
    public async Task<IActionResult> PetById(uint id)
    {
        var pet = await _context.Pets.FindAsync(id);
        return pet == null ? NotFound("id not in database") : Ok(pet);
    }

    [SwaggerOperation("list pets in database. You can select the status of pets to be selected")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(Pet[]), Description = "all pets were listed")]
    [HttpGet("list")]
    public async Task<ActionResult<IEnumerable<Pet>>> List(Status? status)
    {
        if (status == null)
        {
            return Ok(await _context.Pets.ToListAsync());
        }
        return Ok(await _context.Pets.Where(p => p.Status == status).ToListAsync());
    }

    [SwaggerOperation("add pet to database")]
    [SwaggerResponse(StatusCodes.Status200OK, "pet was added", typeof(void))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "user is not authorised", typeof(void))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "user does not have permission to perform action", typeof(void))]
    [HttpPost(""), Authorize(Roles = "Admin")]
    public async Task<IActionResult> Pet(Pet p)
    {
        p.Id = 0;
        await _context.Pets.AddAsync(p);
        await _context.SaveChangesAsync();
        return Ok();
    }


    [SwaggerOperation("update pet in database")]
    [SwaggerResponse(StatusCodes.Status200OK, "pet was changed", typeof(void))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "pet with such id does not exists", typeof(void))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "user is not authorised", typeof(void))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "user does not have permission to perform action", typeof(void))]
    [HttpPut(""), Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdatePet(Pet p)
    {
        var pet = await _context.Pets.FindAsync(p.Id);
        if (pet == null)
        {
            return NotFound("id not in database");
        }
        pet.Name = p.Name;
        pet.PhotoUrls = p.PhotoUrls;
        pet.Species = p.Species;
        pet.Status = p.Status;
        await _context.SaveChangesAsync();
        return Ok();
    }
}