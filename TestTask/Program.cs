using System;
using System.Collections.Generic;
using System.Linq;

namespace TestTask
{
    public class Program
    {
        /// <summary>
        /// Программа принимает на входе 2 пути до файлов.
        /// Анализирует в первом файле кол-во вхождений каждой буквы (регистрозависимо). Например А, б, Б, Г и т.д.
        /// Анализирует во втором файле кол-во вхождений парных букв (не регистрозависимо). Например АА, Оо, еЕ, тт и т.д.
        /// По окончанию работы - выводит данную статистику на экран.
        /// </summary>
        /// <param name="args">Первый параметр - путь до первого файла.
        /// Второй параметр - путь до второго файла.</param>
        static void Main(string[] args)
        {
            try
            {
                IReadOnlyStream inputStream1 = GetInputStream(args[0]);
                IReadOnlyStream inputStream2 = GetInputStream(args[1]);

                IList<LetterStats> singleLetterStats = FillSingleLetterStats(inputStream1);
                IList<LetterStats> doubleLetterStats = FillDoubleLetterStats(inputStream2);

                RemoveCharStatsByType(singleLetterStats, CharType.Vowel);
                RemoveCharStatsByType(doubleLetterStats, CharType.Consonants);

                PrintStatistic(singleLetterStats);
                PrintStatistic(doubleLetterStats);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType());
            }


            // TODO : Необжодимо дождаться нажатия клавиши, прежде чем завершать выполнение программы.
            Console.WriteLine("Press any key to close.");
            Console.ReadKey();
        }

