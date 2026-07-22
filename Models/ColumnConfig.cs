namespace PredatorWeb.Models;

/// <summary>
/// Representa la configuración de una columna en la grilla.
/// </summary>
public class ColumnConfig
{
    public required string ColumnName { get; set; }
    public bool IsVisible { get; set; } = true;
    public int DisplayOrder { get; set; }
    public bool ShowTotal { get; set; } = false;
    public bool IsNumeric { get; set; } = false;
    public string? CustomTitle { get; set; }

    // Nombre formateado para mostrar en UI (usa CustomTitle si existe, sino formatea ColumnName)
    public string DisplayName => !string.IsNullOrWhiteSpace(CustomTitle) ? CustomTitle : FormatColumnName(ColumnName);

    private string FormatColumnName(string columnName)
    {
        if (string.IsNullOrEmpty(columnName))
            return string.Empty;

        // Eliminar prefijos comunes
        var name = columnName;
        if (name.StartsWith("str_", StringComparison.OrdinalIgnoreCase))
            name = name[4..];
        else if (name.StartsWith("int_", StringComparison.OrdinalIgnoreCase))
            name = name[4..];
        else if (name.StartsWith("dec_", StringComparison.OrdinalIgnoreCase))
            name = name[4..];
        else if (name.StartsWith("bit_", StringComparison.OrdinalIgnoreCase))
            name = name[4..];
        else if (name.StartsWith("dat_", StringComparison.OrdinalIgnoreCase))
            name = name[4..];

        // Convertir a título (primera letra mayúscula)
        if (name.Length > 0)
            name = char.ToUpper(name[0]) + name[1..];

        return name;
    }
}
