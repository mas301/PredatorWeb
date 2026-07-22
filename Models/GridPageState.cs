using System.Data;

namespace PredatorWeb.Models;

/// <summary>
/// Encapsula todo el estado de una instancia de grilla.
/// Cada pestaña tiene su propia instancia independiente.
/// </summary>
public class GridPageState
{
    // Datos y carga
    public DataTable? GridData { get; set; }
    public bool IsLoading { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public string? StackTrace { get; set; }

    // Control de carga asíncrona
    public CancellationTokenSource? CancellationTokenSource { get; set; }
    public string LoadingMessage { get; set; } = "Preparando consulta...";
    public int ProgressPercentage { get; set; } = 0;
    public bool ShowLoadingDialog { get; set; } = false;

    // Ordenamiento
    public string? SortColumn { get; set; }
    public bool SortAscending { get; set; } = true;

    // Diálogo de ordenamiento y filtrado
    public bool ShowSortDialog { get; set; } = false;
    public string? SelectedColumnForSort { get; set; }
    public bool TempSortAscending { get; set; } = true;
    public string TempFilterValue { get; set; } = "";

    // Filtrado de fechas
    public DateFilterType TempDateFilterType { get; set; } = DateFilterType.None;
    public DateTime? TempDateFrom { get; set; }
    public DateTime? TempDateTo { get; set; }

    // Filtrado de booleanos
    public BooleanFilterType TempBooleanFilterType { get; set; } = BooleanFilterType.All;

    // Filtros de selección múltiple (para columnas con pocos valores distintos)
    public List<string>? AvailableFilterValues { get; set; } = null;
    public HashSet<string> SelectedFilterValues { get; set; } = new();
    public bool IsLoadingFilterValues { get; set; } = false;

    // Filtros activos por columna
    public Dictionary<string, string> ColumnFilters { get; } = new();
    public Dictionary<string, List<string>> MultiValueFilters { get; } = new();
    public Dictionary<string, (DateFilterType Type, DateTime? From, DateTime? To)> DateFilters { get; } = new();
    public Dictionary<string, BooleanFilterType> BooleanFilters { get; } = new();

    // Selección de filas
    public HashSet<DataRow> SelectedRows { get; } = new();

    // Paginación
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 1000;
    public int TotalPages => GridData == null || GridData.Rows.Count == 0 
        ? 1 
        : (int)Math.Ceiling((double)GridData.Rows.Count / PageSize);
    public bool ShowPagination => GridData != null && GridData.Rows.Count > PageSize;

    // Configuración de columnas
    public bool ShowColumnDialog { get; set; } = false;
    public List<ColumnConfig> ColumnConfigs { get; set; } = new();

    // Vista previa de impresión
    public bool ShowPrintPreview { get; set; } = false;
    public DataTable? PrintPreviewData { get; set; } = null;

    // Métodos de utilidad
    public bool HasActiveFilters()
    {
        return ColumnFilters.Any() || 
               MultiValueFilters.Any() ||
               DateFilters.Any() || 
               BooleanFilters.Any(f => f.Value != BooleanFilterType.All);
    }

    public void ClearAllFilters()
    {
        ColumnFilters.Clear();
        MultiValueFilters.Clear();
        DateFilters.Clear();
        BooleanFilters.Clear();
    }

    public void Dispose()
    {
        CancellationTokenSource?.Cancel();
        CancellationTokenSource?.Dispose();
        CancellationTokenSource = null;
        SelectedRows.Clear();
    }
}

public enum DateFilterType
{
    None,
    Today,
    ThisWeek,
    ThisMonth,
    ThisYear,
    CustomRange
}

public enum BooleanFilterType
{
    All,
    Checked,
    Unchecked
}
