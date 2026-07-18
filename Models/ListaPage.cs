namespace PredatorWeb.Models;

public class ListaPage
{
    public required string Title { get; set; } = string.Empty;
    public required string NombreEntidad { get; set; } = string.Empty;
    public int MenuId { get; set; }
    public Type? ComponentType { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}
