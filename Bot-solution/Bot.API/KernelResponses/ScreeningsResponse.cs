namespace Bot.API.KernelResponses;

public class ScreeningsResponse
{
    public string? Answer { get; set; }
    public List<ScreeningDto>? Screenings { get; set; }
    public string? Question { get; set; }
}

public class ScreeningDto
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public DateTime DateTime { get; set; }
    public string? Room { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public decimal Price { get; set; }
    public MovieDto? Movie { get; set; }
}
