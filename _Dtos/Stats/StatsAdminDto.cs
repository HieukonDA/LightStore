namespace TheLightStore.Dtos.Stats;
public class CategoryStatsDto
{
    public string CategoryName { get; set; }
    public int ProductCount { get; set; }
    public double Percentage { get; set; }
}

public class SalesDataPoint  
{
    public string Month { get; set; } // "2025-09"
    public int Year { get; set; }     // 2025
    public int MonthNumber { get; set; } // 9
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; } // Nếu muốn đếm số đơn
}