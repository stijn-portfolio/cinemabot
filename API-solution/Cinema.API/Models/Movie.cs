namespace Cinema.API.Models;

public class Movie
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Genre { get; set; }
    public int Duration { get; set; } // minutes
    public decimal Rating { get; set; } // 1.0-10.0
    public required string PosterUrl { get; set; } // TMDb image URL

    // Navigation property
    public ICollection<Screening> Screenings { get; set; } = new List<Screening>();
}
