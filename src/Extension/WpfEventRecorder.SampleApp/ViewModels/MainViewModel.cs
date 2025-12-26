using System.Windows.Input;
using WpfEventRecorder.Core.Attributes;
using WpfEventRecorder.SampleApp.Services;

namespace WpfEventRecorder.SampleApp.ViewModels;

/// <summary>
/// Main window ViewModel.
/// </summary>
[RecordViewModel]
public class MainViewModel : ViewModelBase
{
    private readonly INavigationService _navigationService;
    private ViewModelBase? _currentViewModel;
    private string _currentView = "Customers";

    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public string CurrentView
    {
        get => _currentView;
        set => SetProperty(ref _currentView, value);
    }

    public ICommand NavigateToCustomersCommand { get; }
    public ICommand NavigateToOrdersCommand { get; }
    public ICommand RefreshCommand { get; }

    public MainViewModel()
    {
        _navigationService = new NavigationService();

        NavigateToCustomersCommand = new RelayCommand(() => NavigateTo("Customers"));
        NavigateToOrdersCommand = new RelayCommand(() => NavigateTo("Orders"));
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);

        // Start with customers view
        NavigateTo("Customers");
    }

    private void NavigateTo(string view)
    {
        CurrentView = view;

        CurrentViewModel = view switch
        {
            "Customers" => new CustomerListViewModel(),
            "Orders" => new OrderListViewModel(),
            _ => CurrentViewModel
        };
    }

    private async Task RefreshAsync()
    {
        if (CurrentViewModel is CustomerListViewModel customerVm)
        {
            await customerVm.LoadCustomersAsync();
        }
        else if (CurrentViewModel is OrderListViewModel orderVm)
        {
            await orderVm.LoadOrdersAsync();
        }
    }
}
