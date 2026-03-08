using Cinema.API.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cinema.API.Data;

public static class DbInitializer
{
    public static async Task Initialize(CinemaDbContext context, IConfiguration configuration)
    {
        // Ensure database is created
        context.Database.EnsureCreated();

        // Check if data already exists
        if (context.Movies.Any())
        {
            return; // Database has been seeded
        }

        // Get TMDb API configuration
        var tmdbApiKey = configuration["TMDb:ApiKey"];
        var tmdbImageBaseUrl = configuration["TMDb:ImageBaseUrl"] ?? "https://image.tmdb.org/t/p/w500";

        if (string.IsNullOrEmpty(tmdbApiKey))
        {
            Console.WriteLine("Warning: TMDb API key not found. Using fallback movie data.");
            await SeedFallbackMovies(context, tmdbImageBaseUrl);
            return;
        }

        try
        {
            await SeedMoviesFromTMDb(context, tmdbApiKey, tmdbImageBaseUrl);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching from TMDb: {ex.Message}. Using fallback data.");
            await SeedFallbackMovies(context, tmdbImageBaseUrl);
        }
    }

    private static async Task SeedMoviesFromTMDb(CinemaDbContext context, string apiKey, string imageBaseUrl)
    {
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri("https://api.themoviedb.org/3/");

        // Fetch popular movies
        var response = await httpClient.GetAsync($"movie/popular?api_key={apiKey}&language=nl-NL&page=1");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        var tmdbResponse = JsonSerializer.Deserialize<TMDbResponse>(json);

        if (tmdbResponse?.Results == null || tmdbResponse.Results.Count == 0)
        {
            throw new Exception("No movies found from TMDb API");
        }

        var movies = new List<Movie>();

        // Take first 20 movies
        foreach (var tmdbMovie in tmdbResponse.Results.Take(20))
        {
            var movie = new Movie
            {
                Id = movies.Count + 1,
                Title = tmdbMovie.Title ?? "Unknown",
                Description = tmdbMovie.Overview ?? "No description available",
                Genre = GetGenreName(tmdbMovie.GenreIds?.FirstOrDefault() ?? 0),
                Duration = 120, // Default, would need additional API call to get exact duration
                Rating = (decimal)Math.Round((decimal)(tmdbMovie.VoteAverage ?? 0), 1),
                PosterUrl = !string.IsNullOrEmpty(tmdbMovie.PosterPath)
                    ? $"{imageBaseUrl}{tmdbMovie.PosterPath}"
                    : "https://via.placeholder.com/500x750?text=No+Poster"
            };

            movies.Add(movie);
        }

        context.Movies.AddRange(movies);
        await context.SaveChangesAsync();

        // Generate screenings for each movie
        await GenerateScreenings(context, movies);
    }

