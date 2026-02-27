namespace HotelManagementSystem.Web.Models;

/// <summary>
/// Main DTO returned by GET api/dashboard/stats
/// Must match the API response shape exactly.
/// </summary>
public class DashboardDto
{
    public decimal TotalLaborCost { get; set; }
    public int TotalShifts { get; set; }

    /// <summary>
    /// ✅ Changed from List&lt;OccupancyDataPoint&gt; to List&lt;ChartPoint&gt;
    /// to match what the API actually returns.
    /// </summary>
    public List<ChartPoint> ForecastData { get; set; } = new();

    /// <summary>Optional — only rendered if API returns it.</summary>
    public List<LaborCostDataPoint>? LaborCostTrend { get; set; }
}

/// <summary>
/// Matches the API ChartPoint exactly:
/// { "dateLabel": "Feb 28", "value": 116 }
/// </summary>
public class ChartPoint
{
    public string DateLabel { get; set; } = "";
    public int Value { get; set; }
}

/// <summary>
/// For the optional labor cost trend bar chart.
/// </summary>
public class LaborCostDataPoint
{
    public DateTime Date { get; set; }
    public decimal Cost { get; set; }
}