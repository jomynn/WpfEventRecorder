using System.Collections.Generic;
using System.Threading.Tasks;
using WpfEventRecorder.SampleApp.Models;

namespace WpfEventRecorder.SampleApp.Services
{
    /// <summary>
    /// Interface for customer service operations
    /// </summary>
    public interface ICustomerService
    {
        /// <summary>
        /// Gets all customers
        /// </summary>
        Task<IEnumerable<Customer>> GetAllAsync();

        /// <summary>
        /// Gets a customer by ID
        /// </summary>
        Task<Customer> GetByIdAsync(int id);

        /// <summary>
        /// Searches for customers by name
        /// </summary>
        Task<IEnumerable<Customer>> SearchAsync(string searchTerm);

        /// <summary>
        /// Creates a new customer
        /// </summary>
        Task<Customer> CreateAsync(Customer customer);

        /// <summary>
        /// Updates an existing customer
        /// </summary>
        Task<Customer> UpdateAsync(Customer customer);

        /// <summary>
        /// Deletes a customer
        /// </summary>
        Task<bool> DeleteAsync(int id);
    }
}
