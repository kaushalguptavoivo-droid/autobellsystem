namespace AutoBellSystem.Services
{
    // Simple static service locator for WPF App context and Quartz Jobs
    public static class AppServices
    {
        public static AudioService Audio { get; set; } = new AudioService();
        public static DatabaseService Database { get; set; } = new DatabaseService();
        public static BluetoothConnector Bluetooth { get; set; } = new BluetoothConnector();
        public static SchedulerService Scheduler { get; set; }

        static AppServices()
        {
            Scheduler = new SchedulerService(Database, Audio);
        }
    }
}
