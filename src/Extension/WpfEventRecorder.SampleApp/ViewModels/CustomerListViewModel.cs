using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WpfEventRecorder.Core.Attributes;
using WpfEventRecorder.SampleApp.Models;
using WpfEventRecorder.SampleApp.Services;

namespace WpfEventRecorder.SampleApp.ViewModels;

/// <summary>
/// ViewModel for the customer list view.
/// </summary>
[RecordViewModel]
public class CustomerListViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private ObservableCollection<Customer> _customers = new();
    private Customer? _selectedCustomer;
    private string _searchText = "";
    private bool _showInactiveCustomers;

    public ObservableCollection<Customer> Customers
    {
        get => _customers;
        set => SetProperty(ref _customers, value);
    }

    [RecordProperty]
    public Customer? SelectedCustomer
    {
        get => _selectedCustomer;
        set => SetProperty(ref _selectedCustomer, value);
    }

    [RecordProperty]
    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public bool ShowInactiveCustomers
    {
        get => _showInactiveCustomers;
        set => SetProperty(ref _showInactiveCustomers, value);
    }

    public ICommand LoadCommand { get; }
    public ICommand AddCustomerCommand { get; }
    public ICommand EditCustomerCommand { get; }
    public ICommand DeleteCustomerCommand { get; }
    public ICommand SearchCommand { get; }

    public CustomerListViewModel()
    {
        _apiService = ((App)Application.Current).ApiService;

        LoadCommand = new AsyncRelayCommand(LoadCustomersAsync);
        AddCustomerCommand = new RelayCommand(AddCustomer);
        EditCustomerCommand = new RelayCommand(EditCustomer, () => SelectedCustomer != null);
        DeleteCustomerCommand = new AsyncRelayCommand(DeleteCustomerAsync, () => SelectedCustomer != null);
        SearchCommand = new AsyncRelayCommand(SearchAsync);

        // Load initial data
        _ = LoadCustomersAsync();
    }

    public async Task LoadCustomersAsync()
    {
        await RunBusyAsync(async () =>
        {
            var customers = await _apiService.GetCustomersAsync();
            Customers = new ObservableCollection<Customer>(customers);
        }, "Loading customers...");
    }

    private void AddCustomer()
    {
        // Open customer edit dialog
        var newCustomer = new Customer();
        // In a real app, this would open a dialog
    }

    private void EditCustomer()
    {
        if (SelectedCustomer == null) return;
        // Open customer edit dialog
    }

    private async Task DeleteCustomerAsync()
    {
        if (SelectedCustomer == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete {SelectedCustomer.FullName}?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            await RunBusyAsync(async () =>
            {
                await _apiService.DeleteCustomerAsync(SelectedCustomer.Id);
                Customers.Remove(SelectedCustomer);
                SelectedCustomer = null;
            }, "Deleting customer...");
        }
    }

    private async Task SearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SearchText))
        {
            await LoadCustomersAsync();
            return;
        }

        await RunBusyAsync(async () =>
        {
            var customers = await _apiService.SearchCustomersAsync(SearchText);
            Customers = new ObservableCollection<Customer>(customers);
        }, "Searching...");
    }
}
