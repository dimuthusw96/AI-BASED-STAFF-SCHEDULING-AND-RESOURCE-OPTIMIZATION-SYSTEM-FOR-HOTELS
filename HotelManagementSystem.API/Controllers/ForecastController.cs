using Microsoft.AspNetCore.Mvc;
using HotelManagementSystem.API_.Data;
using HotelManagementSystem.API_.Models;
using Newtonsoft.Json;

namespace HotelManagementSystem.API_.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForecastController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public ForecastController(ApplicationDbContext context, IHttpClientFactory clientFactory)
        {
            _context = context;
            _httpClient = clientFactory.CreateClient();
        }

        // POST api/forecast/generate?days=7
        [HttpPost("generate")]
        public async Task<IActionResult> GenerateForecast(int days = 7)
        {
            try
            {
                // 1. Call the Python API (Running locally on Port 5000)
                // Ensure your Python app.py is running!
                string pythonUrl = $"http://127.0.0.1:5000/predict?days={days}";
                var response = await _httpClient.GetAsync(pythonUrl);

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Error communicating with ML Service.");
                }

                var jsonString = await response.Content.ReadAsStringAsync();

                // 2. Deserialize JSON (Convert Python response to C# Objects)
                var predictions = JsonConvert.DeserializeObject<List<PythonForecastDto>>(jsonString);

                // 3. Save to SQL Database
                foreach (var p in predictions)
                {
                    var forecast = new Forecast
                    {
                        ForecastDate = DateTime.Parse(p.date),
                        OccupancyCount = p.predicted_occupancy
                    };
                    _context.Forecasts.Add(forecast);
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Forecast generated and saved successfully", data = predictions });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }

        // POST api/forecast/generate?days=7
        [HttpPost("generateForcast")]
        public async Task<IActionResult> GenerateForecastDB(int days = 7)
        {
            try
            {
                // 1. Define the Python URL (The app you just created)
                string pythonUrl = $"http://127.0.0.1:5000/predict?days={days}";

                // 2. Call the Python API
                var response = await _httpClient.GetAsync(pythonUrl);

                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Error communicating with Python ML Service. Is app.py running?");
                }

                // 3. Read the JSON response
                var jsonString = await response.Content.ReadAsStringAsync();
                var predictions = JsonConvert.DeserializeObject<List<PythonForecastDto>>(jsonString);

                if (predictions == null || predictions.Count == 0)
                {
                    return BadRequest("Python service returned no data.");
                }

                // 4. Save to SQL Database
                // First, optionally clear old forecasts to avoid duplicates (optional)
                // _context.Forecasts.RemoveRange(_context.Forecasts); 

                foreach (var p in predictions)
                {
                    var forecast = new Forecast
                    {
                        ForecastDate = DateTime.Parse(p.date),
                        OccupancyCount = p.predicted_occupancy
                    };
                    _context.Forecasts.Add(forecast);
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Success! Forecast generated and saved to SQL.", count = predictions.Count });
            }
            catch (Exception ex)
            {
                // This catches connection errors (e.g., if Python is closed)
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }

    // Helper class to match Python JSON structure
    public class PythonForecastDto
    {
        public string date { get; set; }
        public int predicted_occupancy { get; set; }
    }
}
