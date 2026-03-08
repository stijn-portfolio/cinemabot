namespace Bot.API.KernelResponses;

public class BookingResponse
{
    public string? Answer { get; set; }
    public BookingDto? Booking { get; set; }
    public string? Question { get; set; }
}

public class BookingDto
{
    public int Id { get; set; }
    public int ScreeningId { get; set; }
    public string? CustomerName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int Seats { get; set; }
    public string? BookingCode { get; set; }
    public DateTime BookedAt { get; set; }
    public ScreeningDto? Screening { get; set; }
}
