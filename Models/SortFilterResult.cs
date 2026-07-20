namespace PredatorWeb.Models;

public class SortFilterResult
{
    public bool SortAscending { get; set; }
    public string FilterValue { get; set; } = "";
    public List<string> SelectedValues { get; set; } = new();
    public DateFilterType DateFilterType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public BooleanFilterType BooleanFilterType { get; set; }
}
