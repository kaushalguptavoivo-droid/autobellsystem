using Quartz;
using Quartz.Impl;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace AutoBellSystem.Services
{
    public class SchedulerService
    {
        private IScheduler _scheduler = null!;
        private readonly DatabaseService _dbService;
        private readonly AudioService _audioService;

        private static readonly Dictionary<string, string> DayMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "MON", "MON" }, { "MONDAY", "MON" },
            { "TUE", "TUE" }, { "TUESDAY", "TUE" },
            { "WED", "WED" }, { "WEDNESDAY", "WED" },
            { "THU", "THU" }, { "THURSDAY", "THU" },
            { "FRI", "FRI" }, { "FRIDAY", "FRI" },
            { "SAT", "SAT" }, { "SATURDAY", "SAT" },
            { "SUN", "SUN" }, { "SUNDAY", "SUN" },
        };

        public SchedulerService(DatabaseService dbService, AudioService audioService)
        {
            _dbService = dbService;
            _audioService = audioService;
        }

        public async Task InitializeAsync()
        {
            StdSchedulerFactory factory = new StdSchedulerFactory();
            _scheduler = await factory.GetScheduler();
            await _scheduler.Start();
        }

        public async Task ReloadSchedulesAsync()
        {
            if (_scheduler == null) return;
            await _scheduler.Clear();

            var settings = await _dbService.GetSettingsAsync();
            var activeProfile = settings?.ActiveProfile ?? "Normal";
            var schedules = await _dbService.GetSchedulesAsync(activeProfile);

            foreach (var schedule in schedules)
            {
                if (!schedule.IsActive) continue;

                var timeParts = schedule.Time.Split(':');
                if (timeParts.Length != 2) continue;
                if (!int.TryParse(timeParts[0], out int hour)) continue;
                if (!int.TryParse(timeParts[1], out int minute)) continue;

                string cronDay = MapDaysToCron(schedule.DayOfWeek);
                string cronExpression = $"0 {minute} {hour} ? * {cronDay}";

                IJobDetail job = JobBuilder.Create<BellJob>()
                    .WithIdentity($"job_{schedule.Id}", "Bells")
                    .UsingJobData("soundPath", schedule.SoundFilePath ?? "")
                    .UsingJobData("rings", schedule.Rings)
                    .UsingJobData("bellName", schedule.BellTypeName)
                    .UsingJobData("volume", settings?.VolumeLevel ?? 100)
                    .UsingJobData("deviceId", settings?.PreferredDeviceId ?? "")
                    .Build();

                try
                {
                    ITrigger trigger = TriggerBuilder.Create()
                        .WithIdentity($"trigger_{schedule.Id}", "Bells")
                        .WithCronSchedule(cronExpression)
                        .Build();

                    await _scheduler.ScheduleJob(job, trigger);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to schedule '{schedule.BellTypeName}' ({cronExpression}): {ex.Message}");
                }
            }

            if (settings?.SchedulePaused == true)
            {
                await _scheduler.PauseAll();
            }
        }

        /// <summary>
        /// Converts a stored day value ("ALL", "MON", or a comma list like
        /// "MON,WED,FRI") into a Quartz cron day-of-week field.
        /// </summary>
        private string MapDaysToCron(string dayStr)
        {
            if (string.IsNullOrWhiteSpace(dayStr) ||
                string.Equals(dayStr, "ALL", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(dayStr, "All", StringComparison.OrdinalIgnoreCase))
            {
                return "*";
            }

            var parts = dayStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var mapped = parts
                .Select(p => DayMap.TryGetValue(p, out var v) ? v : null)
                .Where(v => v != null)
                .Distinct()
                .ToList();

            return mapped.Count > 0 ? string.Join(",", mapped) : "*";
        }

        public async Task PauseAllAsync()
        {
            if (_scheduler != null) await _scheduler.PauseAll();
        }

        public async Task ResumeAllAsync()
        {
            if (_scheduler != null) await _scheduler.ResumeAll();
        }
    }

    public class BellJob : IJob
    {
        public async Task Execute(IJobExecutionContext context)
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            string soundPath = dataMap.GetString("soundPath") ?? "";
            int rings = dataMap.GetInt("rings");
            int volume = dataMap.GetInt("volume");
            string bellName = dataMap.GetString("bellName") ?? "Bell";
            string deviceId = dataMap.GetString("deviceId") ?? "";

            var audioService = AppServices.Audio;
            var dbService = AppServices.Database;

            try
            {
                audioService.PlaySound(soundPath, rings, volume, deviceId);
                await dbService.LogEventAsync(bellName, "Played");
            }
            catch (Exception ex)
            {
                Logger.LogError($"BellJob ({bellName})", ex);
                await dbService.LogEventAsync(bellName, "Failed", ex.Message);
            }
        }
    }
}
