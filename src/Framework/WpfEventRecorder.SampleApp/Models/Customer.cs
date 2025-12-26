using System;

namespace WpfEventRecorder.SampleApp.Models
{
    /// <summary>
    /// Customer model
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Customer name
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Email address
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Phone number
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Country
        /// </summary>
        public string Country { get; set; } = string.Empty;

        /// <summary>
        /// Company name
        /// </summary>
        public string Company { get; set; } = string.Empty;

        /// <summary>
        /// Notes about the customer
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Whether the customer is active
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Date when the customer was created
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Creates a copy of this customer
        /// </summary>
        public Customer Clone()
        {
            return new Customer
            {
                Id = Id,
                Name = Name,
                Email = Email,
                Phone = Phone,
                Country = Country,
                Company = Company,
                Notes = Notes,
                IsActive = IsActive,
                CreatedDate = CreatedDate
            };
        }
    }
}
