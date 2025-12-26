using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WpfEventRecorder.SampleApp.Models;

namespace WpfEventRecorder.SampleApp.Services
{
    /// <summary>
    /// Mock customer service with HTTP client support for recording demo
    /// </summary>
    public class CustomerService : ICustomerService
    {
        private readonly HttpClient _httpClient;
        private readonly List<Customer> _customers;
        private int _nextId = 1;

        /// <summary>
        /// Creates a new customer service
        /// </summary>
        public CustomerService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

            // Initialize with sample data
            _customers = new List<Customer>
            {
                new Customer { Id = _nextId++, Name = "John Doe", Email = "john@example.com", Phone = "555-0101", Country = "USA", Company = "Acme Inc", IsActive = true },
                new Customer { Id = _nextId++, Name = "Jane Smith", Email = "jane@example.com", Phone = "555-0102", Country = "UK", Company = "Tech Corp", IsActive = true },
                new Customer { Id = _nextId++, Name = "Bob Johnson", Email = "bob@example.com", Phone = "555-0103", Country = "Canada", Company = "Global Ltd", IsActive = false },
                new Customer { Id = _nextId++, Name = "Alice Brown", Email = "alice@example.com", Phone = "555-0104", Country = "Australia", Company = "Down Under Co", IsActive = true },
                new Customer { Id = _nextId++, Name = "Charlie Wilson", Email = "charlie@example.com", Phone = "555-0105", Country = "Germany", Company = "Euro Tech", IsActive = true },
            };
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Customer>> GetAllAsync()
        {
            // Simulate API call
            try
            {
                await _httpClient.GetAsync("https://jsonplaceholder.typicode.com/users");
            }
            catch
            {
                // Ignore HTTP errors for mock data
            }

            await Task.Delay(100); // Simulate network delay
            return _customers.Select(c => c.Clone());
        }

        /// <inheritdoc />
        public async Task<Customer> GetByIdAsync(int id)
        {
            await Task.Delay(50);
            var customer = _customers.FirstOrDefault(c => c.Id == id);
            return customer?.Clone();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Customer>> SearchAsync(string searchTerm)
        {
            await Task.Delay(100);

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return _customers.Select(c => c.Clone());
            }

            var term = searchTerm.ToLowerInvariant();
            return _customers
                .Where(c => c.Name.ToLowerInvariant().Contains(term) ||
                            c.Email.ToLowerInvariant().Contains(term) ||
                            c.Company.ToLowerInvariant().Contains(term))
                .Select(c => c.Clone());
        }

        /// <inheritdoc />
        public async Task<Customer> CreateAsync(Customer customer)
        {
            // Simulate POST API call
            try
            {
                var json = JsonSerializer.Serialize(customer);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await _httpClient.PostAsync("https://jsonplaceholder.typicode.com/users", content);
            }
            catch
            {
                // Ignore HTTP errors for mock data
            }

            await Task.Delay(100);

            var newCustomer = customer.Clone();
            newCustomer.Id = _nextId++;
            newCustomer.CreatedDate = DateTime.Now;
            _customers.Add(newCustomer);

            return newCustomer.Clone();
        }

        /// <inheritdoc />
        public async Task<Customer> UpdateAsync(Customer customer)
        {
            // Simulate PUT API call
            try
            {
                var json = JsonSerializer.Serialize(customer);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                await _httpClient.PutAsync($"https://jsonplaceholder.typicode.com/users/{customer.Id}", content);
            }
            catch
            {
                // Ignore HTTP errors for mock data
            }

            await Task.Delay(100);

            var existing = _customers.FirstOrDefault(c => c.Id == customer.Id);
            if (existing == null)
            {
                throw new InvalidOperationException($"Customer with ID {customer.Id} not found");
            }

            existing.Name = customer.Name;
            existing.Email = customer.Email;
            existing.Phone = customer.Phone;
            existing.Country = customer.Country;
            existing.Company = customer.Company;
            existing.Notes = customer.Notes;
            existing.IsActive = customer.IsActive;

            return existing.Clone();
        }

        /// <inheritdoc />
        public async Task<bool> DeleteAsync(int id)
        {
            // Simulate DELETE API call
            try
            {
                await _httpClient.DeleteAsync($"https://jsonplaceholder.typicode.com/users/{id}");
            }
            catch
            {
                // Ignore HTTP errors for mock data
            }

            await Task.Delay(100);

            var customer = _customers.FirstOrDefault(c => c.Id == id);
            if (customer == null)
            {
                return false;
            }

            _customers.Remove(customer);
            return true;
        }
    }
}
