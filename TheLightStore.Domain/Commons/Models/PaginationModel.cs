using System;

namespace TheLightStore.Domain.Commons.Models;

public class PaginationModel<T> where T : class
{
    public Pagination? Pagination { get; set; }
    public required IEnumerable<T> Records { get; set; }
}
public class Pagination
{
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PerPage { get; set; }
    public int? NextPage { get; set; } = null;
    public int? PrevPage { get; set; } = null;
}
