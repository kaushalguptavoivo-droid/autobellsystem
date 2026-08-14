using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using AutoBellSystem.Models;
using AutoBellSystem.Services;
using System.Linq;

namespace AutoBellSystem.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly DatabaseService _db = AppServices.Database;
        private readonly BluetoothConnector _audioDevices = AppServices.Bluetooth;

        [ObservableProperty]
        private ObservableCollection<Schedule> _schedules = new();

        [ObservableProperty]
        private ObservableCollection<AudioDeviceInfo> _availableDevices = new();

        [ObservableProperty]
        private AudioDeviceInfo? _selectedDevice;

        [ObservableProperty]
        private int _volume = 100;

        [ObservableProperty]
        private bool _isPaused;

        [ObservableProperty]
        private string _statusText = "";

        private Settings _settings = new();

        public MainViewModel()
        {
            _ = InitializeAsync();
        }

        public async Task InitializeAsync()
        {
            try
            {
                await LoadSettingsAsync();
                await LoadDataAsync();
                RefreshDevices();
            }
            catch (Exception ex)
            {
                StatusText = $"Startup issue: {ex.Message}";
            }
        }

        public async Task LoadSettingsAsync()
        {
            _settings = await _db.GetSettingsAsync() ?? new Settings();
            Volume = _settings.VolumeLevel;
            IsPaused = _settings.SchedulePaused;
        }

        public async Task LoadDataAsync()
        {
            var data = await _db.GetSchedulesAsync(_settings.ActiveProfile);
            Schedules.Clear();
            foreach (var s in data.OrderBy(x => x.Time))
            {
                Schedules.Add(s);
            }
        }

        [RelayCommand]
        private void RefreshDevices()
        {
            try
            {
                var devices = _audioDevices.GetAvailablePlaybackDevices();
                AvailableDevices.Clear();
                foreach (var d in devices) AvailableDevices.Add(d);

                SelectedDevice = AvailableDevices.FirstOrDefault(d => d.Id == _settings.PreferredDeviceId)
                                  ?? AvailableDevices.FirstOrDefault(d => d.IsDefault);

                StatusText = $"{AvailableDevices.Count} playback device(s) found.";
            }
            catch (Exception ex)
            {
                StatusText = $"Could not list audio devices: {ex.Message}";
            }
        }

        [RelayCommand]
        private async Task SaveDeviceChoice()
        {
            _settings.PreferredDeviceId = SelectedDevice?.Id ?? "";
            _settings.PreferredDeviceName = SelectedDevice?.Name ?? "";
            await _db.SaveSettingsAsync(_settings);
            await AppServices.Scheduler.ReloadSchedulesAsync();
            StatusText = string.IsNullOrEmpty(_settings.PreferredDeviceId)
                ? "Using Windows default playback device."
                : $"Bells will play through: {_settings.PreferredDeviceName}";
        }

        [RelayCommand]
        private async Task SaveVolume()
        {
            _settings.VolumeLevel = Volume;
            await _db.SaveSettingsAsync(_settings);
        }

        [RelayCommand]
        private void TestBell()
        {
            AppServices.Audio.PlaySound("", 1, Volume, _settings.PreferredDeviceId);
        }

        [RelayCommand]
        private async Task TogglePause()
        {
            IsPaused = !IsPaused;
            _settings.SchedulePaused = IsPaused;
            await _db.SaveSettingsAsync(_settings);

            if (IsPaused) await AppServices.Scheduler.PauseAllAsync();
            else await AppServices.Scheduler.ResumeAllAsync();
        }

        public async Task SaveScheduleAsync(Schedule schedule)
        {
            await _db.SaveScheduleAsync(schedule);
            await AppServices.Scheduler.ReloadSchedulesAsync();
            await LoadDataAsync();
        }

        [RelayCommand]
        private async Task DeleteSchedule(Schedule s)
        {
            if (s == null) return;
            await _db.DeleteScheduleAsync(s);
            await AppServices.Scheduler.ReloadSchedulesAsync();
            await LoadDataAsync();
        }
    }
}
