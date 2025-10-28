using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Transactions;

namespace BookMood.Data
{
    public class BookRepository : IBookRepository
    {
        private readonly string _dataDir; // путь к рабочей папке
        private readonly string _filePath; // путь к файлу json
        private readonly JsonSerializerOptions _jsonOptions; // json свойства сериализации
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // внутрипроцессная синхронизация (только 1 поток одновременно выполняет метод)

        public BookRepository(string baseFolder = null) // конструктор может принимать путь к рабочей папке, по умолчанию пустое значение
        {
            //если входящий атрибут пуст, тогда перемееной присваивается путь к системной папке для данных пользователя, куда программы сохраняют файлы, уникальные для каждого пользователя
            // в Windows это C:\Users\<User>\AppData\Roaming
            // masOS /Users/<User>/Library/Applicration/Support
            baseFolder ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "BookMood"); 
            
            _dataDir = baseFolder;
            _filePath = Path.Combine(_dataDir, "books.json"); //указывает на путь к файлу json в папке _dataDir

            Debug.WriteLine($"[BookRepository] dataDir: {_dataDir}");
            Debug.WriteLine($"[BookRepository] filePath: {_filePath}");

            _jsonOptions = new JsonSerializerOptions // доп. настройки сериализации
            {
                WriteIndented = true, //добавляет пробелы для красоты
                PropertyNameCaseInsensitive = true //убирает чувтсвительность к регистру
            };
        }

        private void EnsureDirectoryExists() //перед чтением и записью проверяем существование папки, если её нет, папка будет создана и приложение не упадет
        {
            if (!Directory.Exists(_dataDir))
                Directory.CreateDirectory(_dataDir);
        }

        // асинхронный метод, который возвращает список книг и может принимать токен отмены
        public async Task<List<Book>> LoadAsync(CancellationToken ct = default)
        {
            bool entered = false;
            try
            {
                // ждем свою очередь. Если передан токен отмены WaitAsync выбросит отмену операции
                await _semaphore.WaitAsync(ct).ConfigureAwait(false);
                entered = true;

                EnsureDirectoryExists();

                if (!File.Exists(_filePath)) //если файл json не существует, возвращаем пустой список книг
                    return new List<Book>();
               
                try
                {
                    // используя using открытый поток автоматически закроется после выхода из блока (или при исключении)
                    // разрешаем другим читать файл параллельно, но запись запрещена (блокируется через семафор)
                    using FileStream fs = File.Open(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                    List<Book> books = await JsonSerializer.DeserializeAsync<List<Book>>(fs, _jsonOptions, ct).ConfigureAwait(false);
                    return books ?? new List<Book>();
                }
                catch (JsonException) //если файл поврежден, делаем копию и возвращаем пустой список
                {
                    string backup = _filePath + "." + DateTime.Now.ToString("yyyyMMddHHmmss") + ".currupt.bak";
                    try 
                    { 
                        File.Copy(_filePath, backup, true);
                    }
                    catch // если в процессе копирования возникнет ошибка, она будет проигнорирована и программа продолжит работу
                    { }
                    return new List<Book>();
                }
            }
            finally // семафор высвобождается в любом случае
            {
                if (entered)
                    _semaphore.Release();
            }
        }

        public async Task SaveAsync(List<Book> books, CancellationToken ct = default)
        {
            bool entered = false;

            try
            {
                await _semaphore.WaitAsync(ct).ConfigureAwait(false);
                entered = true;

                EnsureDirectoryExists();

                string tempFile = _filePath + ".tmp"; 
                //создаем временный файл
                using (FileStream fs = File.Create(tempFile)) 
                {
                    await JsonSerializer.SerializeAsync(fs, books, _jsonOptions, ct).ConfigureAwait(false);
                    await fs.FlushAsync(ct).ConfigureAwait(false); // явно записывает буфер на диск
                }

                // резервная копия старого файла
                if (File.Exists(_filePath))
                {
                    string backup = _filePath + ".back";
                    try
                    {
                        File.Copy(_filePath, backup, true);
                    }
                    catch { }
                }

                File.Copy(tempFile, _filePath, true); //перезаписываем наш json
                File.Delete(tempFile);
            }
            finally
            {
                if (entered)
                    _semaphore.Release();
            }
        }
    }
}
