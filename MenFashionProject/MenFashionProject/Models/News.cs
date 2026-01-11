using System;
using System.ComponentModel.DataAnnotations;

namespace MenFashionProject.Models
{
    public class News
    {
        [Key]
        public int NewsId { get; set; }
        public string Title { get; set; } = null!;
        public string? Summary { get; set; }
        public string? Content { get; set; }
        public string? Image { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}