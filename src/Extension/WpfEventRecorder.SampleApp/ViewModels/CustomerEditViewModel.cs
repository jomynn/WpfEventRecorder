using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WpfEventRecorder.Core.Attributes;
using WpfEventRecorder.SampleApp.Models;
using WpfEventRecorder.SampleApp.Services;

namespace WpfEventRecorder.SampleApp.ViewModels;

/// <summary>
/// ViewModel for the customer edit view.
/// </summary>
[RecordViewModel]
public class CustomerEditViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private Customer _customer;
    private ObservableCollection<Country> _countries = new();
    private Country? _selectedCountry;
    private bool _isNewCustomer;

    public Customer Customer
    {
        get => _customer;
        set => SetProperty(ref _customer, value);
    }

    [RecordProperty]
    public string FirstName
    {
        get => _customer.FirstName;
        set
        {
            _customer.FirstName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSave));
        }
    }

    [RecordProperty]
    public string LastName
    {
        get => _customer.LastName;
        set
        {
            _customer.LastName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSave));
        }
    }

    [RecordProperty]
    public string Email
    {
        get => _customer.Email;
        set
        {
            _customer.Email = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanSave));
        }
    }

    [RecordProperty]
    public string? Phone
    {
        get => _customer.Phone;
        set
        {
            _customer.Phone = value;
            OnPropertyChanged();
        }
    }

    [RecordProperty]
    public DateTime DateOfBirth
    {
        get => _customer.DateOfBirth;
        set
        {
            _customer.DateOfBirth = value;
            OnPropertyChanged();
        }
    }

    public bool IsActive
    {
        get => _customer.IsActive;
        set
        {
            _customer.IsActive = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<Country> Countries
    {
        get => _countries;
        set => SetProperty(ref _countries, value);
    }

    public Country? SelectedCountry
    {
        get => _selectedCountry;
        set
        {
            SetProperty(ref _selectedCountry, value);
            _customer.CountryId = value?.Id;
            _customer.Country = value;
        }
    }

    public bool IsNewCustomer
    {
        get => _isNewCustomer;
        set => SetProperty(ref _isNewCustomer, value);
    }

    public bool CanSave =>
        !string.IsNullOrWhiteSpace(FirstName) &&
        !string.IsNullOrWhiteSpace(LastName) &&
        !string.IsNullOrWhiteSpace(Email);

    public string Title => IsNewCustomer ? "New Customer" : "Edit Customer";

    public ICommand SaveCommand { get; }
    public ICommand CancelCommand { get; }

    public event EventHandler<bool>? RequestClose;

    public CustomerEditViewModel(Customer? customer = null)
    {
        _apiService = ((App)Application.Current).ApiService;

        _customer = customer ?? new Customer();
        _isNewCustomer = customer == null;

        SaveCommand = new AsyncRelayCommand(SaveAsync, () => CanSave);
        CancelCommand = new RelayCommand(Cancel);

        _ = LoadCountriesAsync();
    }

    private async Task LoadCountriesAsync()
    {
        await RunBusyAsync(async () =>
        {
            var countries = await _apiService.GetCountriesAsync();
            Countries = new ObservableCollection<Country>(countries);

            if (_customer.CountryId.HasValue)
            {
                SelectedCountry = Countries.FirstOrDefault(c => c.Id == _customer.CountryId.Value);
            }
        });
    }

    private async Task SaveAsync()
    {
        await RunBusyAsync(async () =>
        {
            if (IsNewCustomer)
            {
                await _apiService.CreateCustomerAsync(_customer);
            }
            else
            {
                await _apiService.UpdateCustomerAsync(_customer);
            }

            RequestClose?.Invoke(this, true);
        }, "Saving...");
    }

    private void Cancel()
    {
        RequestClose?.Invoke(this, false);
    }
}
