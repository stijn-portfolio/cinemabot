using Cinema.API.Data;
using Cinema.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookingsController : ControllerBase
{
    private readonly CinemaDbContext _context;
    private readonly ILogger<BookingsController> _logger;

    public BookingsController(CinemaDbContext context, ILogger<BookingsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: api/bookings
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
    {
        try
        {
            var bookings = await _context.Bookings
                .Include(b => b.Screening)
                    .ThenInclude(s => s!.Movie)
                .ToListAsync();
            return Ok(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bookings");
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/bookings/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Booking>> GetBooking(int id)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Screening)
                    .ThenInclude(s => s!.Movie)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound($"Booking with ID {id} not found");
            }

            return Ok(booking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving booking {BookingId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/bookings/code/CINE1234
    [HttpGet("code/{bookingCode}")]
    public async Task<ActionResult<Booking>> GetBookingByCode(string bookingCode)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Screening)
                    .ThenInclude(s => s!.Movie)
                .FirstOrDefaultAsync(b => b.BookingCode == bookingCode);

            if (booking == null)
            {
                return NotFound($"Booking with code {bookingCode} not found");
            }

            return Ok(booking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving booking by code {BookingCode}", bookingCode);
            return StatusCode(500, "Internal server error");
        }
    }

    // GET: api/bookings/email/user@example.com
    [HttpGet("email/{email}")]
    public async Task<ActionResult<IEnumerable<Booking>>> GetBookingsByEmail(string email)
    {
        try
        {
            var bookings = await _context.Bookings
                .Include(b => b.Screening)
                    .ThenInclude(s => s!.Movie)
                .Where(b => b.Email.ToLower() == email.ToLower())
                .OrderByDescending(b => b.BookedAt)
                .ToListAsync();

            if (!bookings.Any())
            {
                return NotFound($"No bookings found for email: {email}");
            }

            return Ok(bookings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving bookings for email {Email}", email);
            return StatusCode(500, "Internal server error");
        }
    }

    // POST: api/bookings
    // This is the crucial endpoint with 5+ required fields for Challenge 2 requirements
    [HttpPost]
    public async Task<ActionResult<Booking>> CreateBooking(CreateBookingDto bookingDto)
    {
        try
        {
            // Verify screening exists
            var screening = await _context.Screenings
                .Include(s => s.Movie)
                .FirstOrDefaultAsync(s => s.Id == bookingDto.ScreeningId);

            if (screening == null)
            {
                return BadRequest($"Screening with ID {bookingDto.ScreeningId} not found");
            }

            // Check seat availability
            if (screening.AvailableSeats < bookingDto.Seats)
            {
                return BadRequest($"Not enough seats available. Only {screening.AvailableSeats} seats left.");
            }

            // Generate unique booking code
            var bookingCode = GenerateBookingCode();

            // Create booking
            var booking = new Booking
            {
                ScreeningId = bookingDto.ScreeningId,
                CustomerName = bookingDto.CustomerName,
                Email = bookingDto.Email,
                Phone = bookingDto.Phone,
                Seats = bookingDto.Seats,
                BookingCode = bookingCode,
                BookedAt = DateTime.Now
            };

            _context.Bookings.Add(booking);

            // Update available seats
            screening.AvailableSeats -= bookingDto.Seats;

            await _context.SaveChangesAsync();

            // Reload booking with navigation properties
            var createdBooking = await _context.Bookings
                .Include(b => b.Screening)
                    .ThenInclude(s => s!.Movie)
                .FirstOrDefaultAsync(b => b.Id == booking.Id);

            return CreatedAtAction(nameof(GetBooking), new { id = booking.Id }, createdBooking);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating booking");
            return StatusCode(500, "Internal server error");
        }
    }

    // PUT: api/bookings/5
    // Update booking (e.g., change number of seats)
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBooking(int id, UpdateBookingDto updateDto)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Screening)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound($"Booking with ID {id} not found");
            }

            var screening = booking.Screening;
            if (screening == null)
            {
                return BadRequest("Screening not found");
            }

            // Calculate seat difference
            var seatDifference = updateDto.Seats - booking.Seats;

            // Check if enough seats available for increase
            if (seatDifference > 0 && screening.AvailableSeats < seatDifference)
            {
                return BadRequest($"Cannot increase seats. Only {screening.AvailableSeats} seats available.");
            }

            // Update booking
            booking.Seats = updateDto.Seats;
            booking.CustomerName = updateDto.CustomerName ?? booking.CustomerName;
            booking.Email = updateDto.Email ?? booking.Email;
            booking.Phone = updateDto.Phone ?? booking.Phone;

            // Update available seats
            screening.AvailableSeats -= seatDifference;

            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating booking {BookingId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    // DELETE: api/bookings/5
    // Cancel booking
    [HttpDelete("{id}")]
    public async Task<IActionResult> CancelBooking(int id)
    {
        try
        {
            var booking = await _context.Bookings
                .Include(b => b.Screening)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (booking == null)
            {
                return NotFound($"Booking with ID {id} not found");
            }

            var screening = booking.Screening;
            if (screening != null)
            {
                // Return seats to available pool
                screening.AvailableSeats += booking.Seats;
            }

            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error canceling booking {BookingId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    private string GenerateBookingCode()
    {
        // Generate code like #CINE1234
        var random = new Random();
        var number = random.Next(1000, 9999);
        return $"#CINE{number}";
    }
}

// DTOs for booking operations
public class CreateBookingDto
{
    public int ScreeningId { get; set; }
    public required string CustomerName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public int Seats { get; set; }
}

public class UpdateBookingDto
{
    public int Seats { get; set; }
    public string? CustomerName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
}
