namespace MVC.Models;

public class ApiResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, string>? Errors { get; set; }
    public string? RedirectUrl { get; set; }
    public bool Notify { get; set; } = false;
}
