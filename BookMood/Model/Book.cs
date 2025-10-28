using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookMood.Model
{
    public class Book
    {
        public Guid Id { get; init; }
        public string Title { get; set; } 
        public string Author { get; set; }
        public string Description { get; set; } 
        public string Notes { get; set; }
        public string Mood { get; set; }
        public DateTime DateAdded {  get; set; }

        public Book() { }
    }
}
