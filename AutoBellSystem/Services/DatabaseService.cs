using SQLite;
using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoBellSystem.Models;

namespace AutoBellSystem.Services
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _db;

        public DatabaseService()
        {
            var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "AutoBellSystem");
            Directory.CreateDirectory(folder);
            var dbPath = Path.Combine(folder, "autobell.db");
            _db = new SQLiteAsyncConnection(dbPath);

            // Create tables synchronously during init
            var syncDb = new SQLiteConnection(dbPath);
            syncDb.CreateTable<Schedule>();
            syncDb.CreateTable<Settings>();
            syncDb.CreateTable<BellLog>();

            // Initialize default settings if missing
            if (syncDb.Table<Settings>().Count() == 0)
            {
                syncDb.Insert(new Settings());
            }
        }

        public async Task<List<Schedule>> GetSchedulesAsync(string profile)
        {
            return await _db.Table<Schedule>().Where(s => s.ProfileName == profile).ToListAsync();
        }

        public async Task<int> SaveScheduleAsync(Schedule item)
        {
            if (item.Id != 0) return await _db.UpdateAsync(item);
            return await _db.InsertAsync(item);
        }

        public async Task<int> DeleteScheduleAsync(Schedule item)
        {
            return await _db.DeleteAsync(item);
        }

        public async Task<Settings> GetSettingsAsync()
        {
            return await _db.Table<Settings>().FirstOrDefaultAsync();
        }

        public async Task<int> SaveSettingsAsync(Settings settings)
        {
            if (settings.Id == 0)
            {
                var existing = await GetSettingsAsync();
                if (existing != null) settings.Id = existing.Id;
            }
            return settings.Id != 0 ? await _db.UpdateAsync(settings) : await _db.InsertAsync(settings);
        }

        public async Task<int> LogEventAsync(string bellName, string status, string error = "")
        {
            return await _db.InsertAsync(new BellLog
            {
                Timestamp = DateTime.Now,
                BellName = bellName,
                Status = status,
                ErrorMessage = error
            });
        }
    }
}
