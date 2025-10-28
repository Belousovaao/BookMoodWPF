using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookMood.Services
{
    public class DefaultBookFactory : IBookFactory
    {
        public Book CreateNew()
        {
            return new Book
            {
                Id = Guid.NewGuid(),
                DateAdded = DateTime.Now,
                Title = "New Book"
            };
        }
    }
}
