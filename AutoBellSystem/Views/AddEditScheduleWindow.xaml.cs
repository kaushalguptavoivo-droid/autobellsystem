using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using AutoBellSystem.Models;
using AutoBellSystem.Services;
using Microsoft.Win32;

namespace AutoBellSystem.Views
{
    public partial class AddEditScheduleWindow : Window
    {
        public Schedule Result { get; private set; }

        private readonly string _preferredDeviceId;

        public AddEditScheduleWindow(Schedule? existing, string preferredDeviceId)
        {
            InitializeComponent();
            _preferredDeviceId = preferredDeviceId;

            for (int h = 0; h < 24; h++) HourBox.Items.Add(h.ToString("D2"));
            for (int m = 0; m < 60; m++) MinuteBox.Items.Add(m.ToString("D2"));

            Result = existing != null ? Clone(existing) : new Schedule();

            if (existing != null)
            {
                NameBox.Text = existing.BellTypeName;
                var parts = existing.Time.Split(':');
                HourBox.SelectedItem = parts.Length > 0 ? parts[0] : "08";
                MinuteBox.SelectedItem = parts.Length > 1 ? parts[1] : "00";
                RingsBox.Text = existing.Rings.ToString();
                SoundPathBox.Text = existing.SoundFilePath ?? "";
                SetDays(existing.DayOfWeek);
                PatternBox.SelectedIndex = (int)existing.Pattern;
            }
            else
            {
                HourBox.SelectedItem = "08";
                MinuteBox.SelectedItem = "00";
                AllDaysBox.IsChecked = true;
                PatternBox.SelectedIndex = 0;
            }
        }

        private static Schedule Clone(Schedule s) => new Schedule
        {
            Id = s.Id,
            ProfileName = s.ProfileName,
            DayOfWeek = s.DayOfWeek,
            Time = s.Time,
            BellTypeName = s.BellTypeName,
            Pattern = s.Pattern,
            SoundFilePath = s.SoundFilePath,
            Rings = s.Rings,
            IsActive = s.IsActive
        };

        private void SetDays(string dayOfWeek)
        {
            if (string.IsNullOrWhiteSpace(dayOfWeek) || dayOfWeek.Equals("ALL", StringComparison.OrdinalIgnoreCase))
            {
                AllDaysBox.IsChecked = true;
                return;
            }

            AllDaysBox.IsChecked = false;
            var days = dayOfWeek.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                 .Select(d => d.ToUpperInvariant())
                                 .ToHashSet();

            MonBox.IsChecked = days.Contains("MON");
            TueBox.IsChecked = days.Contains("TUE");
            WedBox.IsChecked = days.Contains("WED");
            ThuBox.IsChecked = days.Contains("THU");
            FriBox.IsChecked = days.Contains("FRI");
            SatBox.IsChecked = days.Contains("SAT");
            SunBox.IsChecked = days.Contains("SUN");
        }

        private string GetDaysString()
        {
            if (AllDaysBox.IsChecked == true) return "ALL";

            var chosen = new System.Collections.Generic.List<string>();
            if (MonBox.IsChecked == true) chosen.Add("MON");
            if (TueBox.IsChecked == true) chosen.Add("TUE");
            if (WedBox.IsChecked == true) chosen.Add("WED");
            if (ThuBox.IsChecked == true) chosen.Add("THU");
            if (FriBox.IsChecked == true) chosen.Add("FRI");
            if (SatBox.IsChecked == true) chosen.Add("SAT");
            if (SunBox.IsChecked == true) chosen.Add("SUN");

            return chosen.Count > 0 ? string.Join(",", chosen) : "ALL";
        }

        private void AllDaysBox_Changed(object sender, RoutedEventArgs e)
        {
            bool allChecked = AllDaysBox.IsChecked == true;
            DaysPanel.IsEnabled = !allChecked;
        }

        private void PatternBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PatternBox.SelectedItem is not ComboBoxItem item) return;
            switch (item.Tag as string)
            {
                case "RegularPeriod": RingsBox.Text = "1"; break;
                case "Assembly": RingsBox.Text = "3"; break;
                case "Lunch": RingsBox.Text = "2"; break;
                // Custom: leave whatever the user has typed
            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Audio Files (*.wav;*.mp3)|*.wav;*.mp3|All Files (*.*)|*.*",
                Title = "Choose Bell Sound"
            };
            if (dlg.ShowDialog() == true)
            {
                SoundPathBox.Text = dlg.FileName;
            }
        }

        private void ClearSound_Click(object sender, RoutedEventArgs e)
        {
            SoundPathBox.Text = "";
        }

        private void Preview_Click(object sender, RoutedEventArgs e)
        {
            int rings = int.TryParse(RingsBox.Text, out var r) ? r : 1;
            AppServices.Audio.PlaySound(SoundPathBox.Text, rings, 100, _preferredDeviceId);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show("Please enter a period name.", "Missing Name", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (HourBox.SelectedItem == null || MinuteBox.SelectedItem == null)
            {
                MessageBox.Show("Please choose a time.", "Missing Time", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(RingsBox.Text, out int rings) || rings < 1 || rings > 20)
            {
                MessageBox.Show("Rings must be a number between 1 and 20.", "Invalid Rings", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Result.BellTypeName = NameBox.Text.Trim();
            Result.Time = $"{HourBox.SelectedItem}:{MinuteBox.SelectedItem}";
            Result.DayOfWeek = GetDaysString();
            Result.Rings = rings;
            Result.SoundFilePath = SoundPathBox.Text ?? "";
            Result.Pattern = (BellPattern)PatternBox.SelectedIndex;
            Result.IsActive = true;

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
