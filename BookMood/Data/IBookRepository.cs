using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookMood.Data
{
    public interface IBookRepository
    {
        Task<List<Book>> LoadAsync(CancellationToken ct = default);
        Task SaveAsync(List<Book> books, CancellationToken ct = default);
    }
}
