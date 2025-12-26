using System.Net.Http;
using System.Net.Http.Json;
using WpfEventRecorder.Core.Recording;
using WpfEventRecorder.SampleApp.Models;

namespace WpfEventRecorder.SampleApp.Services;

/// <summary>
/// Implementation of API service with mock data.
/// In production, this would call a real API.
/// </summary>
public class ApiService : IApiService
{
    private readonly HttpClient _httpClient;
    private readonly List<Customer> _customers;
    private readonly List<Order> _orders;
    private readonly List<Country> _countries;

    public ApiService()
    {
        // Use recording HTTP client
        _httpClient = RecordingBootstrapper.CreateRecordingHttpClient();

        // Initialize mock data
        _countries = new List<Country>
        {
            new() { Id = 1, Name = "United States", Code = "US" },
            new() { Id = 2, Name = "United Kingdom", Code = "UK" },
            new() { Id = 3, Name = "Canada", Code = "CA" },
            new() { Id = 4, Name = "Germany", Code = "DE" },
            new() { Id = 5, Name = "France", Code = "FR" }
        };

        _customers = new List<Customer>
        {
            new() { Id = 1, FirstName = "John", LastName = "Doe", Email = "john.doe@example.com", Phone = "555-0101", DateOfBirth = new DateTime(1985, 3, 15), IsActive = true, CountryId = 1 },
            new() { Id = 2, FirstName = "Jane", LastName = "Smith", Email = "jane.smith@example.com", Phone = "555-0102", DateOfBirth = new DateTime(1990, 7, 22), IsActive = true, CountryId = 2 },
            new() { Id = 3, FirstName = "Bob", LastName = "Johnson", Email = "bob.johnson@example.com", Phone = "555-0103", DateOfBirth = new DateTime(1978, 11, 8), IsActive = false, CountryId = 1 },
            new() { Id = 4, FirstName = "Alice", LastName = "Brown", Email = "alice.brown@example.com", Phone = "555-0104", DateOfBirth = new DateTime(1995, 1, 30), IsActive = true, CountryId = 3 },
            new() { Id = 5, FirstName = "Charlie", LastName = "Wilson", Email = "charlie.wilson@example.com", Phone = "555-0105", DateOfBirth = new DateTime(1982, 5, 12), IsActive = true, CountryId = 4 }
        };

        _orders = new List<Order>
        {
            new() { Id = 1001, CustomerId = 1, OrderDate = DateTime.Now.AddDays(-5), TotalAmount = 150.00m, Status = OrderStatus.Delivered },
            new() { Id = 1002, CustomerId = 2, OrderDate = DateTime.Now.AddDays(-3), TotalAmount = 75.50m, Status = OrderStatus.Shipped },
            new() { Id = 1003, CustomerId = 1, OrderDate = DateTime.Now.AddDays(-1), TotalAmount = 220.00m, Status = OrderStatus.Processing },
            new() { Id = 1004, CustomerId = 4, OrderDate = DateTime.Now, TotalAmount = 89.99m, Status = OrderStatus.Pending },
            new() { Id = 1005, CustomerId = 3, OrderDate = DateTime.Now.AddDays(-10), TotalAmount = 450.00m, Status = OrderStatus.Cancelled }
        };
    }

    public Task<IEnumerable<Customer>> GetCustomersAsync()
    {
        return Task.FromResult(_customers.AsEnumerable());
    }

    public Task<Customer?> GetCustomerAsync(int id)
    {
        return Task.FromResult(_customers.FirstOrDefault(c => c.Id == id));
    }

    public Task<IEnumerable<Customer>> SearchCustomersAsync(string query)
    {
        var results = _customers.Where(c =>
            c.FirstName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            c.LastName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            c.Email.Contains(query, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(results);
    }

    public Task<Customer> CreateCustomerAsync(Customer customer)
    {
        customer.Id = _customers.Max(c => c.Id) + 1;
        customer.CreatedAt = DateTime.Now;
        _customers.Add(customer);
        return Task.FromResult(customer);
    }

    public Task<Customer> UpdateCustomerAsync(Customer customer)
    {
        var existing = _customers.FirstOrDefault(c => c.Id == customer.Id);
        if (existing != null)
        {
            existing.FirstName = customer.FirstName;
            existing.LastName = customer.LastName;
            existing.Email = customer.Email;
            existing.Phone = customer.Phone;
            existing.DateOfBirth = customer.DateOfBirth;
            existing.IsActive = customer.IsActive;
            existing.CountryId = customer.CountryId;
            existing.UpdatedAt = DateTime.Now;
        }
        return Task.FromResult(customer);
    }

    public Task DeleteCustomerAsync(int id)
    {
        var customer = _customers.FirstOrDefault(c => c.Id == id);
        if (customer != null)
        {
            _customers.Remove(customer);
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Order>> GetOrdersAsync()
    {
        return Task.FromResult(_orders.AsEnumerable());
    }

    public Task<Order?> GetOrderAsync(int id)
    {
        return Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));
    }

    public Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId)
    {
        return Task.FromResult(_orders.Where(o => o.CustomerId == customerId));
    }

    public Task<Order> CreateOrderAsync(Order order)
    {
        order.Id = _orders.Max(o => o.Id) + 1;
        order.OrderDate = DateTime.Now;
        _orders.Add(order);
        return Task.FromResult(order);
    }

    public Task CancelOrderAsync(int id)
    {
        var order = _orders.FirstOrDefault(o => o.Id == id);
        if (order != null)
        {
            order.Status = OrderStatus.Cancelled;
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<Country>> GetCountriesAsync()
    {
        return Task.FromResult(_countries.AsEnumerable());
    }
}
