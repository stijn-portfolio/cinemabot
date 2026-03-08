using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace Bot.Core.Plugins;

public class CinemaPlugin
{
    private readonly HttpClient _httpClient;
    private readonly string _apiBaseUrl;

    public CinemaPlugin(string apiBaseUrl = "http://localhost:5001")
    {
        _apiBaseUrl = apiBaseUrl;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(_apiBaseUrl)
        };
    }

    [KernelFunction]
    [Description("Get all available movies currently showing in the cinema")]
    public async Task<string> GetMoviesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/movies");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [KernelFunction]
    [Description("Get movies filtered by genre (Action, Drama, Sci-Fi, Comedy, etc.)")]
    public async Task<string> GetMoviesByGenreAsync(
        [Description("The genre to filter by (e.g., Action, Drama, Sci-Fi)")] string genre)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/movies/genre/{genre}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [KernelFunction]
    [Description("Get all screenings for a specific movie, optionally filtered by date")]
    public async Task<string> GetScreeningsAsync(
        [Description("The ID of the movie")] int movieId,
        [Description("Optional date to filter screenings (format: yyyy-MM-dd)")] string? date = null)
    {
        try
        {
            string endpoint = $"/api/screenings/movie/{movieId}";

            // Als date is opgegeven, gebruik screenings/date endpoint
            if (!string.IsNullOrEmpty(date))
            {
                endpoint = $"/api/screenings/date/{date}";
            }

            var response = await _httpClient.GetAsync(endpoint);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [KernelFunction]
    [Description("Book tickets for a movie screening. Returns booking confirmation with code.")]
    public async Task<string> BookScreeningAsync(
        [Description("The ID of the screening to book")] int screeningId,
        [Description("Customer's full name")] string customerName,
        [Description("Customer's email address")] string email,
        [Description("Customer's phone number")] string phone,
        [Description("Number of seats to book")] int seats)
    {
        try
        {
            var bookingRequest = new
            {
                screeningId,
                customerName,
                email,
                phone,
                seats
            };

            var content = new StringContent(
                JsonSerializer.Serialize(bookingRequest),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/api/bookings", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [KernelFunction]
    [Description("Update an existing booking to change the number of seats")]
    public async Task<string> UpdateBookingAsync(
        [Description("The ID of the booking to update")] int bookingId,
        [Description("New number of seats")] int seats)
    {
        try
        {
            var updateRequest = new
            {
                seats
            };

            var content = new StringContent(
                JsonSerializer.Serialize(updateRequest),
                System.Text.Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PutAsync($"/api/bookings/{bookingId}", content);
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Serialize(new
            {
                success = true,
                message = $"Booking updated successfully to {seats} seats"
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [KernelFunction]
    [Description("Cancel an existing booking and return the seats to availability")]
    public async Task<string> CancelBookingAsync(
        [Description("The ID of the booking to cancel")] int bookingId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/bookings/{bookingId}");
            response.EnsureSuccessStatusCode();

            return JsonSerializer.Serialize(new
            {
                success = true,
                message = "Booking cancelled successfully"
            });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }

    [KernelFunction]
    [Description("Get all bookings for a specific email address")]
    public async Task<string> GetMyBookingsAsync(
        [Description("The email address to search bookings for")] string email)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/bookings/email/{email}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new { error = ex.Message });
        }
    }
}
