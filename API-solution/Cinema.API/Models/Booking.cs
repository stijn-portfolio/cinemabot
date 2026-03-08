namespace Cinema.API.Models;

public class Booking
{
    public int Id { get; set; }
    public int ScreeningId { get; set; }
    public required string CustomerName { get; set; }
    public required string Email { get; set; }
    public required string Phone { get; set; }
    public int Seats { get; set; }
    public required string BookingCode { get; set; } // #CINE1234
    public DateTime BookedAt { get; set; }

    // Navigation property
    public Screening? Screening { get; set; }
}