        /// <summary>
        /// Ф-ция возвращает экземпляр потока с уже загруженным файлом для последующего посимвольного чтения.
        /// </summary>
        /// <param name="fileFullPath">Полный путь до файла для чтения</param>
        /// <returns>Поток для последующего чтения.</returns>
        private static IReadOnlyStream GetInputStream(string fileFullPath)
        {
            return new ReadOnlyStream(fileFullPath);
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения каждой буквы.
        /// Статистика РЕГИСТРОЗАВИСИМАЯ!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillSingleLetterStats(IReadOnlyStream stream)
        {
            stream.ResetPositionToStart();

            IList<LetterStats> ret = new List<LetterStats>();

            while (!stream.IsEof)
            {
                char c = stream.ReadNextChar();
                if ((c > 255) || (c < 192))
                    continue;
                    byte[] b = new byte[] { (byte) c };
                string s = System.Text.Encoding.GetEncoding(1251).GetString(b);
                // TODO : заполнять статистику с использованием метода IncStatistic. Учёт букв - регистрозависимый.
                bool incFlag = false;
                for (int i=0; i < ret.Count; i++)
                {
                    if (ret[i].Letter != s)
                        continue;
                    else
                    {
                        LetterStats ls = ret[i];
                        IncStatistic(ref ls);
                        ret[i] = ls;
                        incFlag = true;
                        break;
                    }
                }
                if (!incFlag)
                    ret.Add(new LetterStats(s, 1));

            }
            return ret;            //return ???;

            //throw new NotImplementedException();
        }

        /// <summary>
        /// Ф-ция считывающая из входящего потока все буквы, и возвращающая коллекцию статистик вхождения парных букв.
        /// В статистику должны попадать только пары из одинаковых букв, например АА, СС, УУ, ЕЕ и т.д.
        /// Статистика - НЕ регистрозависимая!
        /// </summary>
        /// <param name="stream">Стрим для считывания символов для последующего анализа</param>
        /// <returns>Коллекция статистик по каждой букве, что была прочитана из стрима.</returns>
        private static IList<LetterStats> FillDoubleLetterStats(IReadOnlyStream stream)
        {
            stream.ResetPositionToStart();
            IList<LetterStats> ret = new List<LetterStats>();
            char c1 = stream.ReadNextChar();
            while (!stream.IsEof)
            {
                if ((c1 > 255) || (c1 < 192))
                {
                    c1 = stream.ReadNextChar();
                    continue;
                }
                char c2 = stream.ReadNextChar();
                // TODO : заполнять статистику с использованием метода IncStatistic. Учёт букв - НЕ регистрозависимый.
                if ((c1 == c2) || (Math.Abs(c1 - c2) == 32))
                {
                    char c = (c1 <= c2) ? c1 : c2;
                    c = (c > 223) ? (char)(c - 32) : c;
                    string letter = System.Text.Encoding.GetEncoding(1251).GetString(new byte[]{ (byte) c });
                    letter += letter;
                    bool incFlag = false;
                    for (int i=0; i < ret.Count; i++)
                    {
                        if (ret[i].Letter != letter)
                            continue;
                        else
                        {
                            LetterStats ls = ret[i];
                            IncStatistic(ref ls);
                            ret[i] = ls;
                            incFlag = true;
                            c1 = stream.ReadNextChar();
                            break;
                        }
                    }
                    if (!incFlag)
                    {
                        ret.Add(new LetterStats(letter,1));
                        c1 = stream.ReadNextChar();
                    }
                }
                else
                    c1 = c2;
            }

            //return ???;
            return ret;
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Ф-ция перебирает все найденные буквы/парные буквы, содержащие в себе только гласные или согласные буквы.
        /// (Тип букв для перебора определяется параметром charType)
        /// Все найденные буквы/пары соответствующие параметру поиска - удаляются из переданной коллекции статистик.
        /// </summary>
        /// <param name="letters">Коллекция со статистиками вхождения букв/пар</param>
        /// <param name="charType">Тип букв для анализа</param>
        private static void RemoveCharStatsByType(IList<LetterStats> letters, CharType charType)
        {
            // TODO : Удалить статистику по запрошенному типу букв.
            switch (charType)
            {
                case CharType.Consonants:
                    string []consonants = new string[] { "а", "о", "и", "е", "ё", "э", "ы", "у", "ю", "я", "А", "О", "И", "Е", "Ё", "Э", "Ы", "У", "Ю", "Я"};
                    foreach (string letter in consonants)
                        for (int i=0; i<letters.Count; i++)
                        {
                            if (letters[i].Letter.Contains(letter))
                                letters.RemoveAt(i);
                        }
                    break;
                case CharType.Vowel:
                    string[] vowel = new string[] { "б", "в", "г", "д", "ж", "з", "й", "к", "л", "м", "н", "п", "р", "с", "т", "ф", "х", "ц", "ч", "ш", "щ", "Б", "В", "Г", "Д", "Ж", "З", "Й", "К", "Л", "М", "Н", "П", "Р", "С", "Т", "Ф", "Х", "Ц", "Ч", "Ш", "Щ"};
                    foreach (string letter in vowel)
                        for (int i = 0; i < letters.Count; i++)
                        {
                            if (letters[i].Letter.Contains(letter))
                                letters.RemoveAt(i);
                        }
                    break;
            }
            
        }

        /// <summary>
        /// Ф-ция выводит на экран полученную статистику в формате "{Буква} : {Кол-во}"
        /// Каждая буква - с новой строки.
        /// Выводить на экран необходимо предварительно отсортировав набор по алфавиту.
        /// В конце отдельная строчка с ИТОГО, содержащая в себе общее кол-во найденных букв/пар
        /// </summary>
        /// <param name="letters">Коллекция со статистикой</param>
        private static void PrintStatistic(IEnumerable<LetterStats> letters)
        {
            // TODO : Выводить на экран статистику. Выводить предварительно отсортировав по алфавиту!
            letters = SortStats(letters);
            foreach (LetterStats stat in letters)
            {
                Console.WriteLine(stat.Letter + " - " + stat.Count);
            }
            Console.WriteLine();
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Метод увеличивает счётчик вхождений по переданной структуре.
        /// </summary>
        /// <param name="letterStats"></param>
        private static void IncStatistic(ref LetterStats letterStats)
        {
            letterStats.Count++;
        }

        /// <summary>
        /// Метод сортировки по алфавиту.
        /// </summary>
        /// <param name="letter"></param>
        private static IEnumerable<LetterStats> SortStats(IEnumerable<LetterStats> letters)
        {
            IList<LetterStats> ret = letters.ToList();
            bool flag = true;
            while (flag)
            {
                flag = false;
                for (int i = 0; i < ret.Count - 1; i++)
                    if (ret[i].Letter.CompareTo(ret[i + 1].Letter) > 0)
                    {
                        LetterStats buffer = ret[i];
                        ret[i] = ret[i + 1];
                        ret[i + 1] = buffer;
                        flag = true;
                    }
            }
            return ret;
        }

    }
}
