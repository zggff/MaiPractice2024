using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Zggff.MaiPractice.Controllers;

[ApiController]
public class PetController(AppDbContext context) : ControllerBase
{
    public AppDbContext Context { get; } = context;


    [HttpGet("pet/{id}")]
    [ProducesResponseType<Pet>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PetById(uint id)
    {
        var pet = await Context.Pets.FindAsync(id);
        return pet == null ? NotFound("id not in database") : Ok(pet);
    }

    [ProducesResponseType<Pet[]>(StatusCodes.Status200OK)]
    [HttpGet("pets")]
    public async Task<ActionResult<IEnumerable<Pet>>> Pets()
    {
        return await Context.Pets.ToListAsync();
    }


    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost("pet")]
    public async Task<ActionResult<Pet>> Pet(Pet p)
    {
        var pet = await Context.Pets.FindAsync(p.Id);
        if (pet != null)
        {
            return BadRequest();
        }

        await Context.Pets.AddAsync(p);
        await Context.SaveChangesAsync();
        return Ok();
    }


    [HttpPut("pet")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePet(Pet p)
    {
        var pet = await Context.Pets.FindAsync(p.Id);
        if (pet == null)
        {
            return NotFound("id not in database");
        }
        pet.Name = p.Name;
        pet.PhotoUrls = p.PhotoUrls;
        pet.Species = p.Species;
        pet.Status = p.Status;
        await Context.SaveChangesAsync();
        return Ok();
    }
}