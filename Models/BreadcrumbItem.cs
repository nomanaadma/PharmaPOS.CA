namespace PharmaPOS_MVC.Models;

public class BreadcrumbItem(string title, string? url = "")
{
    public string Title { get; } = title;
    public string Url { get; } = url ?? "";
}
