namespace WpfEventRecorder.SampleApp.Models;

/// <summary>
/// Represents a customer.
/// </summary>
public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Phone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; } = true;
    public int? CountryId { get; set; }
    public Country? Country { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
