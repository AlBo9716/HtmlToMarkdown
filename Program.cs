using System.Text;
using System.Text.RegularExpressions;

// Какие-либо изменения в строке я проделывал с помощью регулярных выражений.
// Pattern - строка для записи регулярного выражения, используется в качестве "посредника", 
// чтобы компилятор не выдавал сообщения о перегрузках.
partial class Programm
{
    // Главная функция. Здесь происходит чтение текстового файла, конвертация HTML-кода в текст, запись полученного
    // текста в текстовый файл
    static void Main()
    {
        string filename = "C:\\Users\\boao\\Desktop\\Examples\\Пример 1.txt";
        HtmlToMarkdown(filename);   
    }

    // Функция преобразования HTML-кода в Markdown
    public static void HtmlToMarkdown(string _path)
    {
        // Чтение текстового файла
        StreamReader sr = new(_path, true);
        string str = sr.ReadToEnd();
        sr.Close();
        // Удаление всех переносов строк, отступов и лишних пробелов
        string pattern = @"\s+";
        str = Regex.Replace(str, pattern," ").Trim();
        // Удаление  всех пробелов между тегами
        pattern = @">\s+<";
        str = Regex.Replace(str, pattern, "><");
        // Функция конвертации таблиц из HTML в Markdown
        str = TableHtmlToMarkdown(str);
        // Функция конвертации ссылок на изображение из HTML в Markdown 
        str = ImageHtmlToMarkdown(str);
        // Функция конвертации гиперсылок из HTML в Markdown
        str = LinkHtmlToMarkdown(str);
        // Функция конвертации кодировок символов HTML на соответствующие текстовые
        str = SymbolHtmlToMarkdown(str);
        // Функция конвертации разделителей HTML на соответствующие текстовые 
        str = SeparatorHtmlToMarkdown(str);
        // Удаление оставшихся HTML-тегов
        pattern = @"<.*?>";
        str = Regex.Replace(str, pattern, "");
        // Удаление лишних пустых строк
        pattern = @"(?<=\n)(\s*$\n)+|(?<=\n)(\n)+";
        str = Regex.Replace(str, pattern, "  \n"
                                , RegexOptions.Multiline).Trim();
        // Запись полученной строки в текстовый файл
        StreamWriter sw = new("C:\\Users\\boao\\Desktop\\Пример.txt", false);
        sw.Write(str);
        sw.Close();
    }

    // Функция преобразования таблиц из HTML в Markdown
    public static string TableHtmlToMarkdown(string _str)
    {
        // Если без исключений
        try
        {
            string pattern;
            //Проверка на наличие тега <table> - таблица
            if(_str.Contains("<table"))
            {
                // Если есть тег <thead> - заголовок таблицы
                if(_str.Contains("</th>"))
                { 
                    // Конвертация заголовка таблицы
                    pattern =  "<tbody.*?>.*?<tr.*?>(.*?)</tr>";
                    _str = Regex.Replace(_str, pattern
                                             , t =>
                                             {
                                                // Находим все теги <th>, заносим в строку по очереди и
                                                // и конвертируем в Markdown
                                                string head_cell = "";
                                                pattern = "<th.*?>(.*?)</th>";
                                                MatchCollection match = Regex.Matches(t.Groups[1].Value, pattern);
                                                for(int i = 0; i < match.Count; i++)
                                                { 
                                                    head_cell = head_cell + "|" + match[i].Groups[1].Value.Trim();
                                                }
                                                // Удаляем все теги разделителей внутри тегов ячеек таблицы
                                                pattern = "</p>|<br.*?>|<hr.*?>|</div>";
                                                head_cell = Regex.Replace(head_cell, pattern, "");
                                                // Выводим результат и создаем разделитель заголовка таблицы
                                                return head_cell.Trim()+ "|  \n"
                                                        + string.Concat(Enumerable.Repeat("|---", match.Count))
                                                        + "|  \n";
                                             }, RegexOptions.Multiline); 
                }
                // Если нет тега заголовка таблицы HTML
                else
                {
                    //Создаем пустой заголовок
                    pattern = "<table.*?>";
                    _str = Regex.Replace(_str, pattern
                                             , t =>
                                             {
                                                // Находим все строки в таблице, в каждой строке находим кол-во ячеек
                                                // и заносим результаты вычислений в массив
                                                string pattern = "<table.*?>(.*?)</table>";
                                                string lines = Regex.Match(_str, pattern).Groups[1].Value;
                                                pattern = "<tr.*?>(.*?)</tr>";
                                                MatchCollection match = Regex.Matches(lines, pattern);
                                                int[] count_cells_arr = new int[match.Count];
                                                for(int i = 0; i < match.Count; i++)
                                                {
                                                    pattern = "</td>";
                                                    count_cells_arr[i] = Regex.Matches(match[i].Groups[1].Value, pattern).Count;
                                                }
                                                // Находим максимальное количество столбцов
                                                int max_num_cells = count_cells_arr.Max();
                                                // Создание заголовка и разделителя в Markdown
                                                return string.Concat(Enumerable.Repeat("|   ", max_num_cells))
                                                        + "|  \n"
                                                        + string.Concat(Enumerable.Repeat("|---", max_num_cells))
                                                        + "|  \n";
                                             }, RegexOptions.Multiline);
                }
                // Преобразуем остальные ячейки таблицы в Markdown
                pattern = "<td.*?>(.*?)</td>";
                _str = Regex.Replace(_str, pattern, 
                                            t =>
                                            {
                                                // Считываем текст внутри ячейки
                                                string cell = t.Groups[1].Value.Trim();
                                                // // Удаляем теги разделителей внутри тегов ячеек таблицы
                                                string pattern = "<p.*?>|</p>|<br.*?>|<hr.*?>";
                                                cell = Regex.Replace(cell, pattern, "");
                                                // Преобразование в Markdown
                                                return "|" + cell;
                                            }, RegexOptions.Multiline);
                // Разделяем строки таблицы
                pattern = "</tr>";
                _str = Regex.Replace(_str, pattern, "|  \n".PadLeft(2), RegexOptions.Multiline);
            }
        }
        // Если есть исключения
        catch
        {
            Console.WriteLine("Ошибка в конвертации таблицы из HTML в Markdown");
        }
        // Вывод результата
        return _str;
    }