    private static async Task SeedFallbackMovies(CinemaDbContext context, string imageBaseUrl)
    {
        var movies = new List<Movie>
        {
            new Movie { Id = 1, Title = "Dune: Part Two", Description = "Paul Atreides unites with Chani and the Fremen.", Genre = "Sci-Fi", Duration = 166, Rating = 8.5m, PosterUrl = "https://image.tmdb.org/t/p/w500/8b8R8l88Qje9dn9OE8PY05Nxl1X.jpg" },
            new Movie { Id = 2, Title = "The Batman", Description = "Batman ventures into Gotham City's underworld.", Genre = "Action", Duration = 176, Rating = 7.9m, PosterUrl = "https://image.tmdb.org/t/p/w500/74xTEgt7R36Fpooo50r9T25onhq.jpg" },
            new Movie { Id = 3, Title = "Spider-Man: No Way Home", Description = "Peter Parker's secret identity is revealed.", Genre = "Action", Duration = 148, Rating = 8.3m, PosterUrl = "https://image.tmdb.org/t/p/w500/1g0dhYtq4irTY1GPXvft6k4YLjm.jpg" },
            new Movie { Id = 4, Title = "Oppenheimer", Description = "The story of J. Robert Oppenheimer.", Genre = "Drama", Duration = 180, Rating = 8.6m, PosterUrl = "https://image.tmdb.org/t/p/w500/8Gxv8gSFCU0XGDykEGv7zR1n2ua.jpg" },
            new Movie { Id = 5, Title = "Barbie", Description = "Barbie and Ken go on an adventure.", Genre = "Comedy", Duration = 114, Rating = 7.2m, PosterUrl = "https://image.tmdb.org/t/p/w500/iuFNMS8U5cb6xfzi51Dbkovj7vM.jpg" },
            new Movie { Id = 6, Title = "Inception", Description = "A thief who steals corporate secrets.", Genre = "Sci-Fi", Duration = 148, Rating = 8.8m, PosterUrl = "https://image.tmdb.org/t/p/w500/9gk7adHYeDvHkCSEqAvQNLV5Uge.jpg" },
            new Movie { Id = 7, Title = "The Shawshank Redemption", Description = "Two imprisoned men bond over years.", Genre = "Drama", Duration = 142, Rating = 9.3m, PosterUrl = "https://image.tmdb.org/t/p/w500/q6y0Go1tsGEsmtFryDOJo3dEmqu.jpg" },
            new Movie { Id = 8, Title = "Interstellar", Description = "A team of explorers travel through a wormhole.", Genre = "Sci-Fi", Duration = 169, Rating = 8.7m, PosterUrl = "https://image.tmdb.org/t/p/w500/gEU2QniE6E77NI6lCU6MxlNBvIx.jpg" },
            new Movie { Id = 9, Title = "The Dark Knight", Description = "Batman must accept one of the greatest tests.", Genre = "Action", Duration = 152, Rating = 9.0m, PosterUrl = "https://image.tmdb.org/t/p/w500/qJ2tW6WMUDux911r6m7haRef0WH.jpg" },
            new Movie { Id = 10, Title = "Pulp Fiction", Description = "The lives of two mob hitmen intertwine.", Genre = "Crime", Duration = 154, Rating = 8.9m, PosterUrl = "https://image.tmdb.org/t/p/w500/d5iIlFn5s0ImszYzBPb8JPIfbXD.jpg" }
        };

        context.Movies.AddRange(movies);
        await context.SaveChangesAsync();

        await GenerateScreenings(context, movies);
    }

    private static async Task GenerateScreenings(CinemaDbContext context, List<Movie> movies)
    {
        var screenings = new List<Screening>();
        var random = new Random();
        var rooms = new[] { "Zaal 1", "Zaal 2", "Zaal 3" };
        var screeningId = 1;

        foreach (var movie in movies)
        {
            // Generate 3-5 screenings per movie
            var screeningCount = random.Next(3, 6);

            for (int i = 0; i < screeningCount; i++)
            {
                var daysFromNow = random.Next(0, 7); // Next 7 days
                var hour = random.Next(12, 23); // Between 12:00 and 23:00
                var minute = random.Next(0, 2) * 30; // 00 or 30

                screenings.Add(new Screening
                {
                    Id = screeningId++,
                    MovieId = movie.Id,
                    DateTime = DateTime.Today.AddDays(daysFromNow).AddHours(hour).AddMinutes(minute),
                    Room = rooms[random.Next(rooms.Length)],
                    TotalSeats = 100,
                    AvailableSeats = random.Next(20, 100),
                    Price = 9.50m + (decimal)random.Next(0, 5) // €9.50 - €14.00
                });
            }
        }

        context.Screenings.AddRange(screenings);
        await context.SaveChangesAsync();
    }

    private static string GetGenreName(int genreId)
    {
        return genreId switch
        {
            28 => "Action",
            12 => "Adventure",
            16 => "Animation",
            35 => "Comedy",
            80 => "Crime",
            99 => "Documentary",
            18 => "Drama",
            10751 => "Family",
            14 => "Fantasy",
            36 => "History",
            27 => "Horror",
            10402 => "Music",
            9648 => "Mystery",
            10749 => "Romance",
            878 => "Sci-Fi",
            10770 => "TV Movie",
            53 => "Thriller",
            10752 => "War",
            37 => "Western",
            _ => "General"
        };
    }

    // TMDb API response models
    private class TMDbResponse
    {
        [JsonPropertyName("results")]
        public List<TMDbMovie>? Results { get; set; }
    }

    private class TMDbMovie
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("overview")]
        public string? Overview { get; set; }

        [JsonPropertyName("poster_path")]
        public string? PosterPath { get; set; }

        [JsonPropertyName("vote_average")]
        public double? VoteAverage { get; set; }

        [JsonPropertyName("genre_ids")]
        public List<int>? GenreIds { get; set; }
    }
}
