namespace Cinema.API.Models;

public class Screening
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public DateTime DateTime { get; set; }
    public required string Room { get; set; } // "Zaal 1", "Zaal 2", etc.
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public decimal Price { get; set; }

    // Navigation properties
    public Movie? Movie { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
