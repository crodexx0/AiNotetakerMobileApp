using SQLite;

namespace AiNotetakerApp.Models
{
    public class Folder
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }
    }
}