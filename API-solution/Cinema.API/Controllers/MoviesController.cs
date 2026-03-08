using Cinema.API.Data;
using Cinema.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly CinemaDbContext _context;
    private readonly ILogger<MoviesController> _logger;

    public MoviesController(CinemaDbContext context, ILogger<MoviesController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/movies
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Movie>>> GetMovies()
    {
        try
        {
            var movies = await _context.Movies
                .Include(m => m.Screenings)
                .ToListAsync();
            return Ok(movies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving movies");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/movies/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Movie>> GetMovie(int id)
    {
        try
        {
            var movie = await _context.Movies
                .Include(m => m.Screenings)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (movie == null)
            {
                return NotFound($"Movie with ID {id} not found");
            }

            return Ok(movie);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving movie {MovieId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/movies/genre/Action
    [HttpGet("genre/{genre}")]
    public async Task<ActionResult<IEnumerable<Movie>>> GetMoviesByGenre(string genre)
    {
        try
        {
            var movies = await _context.Movies
                .Include(m => m.Screenings)
                .Where(m => m.Genre.ToLower() == genre.ToLower())
                .ToListAsync();

            if (!movies.Any())
            {
                return NotFound($"No movies found for genre: {genre}");
            }

            return Ok(movies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving movies by genre {Genre}", genre);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/movies
    [HttpPost]
    public async Task<ActionResult<Movie>> CreateMovie(Movie movie)
    {
        try
        {
            _context.Movies.Add(movie);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMovie), new { id = movie.Id }, movie);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating movie");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT: api/movies/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMovie(int id, Movie movie)
    {
        if (id != movie.Id)
        {
            return BadRequest("Movie ID mismatch");
        }

        try
        {
            _context.Entry(movie).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await MovieExists(id))
            {
                return NotFound($"Movie with ID {id} not found");
            }
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating movie {MovieId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/movies/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMovie(int id)
    {
        try
        {
            var movie = await _context.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound($"Movie with ID {id} not found");
            }

            _context.Movies.Remove(movie);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting movie {MovieId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private async Task<bool> MovieExists(int id)
    {
        return await _context.Movies.AnyAsync(e => e.Id == id);
    }
}
