namespace PredatorWeb.Models;

public class SortFilterResult
{
    public bool SortAscending { get; set; }
    public string FilterValue { get; set; } = "";
    public StringFilterType StringFilterType { get; set; } = StringFilterType.Contains;
    public List<string> SelectedValues { get; set; } = new();
    public DateFilterType DateFilterType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public BooleanFilterType BooleanFilterType { get; set; }
    public NumericFilterType NumericFilterType { get; set; }
    public decimal? NumericValue1 { get; set; }
    public decimal? NumericValue2 { get; set; }
}
