using System;
using NAudio.Wave;

namespace AutoBellSystem.Services
{
    /// <summary>
    /// Generates a simple decaying two-tone "ding-dong" bell sound in memory.
    /// Used whenever the user hasn't picked a custom .wav/.mp3 file, so the
    /// app always has a working bell sound even with no internet connection
    /// and no bundled audio assets.
    /// </summary>
    public class ToneBellProvider : ISampleProvider
    {
        private readonly int _sampleRate = 44100;
        private readonly float _volume;
        private int _sampleIndex;
        private readonly int _totalSamples;

        // Two-note "ding-dong" like a classic school bell
        private const double Freq1 = 880.0;  // A5
        private const double Freq2 = 659.25; // E5

        public WaveFormat WaveFormat { get; }

        public ToneBellProvider(float volume, double durationSeconds = 1.1)
        {
            _volume = Math.Clamp(volume, 0f, 1f);
            WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(_sampleRate, 1);
            _totalSamples = (int)(durationSeconds * _sampleRate);
        }

        public int Read(float[] buffer, int offset, int count)
        {
            int samplesWritten = 0;

            for (int n = 0; n < count && _sampleIndex < _totalSamples; n++)
            {
                double t = _sampleIndex / (double)_sampleRate;

                // First half: higher note, second half: lower note (ding-dong)
                double freq = t < 0.45 ? Freq1 : Freq2;

                // Exponential decay envelope so each note fades out naturally
                double localT = t < 0.45 ? t : (t - 0.45);
                double envelope = Math.Exp(-localT * 4.5);

                double sample = Math.Sin(2 * Math.PI * freq * t) * envelope;
                buffer[offset + n] = (float)(sample * _volume);

                _sampleIndex++;
                samplesWritten++;
            }

            return samplesWritten;
        }

        public bool Finished => _sampleIndex >= _totalSamples;
    }
}
