using BookMood.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace BookMood
{
    public static class AppSetup
    {
        public static MainViewModel CreateMainViewModel()
        {
            IBookRepository bookRepository = new BookRepository();
            IBookFactory bookFactory = new DefaultBookFactory();
            return new MainViewModel(bookRepository, bookFactory);
        }
    }
}
