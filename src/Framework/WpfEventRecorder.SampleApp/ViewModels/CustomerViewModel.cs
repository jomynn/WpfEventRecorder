using System;
using System.Windows.Input;
using WpfEventRecorder.Core.Attributes;
using WpfEventRecorder.SampleApp.Models;

namespace WpfEventRecorder.SampleApp.ViewModels
{
    /// <summary>
    /// ViewModel for editing a single customer
    /// </summary>
    [RecordViewModel("CustomerEditor")]
    public class CustomerViewModel : ViewModelBase
    {
        private int _id;
        private string _name = string.Empty;
        private string _email = string.Empty;
        private string _phone = string.Empty;
        private string _country = string.Empty;
        private string _company = string.Empty;
        private string _notes = string.Empty;
        private bool _isActive = true;
        private DateTime _createdDate = DateTime.Now;
        private bool _isDirty;
        private string _validationError;

        /// <summary>
        /// Customer ID
        /// </summary>
        [IgnoreRecording]
        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        /// <summary>
        /// Customer name
        /// </summary>
        [RecordProperty("Customer Name")]
        public string Name
        {
            get => _name;
            set
            {
                if (SetProperty(ref _name, value))
                {
                    IsDirty = true;
                    ValidateName();
                }
            }
        }

        /// <summary>
        /// Email address
        /// </summary>
        [RecordProperty("Email Address")]
        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    IsDirty = true;
                    ValidateEmail();
                }
            }
        }

        /// <summary>
        /// Phone number
        /// </summary>
        [RecordProperty]
        public string Phone
        {
            get => _phone;
            set
            {
                if (SetProperty(ref _phone, value))
                {
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Country
        /// </summary>
        [RecordProperty]
        public string Country
        {
            get => _country;
            set
            {
                if (SetProperty(ref _country, value))
                {
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Company name
        /// </summary>
        [RecordProperty]
        public string Company
        {
            get => _company;
            set
            {
                if (SetProperty(ref _company, value))
                {
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Notes
        /// </summary>
        [RecordProperty]
        public string Notes
        {
            get => _notes;
            set
            {
                if (SetProperty(ref _notes, value))
                {
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Whether the customer is active
        /// </summary>
        [RecordProperty("Active Status")]
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (SetProperty(ref _isActive, value))
                {
                    IsDirty = true;
                }
            }
        }

        /// <summary>
        /// Date when customer was created
        /// </summary>
        [IgnoreRecording]
        public DateTime CreatedDate
        {
            get => _createdDate;
            set => SetProperty(ref _createdDate, value);
        }

        /// <summary>
        /// Whether there are unsaved changes
        /// </summary>
        [IgnoreRecording]
        public bool IsDirty
        {
            get => _isDirty;
            set => SetProperty(ref _isDirty, value);
        }

        /// <summary>
        /// Validation error message
        /// </summary>
        [IgnoreRecording]
        public string ValidationError
        {
            get => _validationError;
            set => SetProperty(ref _validationError, value);
        }

        /// <summary>
        /// Whether the customer data is valid
        /// </summary>
        public bool IsValid => string.IsNullOrEmpty(ValidationError);

        /// <summary>
        /// Creates a new empty customer view model
        /// </summary>
        public CustomerViewModel()
        {
        }

        /// <summary>
        /// Creates a customer view model from a model
        /// </summary>
        public CustomerViewModel(Customer customer)
        {
            LoadFromModel(customer);
        }

        /// <summary>
        /// Loads data from a customer model
        /// </summary>
        public void LoadFromModel(Customer customer)
        {
            if (customer == null) return;

            Id = customer.Id;
            Name = customer.Name;
            Email = customer.Email;
            Phone = customer.Phone;
            Country = customer.Country;
            Company = customer.Company;
            Notes = customer.Notes;
            IsActive = customer.IsActive;
            CreatedDate = customer.CreatedDate;

            IsDirty = false;
        }

        /// <summary>
        /// Converts to a customer model
        /// </summary>
        public Customer ToModel()
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

        /// <summary>
        /// Clears all data
        /// </summary>
        public void Clear()
        {
            Id = 0;
            Name = string.Empty;
            Email = string.Empty;
            Phone = string.Empty;
            Country = string.Empty;
            Company = string.Empty;
            Notes = string.Empty;
            IsActive = true;
            CreatedDate = DateTime.Now;
            ValidationError = null;
            IsDirty = false;
        }

        private void ValidateName()
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ValidationError = "Name is required";
            }
            else
            {
                ValidationError = null;
            }
            OnPropertyChanged(nameof(IsValid));
        }

        private void ValidateEmail()
        {
            if (!string.IsNullOrWhiteSpace(Email) && !Email.Contains("@"))
            {
                ValidationError = "Invalid email format";
            }
            else if (string.IsNullOrWhiteSpace(ValidationError) || ValidationError == "Invalid email format")
            {
                ValidationError = null;
            }
            OnPropertyChanged(nameof(IsValid));
        }
    }
}
