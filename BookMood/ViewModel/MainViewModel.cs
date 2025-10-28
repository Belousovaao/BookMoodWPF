using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookMood.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace BookMood.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IBookRepository _bookRepository;
        private readonly IBookFactory _bookFactory;

        private Book _selectedBook;
        private bool _isExpanded;
        public Book SelectedBook
        {
            get => _selectedBook;
            set => SetProperty(ref _selectedBook, value);
        }
        
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                SetProperty(ref _isExpanded, value);
            }
                
        }

        public ObservableCollection<Book> Books { get; set; } = new ObservableCollection<Book>();
        public AsyncRelayCommand LoadCommand { get; }
        public AsyncRelayCommand SaveCommand { get; }
        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand ToggleExpandCommand { get; }

        public MainViewModel(IBookRepository bookRepository, IBookFactory bookFactory)
        {
            _bookRepository = bookRepository;
            _bookFactory = bookFactory;
            LoadCommand = new AsyncRelayCommand(LoadBooksAsync);
            SaveCommand = new AsyncRelayCommand(SaveBooksAsync);
            AddCommand = new RelayCommand(AddBook);
            DeleteCommand = new RelayCommand(DeleteBook);
            ToggleExpandCommand = new RelayCommand(Expand);

            _ = LoadBooksAsync();
        }

        private async Task LoadBooksAsync()
        {
            List<Book> books = await _bookRepository.LoadAsync();
            Books.Clear();
            foreach (Book b in books) { Books.Add(b); }
        }

        private async Task SaveBooksAsync()
        {
            await _bookRepository.SaveAsync(Books.ToList());
        }

        private void AddBook()
        {
            Book newBook = _bookFactory.CreateNew();
            Books.Add(newBook);
            SelectedBook = newBook;
        }

        private void DeleteBook()
        {
            if (SelectedBook != null) 
            { 
                int currentIndex = Books.IndexOf(SelectedBook);
                Books.Remove(SelectedBook);

                if (Books.Count == 0)
                {
                    SelectedBook = null;
                    return;
                }

                else if (currentIndex >= Books.Count)
                    SelectedBook = Books[currentIndex - 1];

                else
                {
                    SelectedBook = Books[currentIndex];
                }
            }
        }

        private void Expand()
        {
            IsExpanded = !IsExpanded;
        }

    }
}
