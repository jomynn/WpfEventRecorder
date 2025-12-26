using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using WpfEventRecorder.Core.Attributes;
using WpfEventRecorder.SampleApp.Models;
using WpfEventRecorder.SampleApp.Services;

namespace WpfEventRecorder.SampleApp.ViewModels;

/// <summary>
/// ViewModel for the order list view.
/// </summary>
[RecordViewModel]
public class OrderListViewModel : ViewModelBase
{
    private readonly IApiService _apiService;
    private ObservableCollection<Order> _orders = new();
    private Order? _selectedOrder;
    private OrderStatus? _statusFilter;
    private DateTime? _fromDate;
    private DateTime? _toDate;

    public ObservableCollection<Order> Orders
    {
        get => _orders;
        set => SetProperty(ref _orders, value);
    }

    [RecordProperty]
    public Order? SelectedOrder
    {
        get => _selectedOrder;
        set => SetProperty(ref _selectedOrder, value);
    }

    public OrderStatus? StatusFilter
    {
        get => _statusFilter;
        set => SetProperty(ref _statusFilter, value);
    }

    public DateTime? FromDate
    {
        get => _fromDate;
        set => SetProperty(ref _fromDate, value);
    }

    public DateTime? ToDate
    {
        get => _toDate;
        set => SetProperty(ref _toDate, value);
    }

    public ObservableCollection<OrderStatus> Statuses { get; } = new(Enum.GetValues<OrderStatus>());

    public ICommand LoadCommand { get; }
    public ICommand FilterCommand { get; }
    public ICommand ClearFilterCommand { get; }
    public ICommand ViewOrderCommand { get; }
    public ICommand CancelOrderCommand { get; }

    public OrderListViewModel()
    {
        _apiService = ((App)Application.Current).ApiService;

        LoadCommand = new AsyncRelayCommand(LoadOrdersAsync);
        FilterCommand = new AsyncRelayCommand(ApplyFilterAsync);
        ClearFilterCommand = new RelayCommand(ClearFilter);
        ViewOrderCommand = new RelayCommand(ViewOrder, () => SelectedOrder != null);
        CancelOrderCommand = new AsyncRelayCommand(CancelOrderAsync,
            () => SelectedOrder != null && SelectedOrder.Status == OrderStatus.Pending);

        _ = LoadOrdersAsync();
    }

    public async Task LoadOrdersAsync()
    {
        await RunBusyAsync(async () =>
        {
            var orders = await _apiService.GetOrdersAsync();
            Orders = new ObservableCollection<Order>(orders);
        }, "Loading orders...");
    }

    private async Task ApplyFilterAsync()
    {
        await RunBusyAsync(async () =>
        {
            var orders = await _apiService.GetOrdersAsync();

            var filtered = orders.AsEnumerable();

            if (StatusFilter.HasValue)
            {
                filtered = filtered.Where(o => o.Status == StatusFilter.Value);
            }

            if (FromDate.HasValue)
            {
                filtered = filtered.Where(o => o.OrderDate >= FromDate.Value);
            }

            if (ToDate.HasValue)
            {
                filtered = filtered.Where(o => o.OrderDate <= ToDate.Value);
            }

            Orders = new ObservableCollection<Order>(filtered);
        }, "Filtering...");
    }

    private void ClearFilter()
    {
        StatusFilter = null;
        FromDate = null;
        ToDate = null;
        _ = LoadOrdersAsync();
    }

    private void ViewOrder()
    {
        if (SelectedOrder == null) return;
        // Open order details dialog
    }

    private async Task CancelOrderAsync()
    {
        if (SelectedOrder == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to cancel order #{SelectedOrder.Id}?",
            "Confirm Cancel",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            await RunBusyAsync(async () =>
            {
                await _apiService.CancelOrderAsync(SelectedOrder.Id);
                SelectedOrder.Status = OrderStatus.Cancelled;
                OnPropertyChanged(nameof(SelectedOrder));
            }, "Cancelling order...");
        }
    }
}
