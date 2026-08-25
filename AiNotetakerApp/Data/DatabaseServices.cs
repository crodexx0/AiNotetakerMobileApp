using SQLite;
using AiNotetakerApp.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AiNotetakerApp.Data
{
    public class DatabaseService
    {
        private SQLiteAsyncConnection _database;

        public DatabaseService()
        {
        }

        async Task Init()
        {
            if (_database is not null)
                return;

            _database = new SQLiteAsyncConnection(DatabaseConstants.DatabasePath, DatabaseConstants.Flags);

            // Create the tables if they don't exist
            await _database.CreateTableAsync<Folder>();
            await _database.CreateTableAsync<Meeting>();
        }

        // --- MEETING OPERATIONS ---

        public async Task<List<Meeting>> GetMeetingAsync()
        {
            await Init();
            return await _database.Table<Meeting>().OrderByDescending(m => m.StartTime).ToListAsync();
        }

        public async Task<int> SaveMeetingAsync(Meeting item)
        {
            await Init();
            if (item.Id != 0)
                return await _database.UpdateAsync(item);
            else
                return await _database.InsertAsync(item);
        }

        public async Task<int> DeleteMeetingAsync(Meeting item)
        {
            await Init();
            return await _database.DeleteAsync(item);
        }

        // --- FOLDER OPERATIONS ---

        public async Task<List<Folder>> GetFoldersAsync()
        {
            await Init();
            return await _database.Table <Folder>().ToListAsync();
        }

        public async Task<int> SaveFolderAsync(Folder item)
        {
            await Init();
            if (item.Id != 0)
                return await _database.UpdateAsync(item);
            else
                return await _database.InsertAsync(item);
        }
    }
}