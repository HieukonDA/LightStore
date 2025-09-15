public class RequestInfo
{
    public DateTime WindowStart { get; set; } = DateTime.UtcNow;
    public int RequestCount { get; set; } = 0;
    public DateTime LastRequest { get; set; } = DateTime.UtcNow;
}

public class RateLimitConfig
{
    public int MaxRequests { get; set; }
    public TimeSpan Window { get; set; }
}

public class RateLimitStatus
{
    public bool IsAllowed { get; set; }
    public int RemainingRequests { get; set; }
    public DateTime ResetTime { get; set; }
    public int RetryAfterSeconds { get; set; }
}
