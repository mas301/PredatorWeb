namespace PredatorWeb.Models;

public class ServerFilter
{
    // Filtros de texto con valor único (para cuando hay más de 50 valores o búsqueda manual)
    public Dictionary<string, string> ColumnFilters { get; set; } = new();

    // Filtros de texto con múltiples valores (para cuando hay ≤50 valores distintos)
    public Dictionary<string, List<string>> MultiValueFilters { get; set; } = new();

    // Filtros de texto con tipo de coincidencia
    public Dictionary<string, StringFilter> StringFilters { get; set; } = new();

    public Dictionary<string, DateFilter> DateFilters { get; set; } = new();
    public Dictionary<string, BooleanFilterType> BooleanFilters { get; set; } = new();
    public Dictionary<string, NumericFilter> NumericFilters { get; set; } = new();
    public string? SortColumn { get; set; }
    public bool SortAscending { get; set; } = true;
}

public class StringFilter
{
    public StringFilterType Type { get; set; }
    public string Value { get; set; } = "";
}

public class DateFilter
{
    public DateFilterType Type { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class NumericFilter
{
    public NumericFilterType Type { get; set; }
    public decimal? Value1 { get; set; }
    public decimal? Value2 { get; set; }  // Para el tipo "Between"
}
