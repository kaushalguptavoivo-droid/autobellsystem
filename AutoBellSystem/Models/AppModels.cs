using SQLite;
using System;

namespace AutoBellSystem.Models
{
    // Preset bell patterns. "Rings" = how many times the tone plays back to back.
    public enum BellPattern
    {
        RegularPeriod = 0,  // single short ring
        Assembly = 1,       // long "tan-tan-tan" triple ring
        Lunch = 2,          // double ring
        Custom = 3          // user-defined ring count
    }

    public class Schedule
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string ProfileName { get; set; } = "Normal";

        // Comma separated day codes, e.g. "MON,WED,FRI" or "ALL"
        public string DayOfWeek { get; set; } = "ALL";

        public string Time { get; set; } = "00:00"; // HH:mm, 24-hour
        public string BellTypeName { get; set; } = "";
        public BellPattern Pattern { get; set; } = BellPattern.RegularPeriod;

        // Path to a .wav/.mp3 file. If empty or missing, a built-in
        // synthesized bell tone is used instead (works fully offline).
        public string SoundFilePath { get; set; } = "";

        public int Rings { get; set; } = 1;
        public bool IsActive { get; set; } = true;
    }

    public class Settings
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Windows CoreAudio device ID of the preferred playback device
        // (e.g. a paired Bluetooth speaker). Empty = use system default device.
        public string PreferredDeviceId { get; set; } = "";
        public string PreferredDeviceName { get; set; } = "";

        public bool AutoStartWithWindows { get; set; } = true;
        public int VolumeLevel { get; set; } = 100;
        public string ActiveProfile { get; set; } = "Normal";
        public bool SchedulePaused { get; set; } = false;
    }

    public class BellLog
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string BellName { get; set; } = "";
        public string Status { get; set; } = ""; // Played, Failed
        public string ErrorMessage { get; set; } = "";
    }
}
