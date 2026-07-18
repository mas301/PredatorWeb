namespace PredatorWeb.Models;

public class ListaPage
{
    public string Title { get; set; } = string.Empty;
    public string NombreEntidad { get; set; } = string.Empty;
    public int MenuId { get; set; }
    public Type? ComponentType { get; set; }
    public Dictionary<string, object>? Parameters { get; set; }
}
