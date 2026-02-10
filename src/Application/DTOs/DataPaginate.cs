namespace Bookazone.Application.DTOs;

public class DataPaginate<T>
{
    public List<T>? Data { get; set; } = new List<T>();
    public int? Pages { get; set; } = null;
    public int? PageIndex { get; set; } = null;
}