    // Функция преобразования ссылок на изображение из HTML в Markdown
    public static string ImageHtmlToMarkdown(string _str)
    {
        // Если без исключений
        try
        {
            // Проверка на наличие тега <img> - ссылка на изображение
            if(_str.Contains("<img"))
            {   
                // Ищем "внутренности" <img>
                string pattern =  @"<img(.*?)>";
                _str = Regex.Replace(_str, pattern, i =>
                                                            {
                                                                string img = i.Groups[1].Value.Trim();
                                                                // Заголовок изображения
                                                                string pattern =@"title=""(.*?)""";
                                                                string title = Regex.Match(img, pattern).Groups[1].Value;
                                                                // Краткое описание содержимого изображения
                                                                pattern = @"alt=""(.*?)""";
                                                                string alt = Regex.Match(img, pattern).Groups[1].Value;
                                                                // Ссылка на изображение
                                                                pattern = @"src=""(.*?)""";
                                                                string src = Regex.Match(img, pattern).Groups[1].Value;
                                                                // Преобразование в Markdown
                                                                return "![" + alt + "](" + src + " \""
                                                                + title + "\")";
                                                            }, RegexOptions.Multiline);
            }
        }
        // Если есть исключения
        catch
        {
            Console.WriteLine("Ошибка в конвертации ссылки на изображение из HTML в MarkDown");
        }
        // Вывод результата
        return _str;
    }

    // Функция преобразования гиперсылок из HTML в Markdown
    public static string LinkHtmlToMarkdown(string _str)
    {
        // Если есть исключения
        try
        {
            // Проверка на наличие тега <a> - гиперсылка
            if(_str.Contains("</a>"))
            {
                // Ищем текст и адрес гиперсылки
                string pattern = @"<a.*?href=""(.*?)"".*?>(.*?)</a>";
                _str = Regex.Replace(_str, pattern, l =>
                                                            {
                                                                // Текст гиперсылки
                                                                string text = l.Groups[2].Value.Trim();
                                                                // Адрес гиперсылки
                                                                string href = l.Groups[1].Value;
                                                                // Преобразование в Markdown
                                                                return @"["+text+"]("+href+")";
                                                            });
            }
        }
        // Если есть исключения
        catch
        {
            Console.WriteLine("Ошибка в конвертации гиперсылок из HTML в Markdown");
        }
        // Результат
        return _str;
    } 

    // Функция преобразования кодировок символов HTML на соответствующие текстовые
    public static string SymbolHtmlToMarkdown(string _str)
    {
        // Если без исключений
        try
        {
            // Создаем два массива: первый включает в себя кодировки символов в HTML,
            // второй - символы в текстовом формате
            string[] OldSymbol = {"&nbsp;", "&amp;", "&quot;", "&lt;","&gt;", "&reg;", 
                                    "&copy;", "&bull;", "&trade;","&#39;"};
            string[] NewSymbol = { " ", "&", "\"", "<", ">", "Â®", "Â©", "â€¢", "â„¢","\'"};
            // Производим замену кодировок на символы
            for (int i = 0; i < OldSymbol.Length; i++)
            {
                _str = Regex.Replace(_str, OldSymbol[i], NewSymbol[i]);
            }
        }
        // Если есть исключения
        catch
        {
            Console.WriteLine("Ошибка в конвертации кодировок символов HTML на соответствующие текстовые");
        }
        // Результат
        return _str;
    }

    // Функция преобразования разделителей HTML на соответствующие текстовые
    public static string SeparatorHtmlToMarkdown(string _str) 
    {
        // Если без исключений
        try
        {
            // Проверка на наличие тега <br> - перенос строки
            if(_str.Contains("<br"))
            {
                // Делаем одиночный перенос строки
                string pattern = "<br.*?>";
                _str = Regex.Replace(_str, pattern,  "  \n", RegexOptions.Multiline);    
            }
            // Проверка на наличие тега <p> - выделение абзаца
            if(_str.Contains("</p>"))
            {
                // Делаем двойной перенос строки
                string pattern = "</p>";
                _str = Regex.Replace(_str, pattern,  "  \n\n", RegexOptions.Multiline);
            }
            // Проверка на наличие тега <hr> - параграф
            if(_str.Contains("</hr>"))
            {
                // Делаем разделитель параграфа для Markdown
                string pattern = "<hr.*?>";
                _str = Regex.Replace(_str, pattern,  "  \n---  \n", RegexOptions.Multiline);
            }
            // Проверка на наличие тега <div> - блоки
            if(_str.Contains("</div>"))
            {
                // Делаем двойной перенос строки
                string pattern = "</div>";
                _str = Regex.Replace(_str, pattern, "  \n\n", RegexOptions.Multiline);
            }
        }
        // Если есть исключения
        catch
        {
            Console.WriteLine("Ошибка в конвертации разделителей HTML на соответствующие текстовые");
        }
        // Результат
        return _str;
    }
}