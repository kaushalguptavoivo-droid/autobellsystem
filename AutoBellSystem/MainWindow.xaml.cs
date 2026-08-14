using System.Windows;
using AutoBellSystem.Models;
using AutoBellSystem.Views;

namespace AutoBellSystem;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private async void AddSchedule_Click(object sender, RoutedEventArgs e)
    {
        var preferredDeviceId = ViewModelRoot.SelectedDevice?.Id ?? "";
        var dialog = new AddEditScheduleWindow(null, preferredDeviceId) { Owner = this };

        if (dialog.ShowDialog() == true)
        {
            await ViewModelRoot.SaveScheduleAsync(dialog.Result);
        }
    }

    private async void EditSchedule_Click(object sender, RoutedEventArgs e)
    {
        if (SchedulesGrid.SelectedItem is not Schedule selected)
        {
            MessageBox.Show("Select a bell period from the list first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var preferredDeviceId = ViewModelRoot.SelectedDevice?.Id ?? "";
        var dialog = new AddEditScheduleWindow(selected, preferredDeviceId) { Owner = this };

        if (dialog.ShowDialog() == true)
        {
            await ViewModelRoot.SaveScheduleAsync(dialog.Result);
        }
    }

    private void DeleteSchedule_Click(object sender, RoutedEventArgs e)
    {
        if (SchedulesGrid.SelectedItem is not Schedule selected)
        {
            MessageBox.Show("Select a bell period from the list first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var confirm = MessageBox.Show($"Delete '{selected.BellTypeName}' at {selected.Time}?", "Confirm Delete",
            MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirm == MessageBoxResult.Yes)
        {
            ViewModelRoot.DeleteScheduleCommand.Execute(selected);
        }
    }
}
