using System.Text.Json.Serialization;

namespace PredatorWeb.Models
{
    /// <summary>
    /// Configuración completa de una grilla para persistencia
    /// </summary>
    public class GridConfiguration
    {
        [JsonPropertyName("Columnas")]
        public List<ColumnConfiguration> Columnas { get; set; } = new();

        [JsonPropertyName("FiltrosTexto")]
        public Dictionary<string, string> FiltrosTexto { get; set; } = new();

        [JsonPropertyName("FiltrosFecha")]
        public Dictionary<string, DateFilterConfig> FiltrosFecha { get; set; } = new();

        [JsonPropertyName("FiltrosBool")]
        public Dictionary<string, BooleanFilterType> FiltrosBool { get; set; } = new();

        [JsonPropertyName("FiltrosNumero")]
        public Dictionary<string, List<string>> FiltrosNumero { get; set; } = new();

        [JsonPropertyName("OrdenAscendente")]
        public bool OrdenAscendente { get; set; } = true;

        [JsonPropertyName("ColumnaOrden")]
        public string? ColumnaOrden { get; set; }
    }

    /// <summary>
    /// Configuración de una columna individual
    /// </summary>
    public class ColumnConfiguration
    {
        [JsonPropertyName("Header")]
        public string Header { get; set; } = "";

        [JsonPropertyName("Orden")]
        public int Orden { get; set; }

        [JsonPropertyName("Visible")]
        public bool Visible { get; set; } = true;

        [JsonPropertyName("CustomTitle")]
        public string? CustomTitle { get; set; }

        [JsonPropertyName("ShowTotal")]
        public bool ShowTotal { get; set; } = true;
    }

    /// <summary>
    /// Configuración de filtro de fecha para persistencia
    /// </summary>
    public class DateFilterConfig
    {
        [JsonPropertyName("Type")]
        public DateFilterType Type { get; set; }

        [JsonPropertyName("From")]
        public DateTime? From { get; set; }

        [JsonPropertyName("To")]
        public DateTime? To { get; set; }
    }
}
