using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfEventRecorder.Core.Attributes;
using WpfEventRecorder.SampleApp.Models;
using WpfEventRecorder.SampleApp.Services;

namespace WpfEventRecorder.SampleApp.ViewModels
{
    /// <summary>
    /// ViewModel for the customer list and editor
    /// </summary>
    [RecordViewModel("CustomerList")]
    public class CustomerListViewModel : ViewModelBase
    {
        private readonly ICustomerService _customerService;
        private ObservableCollection<Customer> _customers;
        private Customer _selectedCustomer;
        private CustomerViewModel _editorViewModel;
        private string _searchText = string.Empty;
        private string _statusMessage = "Ready";
        private bool _isLoading;
        private bool _isEditing;

        /// <summary>
        /// Collection of customers
        /// </summary>
        public ObservableCollection<Customer> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        /// <summary>
        /// Currently selected customer
        /// </summary>
        [RecordProperty("Selected Customer")]
        public Customer SelectedCustomer
        {
            get => _selectedCustomer;
            set
            {
                if (SetProperty(ref _selectedCustomer, value))
                {
                    if (value != null)
                    {
                        EditorViewModel.LoadFromModel(value);
                        IsEditing = true;
                    }
                    ((RelayCommand)DeleteCommand).RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// Editor view model for the selected customer
        /// </summary>
        public CustomerViewModel EditorViewModel
        {
            get => _editorViewModel;
            set => SetProperty(ref _editorViewModel, value);
        }

        /// <summary>
        /// Search text
        /// </summary>
        [RecordProperty("Search Text")]
        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        /// <summary>
        /// Status message
        /// </summary>
        [IgnoreRecording]
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// Whether data is loading
        /// </summary>
        [IgnoreRecording]
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        /// <summary>
        /// Whether in edit mode
        /// </summary>
        [IgnoreRecording]
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        // Commands
        public ICommand LoadCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand NewCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand CancelCommand { get; }

        /// <summary>
        /// Creates a new customer list view model
        /// </summary>
        public CustomerListViewModel(ICustomerService customerService)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _customers = new ObservableCollection<Customer>();
            _editorViewModel = new CustomerViewModel();

            // Initialize commands
            LoadCommand = new AsyncRelayCommand(LoadCustomersAsync);
            SearchCommand = new AsyncRelayCommand(SearchCustomersAsync);
            NewCommand = new RelayCommand(NewCustomer);
            SaveCommand = new AsyncRelayCommand(SaveCustomerAsync, _ => EditorViewModel.IsValid && EditorViewModel.IsDirty);
            DeleteCommand = new AsyncRelayCommand(DeleteCustomerAsync, _ => SelectedCustomer != null);
            CancelCommand = new RelayCommand(CancelEdit);
        }

        /// <summary>
        /// Loads all customers
        /// </summary>
        [RecordApiCall("Load Customers")]
        public async Task LoadCustomersAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading customers...";

                var customers = await _customerService.GetAllAsync();

                Customers.Clear();
                foreach (var customer in customers)
                {
                    Customers.Add(customer);
                }

                StatusMessage = $"Loaded {Customers.Count} customers";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading customers: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Searches for customers
        /// </summary>
        [RecordApiCall("Search Customers")]
        public async Task SearchCustomersAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = $"Searching for '{SearchText}'...";

                var customers = await _customerService.SearchAsync(SearchText);

                Customers.Clear();
                foreach (var customer in customers)
                {
                    Customers.Add(customer);
                }

                StatusMessage = $"Found {Customers.Count} customers";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error searching: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Creates a new customer
        /// </summary>
        [RecordCommand("New Customer")]
        public void NewCustomer()
        {
            SelectedCustomer = null;
            EditorViewModel.Clear();
            IsEditing = true;
            StatusMessage = "Creating new customer";
        }

        /// <summary>
        /// Saves the current customer
        /// </summary>
        [RecordApiCall("Save Customer")]
        public async Task SaveCustomerAsync()
        {
            try
            {
                IsLoading = true;

                var customer = EditorViewModel.ToModel();

                if (customer.Id == 0)
                {
                    StatusMessage = "Creating customer...";
                    var created = await _customerService.CreateAsync(customer);
                    Customers.Add(created);
                    StatusMessage = $"Created customer: {created.Name}";
                }
                else
                {
                    StatusMessage = "Updating customer...";
                    var updated = await _customerService.UpdateAsync(customer);

                    var index = -1;
                    for (int i = 0; i < Customers.Count; i++)
                    {
                        if (Customers[i].Id == updated.Id)
                        {
                            index = i;
                            break;
                        }
                    }

                    if (index >= 0)
                    {
                        Customers[index] = updated;
                    }

                    StatusMessage = $"Updated customer: {updated.Name}";
                }

                EditorViewModel.IsDirty = false;
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error saving: {ex.Message}";
                MessageBox.Show($"Error saving customer: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Deletes the selected customer
        /// </summary>
        [RecordApiCall("Delete Customer")]
        public async Task DeleteCustomerAsync()
        {
            if (SelectedCustomer == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete {SelectedCustomer.Name}?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                IsLoading = true;
                StatusMessage = $"Deleting {SelectedCustomer.Name}...";

                var success = await _customerService.DeleteAsync(SelectedCustomer.Id);

                if (success)
                {
                    Customers.Remove(SelectedCustomer);
                    EditorViewModel.Clear();
                    SelectedCustomer = null;
                    IsEditing = false;
                    StatusMessage = "Customer deleted";
                }
                else
                {
                    StatusMessage = "Failed to delete customer";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error deleting: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Cancels the current edit
        /// </summary>
        [RecordCommand("Cancel Edit")]
        public void CancelEdit()
        {
            if (SelectedCustomer != null)
            {
                EditorViewModel.LoadFromModel(SelectedCustomer);
            }
            else
            {
                EditorViewModel.Clear();
                IsEditing = false;
            }

            StatusMessage = "Edit cancelled";
        }
    }
}
