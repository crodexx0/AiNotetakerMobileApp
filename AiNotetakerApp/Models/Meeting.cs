using SQLite;
using System;

namespace AiNotetakerApp.Models
{
    public class Meeting
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        public string Title {  get; set; }
        public DateTime StartTime {  get; set; }
        public DateTime EndTime { get; set; }

        // Where the actual audio file is saved on the device
        public string AudioFilePath { get; set; }

        public string Transcript {  get; set; }
        public string AiSummary {  get; set; }
        public string ActionItems { get; set;  }

        // Foreign key to link this meeting to a specific folder
        public int FolderId {  get; set; }

        // Storing the ID from the native device calendar
        public string CalendarEventId { get; set;  }
    }
}