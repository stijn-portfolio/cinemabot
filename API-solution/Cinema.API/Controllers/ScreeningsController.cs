using Cinema.API.Data;
using Cinema.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ScreeningsController : ControllerBase
{
    private readonly CinemaDbContext _context;
    private readonly ILogger<ScreeningsController> _logger;

    public ScreeningsController(CinemaDbContext context, ILogger<ScreeningsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/screenings
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Screening>>> GetScreenings()
    {
        try
        {
            var screenings = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Bookings)
                .ToListAsync();
            return Ok(screenings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving screenings");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/screenings/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Screening>> GetScreening(int id)
    {
        try
        {
            var screening = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Bookings)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (screening == null)
            {
                return NotFound($"Screening with ID {id} not found");
            }

            return Ok(screening);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving screening {ScreeningId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/screenings/movie/5
    [HttpGet("movie/{movieId}")]
    public async Task<ActionResult<IEnumerable<Screening>>> GetScreeningsByMovie(int movieId)
    {
        try
        {
            var screenings = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Bookings)
                .Where(s => s.MovieId == movieId)
                .OrderBy(s => s.DateTime)
                .ToListAsync();

            if (!screenings.Any())
            {
                return NotFound($"No screenings found for movie ID: {movieId}");
            }

            return Ok(screenings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving screenings for movie {MovieId}", movieId);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/screenings/date/2024-12-01
    [HttpGet("date/{date}")]
    public async Task<ActionResult<IEnumerable<Screening>>> GetScreeningsByDate(DateTime date)
    {
        try
        {
            var screenings = await _context.Screenings
                .Include(s => s.Movie)
                .Include(s => s.Bookings)
                .Where(s => s.DateTime.Date == date.Date)
                .OrderBy(s => s.DateTime)
                .ToListAsync();

            if (!screenings.Any())
            {
                return NotFound($"No screenings found for date: {date:yyyy-MM-dd}");
            }

            return Ok(screenings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving screenings for date {Date}", date);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/screenings
    [HttpPost]
    public async Task<ActionResult<Screening>> CreateScreening(Screening screening)
    {
        try
        {
            // Verify movie exists
            var movie = await _context.Movies.FindAsync(screening.MovieId);
            if (movie == null)
            {
                return BadRequest($"Movie with ID {screening.MovieId} not found");
            }

            _context.Screenings.Add(screening);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetScreening), new { id = screening.Id }, screening);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating screening");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT: api/screenings/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateScreening(int id, Screening screening)
    {
        if (id != screening.Id)
        {
            return BadRequest("Screening ID mismatch");
        }

        try
        {
            _context.Entry(screening).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ScreeningExists(id))
            {
                return NotFound($"Screening with ID {id} not found");
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating screening {ScreeningId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/screenings/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteScreening(int id)
    {
        try
        {
            var screening = await _context.Screenings.FindAsync(id);
            if (screening == null)
            {
                return NotFound($"Screening with ID {id} not found");
            }

            _context.Screenings.Remove(screening);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting screening {ScreeningId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private async Task<bool> ScreeningExists(int id)
    {
        return await _context.Screenings.AnyAsync(e => e.Id == id);
    }
}
