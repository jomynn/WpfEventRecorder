using WpfEventRecorder.SampleApp.Models;

namespace WpfEventRecorder.SampleApp.Services;

/// <summary>
/// API service interface for data operations.
/// </summary>
public interface IApiService
{
    // Customers
    Task<IEnumerable<Customer>> GetCustomersAsync();
    Task<Customer?> GetCustomerAsync(int id);
    Task<IEnumerable<Customer>> SearchCustomersAsync(string query);
    Task<Customer> CreateCustomerAsync(Customer customer);
    Task<Customer> UpdateCustomerAsync(Customer customer);
    Task DeleteCustomerAsync(int id);

    // Orders
    Task<IEnumerable<Order>> GetOrdersAsync();
    Task<Order?> GetOrderAsync(int id);
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId);
    Task<Order> CreateOrderAsync(Order order);
    Task CancelOrderAsync(int id);

    // Countries
    Task<IEnumerable<Country>> GetCountriesAsync();
}
