namespace WpfEventRecorder.SampleApp.Models;

/// <summary>
/// Represents a country.
/// </summary>
public class Country
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Code { get; set; } = "";

    public override string ToString() => Name;
}
