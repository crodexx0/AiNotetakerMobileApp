using SQLite;
using Microsoft.Maui.Controls;

namespace AiNotetakerApp.Models
{
    public class Folder
    {
        [Preserve(AllMembers = true)]

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Name { get; set; }
    }
}