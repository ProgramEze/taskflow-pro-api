namespace TaskFlowPro.Application.DTOs.Common;

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();

    public int TotalItems { get; set; }
}