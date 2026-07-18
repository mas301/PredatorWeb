namespace PredatorWeb.Models;

public class GrlMenu
{
    public int MenuId { get; set; }
    public int? MenuGrupalId { get; set; }
    public string Menu { get; set; } = string.Empty;
    public string NombreEntidad { get; set; } = string.Empty;
    public string CodigoMenu { get; set; } = string.Empty;
    public bool Abierto { get; set; }
}
