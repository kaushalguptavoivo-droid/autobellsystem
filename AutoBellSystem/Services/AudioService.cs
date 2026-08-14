using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace AutoBellSystem.Services
{
    public class AudioService
    {
        private IWavePlayer? _outputDevice;
        private IDisposable? _activeSource; // AudioFileReader (disposable) or null for tone
        private volatile bool _stopRequested;

        /// <summary>
        /// Plays the bell. If soundFile is missing/empty, a built-in synthesized
        /// tone is used instead so it always works offline. If preferredDeviceId
        /// is set and that device is currently connected, audio is routed directly
        /// to it (e.g. a paired Bluetooth speaker) without changing Windows'
        /// system default device.
        /// </summary>
        public void PlaySound(string soundFile, int rings, int volume, string? preferredDeviceId = null)
        {
            _stopRequested = false;

            Task.Run(() =>
            {
                for (int i = 0; i < Math.Max(1, rings); i++)
                {
                    if (_stopRequested) break;

                    try
                    {
                        PlayOnce(soundFile, volume, preferredDeviceId);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error playing sound: {ex.Message}");
                    }

                    if (i < rings - 1 && !_stopRequested)
                        Thread.Sleep(900); // pause between repeated rings
                }
            });
        }

        private void PlayOnce(string soundFile, int volume, string? preferredDeviceId)
        {
            float vol = Math.Clamp(volume, 0, 100) / 100f;

            ISampleProvider provider;
            AudioFileReader? fileReader = null;

            if (!string.IsNullOrWhiteSpace(soundFile) && File.Exists(soundFile))
            {
                fileReader = new AudioFileReader(soundFile) { Volume = vol };
                provider = fileReader;
            }
            else
            {
                provider = new ToneBellProvider(vol);
            }

            IWavePlayer output = CreateOutput(preferredDeviceId);
            _outputDevice = output;
            _activeSource = fileReader;

            try
            {
                output.Init(provider);
                output.Play();

                while (output.PlaybackState == PlaybackState.Playing && !_stopRequested)
                {
                    Thread.Sleep(100);
                }
            }
            finally
            {
                output.Stop();
                output.Dispose();
                fileReader?.Dispose();
                if (ReferenceEquals(_outputDevice, output)) _outputDevice = null;
                if (ReferenceEquals(_activeSource, fileReader)) _activeSource = null;
            }
        }

        private IWavePlayer CreateOutput(string? preferredDeviceId)
        {
            if (!string.IsNullOrWhiteSpace(preferredDeviceId))
            {
                try
                {
                    using var enumerator = new MMDeviceEnumerator();
                    var device = enumerator.GetDevice(preferredDeviceId);
                    if (device != null && device.State == DeviceState.Active)
                    {
                        // Route audio straight to this device (e.g. the paired
                        // Bluetooth speaker) regardless of the Windows default device.
                        return new WasapiOut(device, AudioClientShareMode.Shared, true, 100);
                    }
                }
                catch
                {
                    // Preferred device not available right now (e.g. speaker turned
                    // off/out of range) - fall back to the system default device below.
                }
            }

            return new WaveOutEvent();
        }

        public void Stop()
        {
            _stopRequested = true;
            _outputDevice?.Stop();
        }
    }
}
