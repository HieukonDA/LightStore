using System.ComponentModel.DataAnnotations;

namespace TheLightStore.Dtos.Paging;
    
public class PagedRequest
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public string? Sort { get; set; } = "";
    public string? Search { get; set; } = "";
}