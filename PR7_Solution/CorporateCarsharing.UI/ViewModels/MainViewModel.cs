using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CorporateCarsharing.Client.Models;
using CorporateCarsharing.Client.Services;

namespace CorporateCarsharing.Client.ViewModels;
public sealed class MainViewModel : INotifyPropertyChanged
{
    private readonly CarsharingApiClient api = new();
    private string apiUrl = "https://localhost:57000/";
    private VehicleDto? selectedVehicle;
    private string statusMessage = "Запустите сервер CorporateCarsharing, затем нажмите «Загрузить автомобили».";
    private string startAt = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-ddTHH:00:00Z", CultureInfo.InvariantCulture);
    private string endAt = DateTime.UtcNow.AddDays(1).AddHours(2).ToString("yyyy-MM-ddTHH:00:00Z", CultureInfo.InvariantCulture);
    public ObservableCollection<VehicleDto> Vehicles { get; } = new();
    public string ApiUrl { get => apiUrl; set => Set(ref apiUrl, value); }
    public VehicleDto? SelectedVehicle { get => selectedVehicle; set => Set(ref selectedVehicle, value); }
    public string StartAt { get => startAt; set => Set(ref startAt, value); }
    public string EndAt { get => endAt; set => Set(ref endAt, value); }
    public string StatusMessage { get => statusMessage; set => Set(ref statusMessage, value); }
    public ICommand LoadCommand { get; }
    public ICommand BookCommand { get; }
    public MainViewModel()
    {
        LoadCommand = new AsyncCommand(LoadAsync);
        BookCommand = new AsyncCommand(BookAsync);
    }
    private async Task LoadAsync()
    {
        try
        {
            StatusMessage = "Загрузка...";
            var list = await api.GetVehiclesAsync(ApiUrl);
            Vehicles.Clear();
            foreach (var v in list) Vehicles.Add(v);
            StatusMessage = $"Загружено автомобилей: {Vehicles.Count}.";
        }
        catch (Exception ex) { StatusMessage = $"Ошибка загрузки: {ex.Message}"; }
    }
    private async Task BookAsync()
    {
        if (SelectedVehicle is null) { StatusMessage = "Сначала выберите автомобиль."; return; }
        if (!TryUtc(StartAt, out var start) || !TryUtc(EndAt, out var end) || end <= start)
        { StatusMessage = "Укажите корректный интервал в UTC: YYYY-MM-DDTHH:mm:ssZ, конец позже начала."; return; }
        try
        {
            StatusMessage = "Создание бронирования...";
            var booking = await api.CreateBookingAsync(ApiUrl, new(SelectedVehicle.Id, start, end));
            StatusMessage = $"Бронирование №{booking.Id} создано. Статус: {booking.Status}.";
        }
        catch (Exception ex) { StatusMessage = $"Ошибка бронирования: {ex.Message}"; }
    }
    private static bool TryUtc(string value, out DateTime result)
    {
        result = default;
        if (!value.Trim().EndsWith('Z')) return false;
        return DateTime.TryParseExact(value.Trim(), "yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out result);
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
