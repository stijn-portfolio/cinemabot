namespace Bot.API.KernelResponses;

public class MoviesResponse
{
    public string? Answer { get; set; }
    public List<MovieDto>? Movies { get; set; }
    public string? Question { get; set; }
}

public class MovieDto
{
    public int Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Genre { get; set; }
    public int Duration { get; set; }
    public double Rating { get; set; }
    public string? PosterUrl { get; set; }
}
