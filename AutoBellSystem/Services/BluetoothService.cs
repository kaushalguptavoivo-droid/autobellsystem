using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;

namespace AutoBellSystem.Services
{
    public class AudioDeviceInfo
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsDefault { get; set; }
    }

    /// <summary>
    /// Lists Windows playback (output) devices, including any paired and
    /// currently-connected Bluetooth speaker/headset (Windows exposes those
    /// as normal audio render endpoints once paired and turned on).
    /// This does NOT pair Bluetooth devices - pairing must be done once via
    /// Windows Settings > Bluetooth, same as any other Bluetooth speaker.
    /// </summary>
    public class BluetoothConnector
    {
        public List<AudioDeviceInfo> GetAvailablePlaybackDevices()
        {
            var result = new List<AudioDeviceInfo>();

            try
            {
                using var enumerator = new MMDeviceEnumerator();
                MMDevice? defaultDevice = null;
                try { defaultDevice = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia); }
                catch { /* no default device configured */ }

                foreach (var device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                {
                    result.Add(new AudioDeviceInfo
                    {
                        Id = device.ID,
                        Name = device.FriendlyName,
                        IsDefault = defaultDevice != null && device.ID == defaultDevice.ID
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error listing audio devices: {ex.Message}");
            }

            return result.OrderBy(d => d.Name).ToList();
        }

        /// <summary>Checks whether a previously chosen device is still connected/available.</summary>
        public bool IsDeviceAvailable(string deviceId)
        {
            if (string.IsNullOrWhiteSpace(deviceId)) return false;
            try
            {
                using var enumerator = new MMDeviceEnumerator();
                var device = enumerator.GetDevice(deviceId);
                return device != null && device.State == DeviceState.Active;
            }
            catch
            {
                return false;
            }
        }
    }
}
