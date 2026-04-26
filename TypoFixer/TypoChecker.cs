namespace TypoFixer;

public class TypoChecker
{
    public void Run()
    {
        HashSet<string> correctWords = new HashSet<string>();
        List<string> allWords = new List<string>();

        if (!File.Exists("words_list.txt"))
        {
            Console.WriteLine("Помилка: Файл words_list.txt не знайдено!");
            return;
        }

        foreach (var line in File.ReadAllLines("words_list.txt"))
        {
            string word = line.ToLower().Trim();
            correctWords.Add(word);
            allWords.Add(word);
        }
        allWords.Sort();
        
        Console.Clear();
        Console.WriteLine("=== Текстовий редактор ===");
        Console.WriteLine("Tab - автодоповнення, Enter - завершити ввід та перейти до перевірки\n");
        Console.Write("> ");

        string allText = ReadTextAutocomlete(allWords);
        
        string finalText = InteractiveProofread(allText, correctWords, allWords);

        Console.Clear();
        Console.WriteLine("=== Завершено! Ваш фінальний текст ===\n");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine(finalText);
        Console.ResetColor();
    }
    
    public string ReadTextAutocomlete(List<string> allWords)
    {
        string allText = "";
        string currentWord = "";

        while (true)
        {
            List<string> vars = ByPrefix(allWords, currentWord);
            string ghostText = "";

            if (currentWord.Length > 0 && vars.Count > 0 && vars[0].Length > currentWord.Length)
            {
                ghostText = vars[0].Substring(currentWord.Length);
            }

            int cursorLeft = Console.CursorLeft;
            int cursorTop = Console.CursorTop;

            if (ghostText.Length > 0)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray; 
                Console.Write(ghostText);
                Console.Write("          "); 
                Console.ResetColor();
            }
            else
            {
                Console.Write("                    ");
            }

            Console.SetCursorPosition(cursorLeft, cursorTop);

            ConsoleKeyInfo key = Console.ReadKey(true);

            Console.SetCursorPosition(cursorLeft, cursorTop);
            Console.Write("                    ");
            Console.SetCursorPosition(cursorLeft, cursorTop);

            if (key.Key == ConsoleKey.Enter)
            {
                Console.WriteLine();
                break;
            }

            if (key.Key == ConsoleKey.Backspace)
            {
                if (allText.Length > 0)
                {
                    allText = allText.Substring(0, allText.Length - 1);
                    Console.Write("\b \b");
                }
                if (currentWord.Length > 0)
                {
                    currentWord = currentWord.Substring(0, currentWord.Length - 1);
                }
                continue;
            }

            if (key.Key == ConsoleKey.Spacebar)
            {
                Console.Write(" ");
                allText += " ";
                currentWord = "";
                continue;
            }

            if (key.Key == ConsoleKey.Tab)
            {
                if (ghostText.Length > 0)
                {
                    Console.Write(ghostText); 
                    allText += ghostText;
                    currentWord = vars[0];
                }
                continue;
            }
            
            char c = key.KeyChar;
            if (!char.IsControl(c))
            {
                c = char.ToLower(c);
                Console.Write(c);
                allText += c;
                currentWord += c;
            }
        }

        return allText;
    }
    
    public List<string> ByPrefix(List<string> words, string prefix, int quantity = 5)
    {
        List<string> result = new List<string>();
        prefix = prefix.ToLower();
    
        int firstIndex = -1;
        int left = 0;
        int right = words.Count - 1;
    
        while (left <= right)
        {
            int mid = (left + right) / 2;
    
            if (words[mid].StartsWith(prefix))
            {
                firstIndex = mid;
                right = mid - 1;
            }
            else if (words[mid].CompareTo(prefix) < 0)
            {
                left = mid + 1;
            }
            else
            {
                right = mid - 1;
            }
        }
    
        if (firstIndex == -1)
            return result;
    
        for (int i = firstIndex; i < words.Count; i++)
        {
            if (words[i].StartsWith(prefix))
            {
                result.Add(words[i]);
    
                if (result.Count == quantity)
                    break;
            }
            else
            {
                break;
            }
        }
    
        return result;
        
    }
    
    public string InteractiveProofread(string inputText, HashSet<string> correctWords, List<string> allWords)
    {
        string[] words = inputText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        char[] punctuation = { ',', '.', '!', '?', ';', ':', '"' };
        bool hasMistakes = false;

        for (int i = 0; i < words.Length; i++)
        {
            string cleanWord = words[i].ToLower().Trim(punctuation);

            if (!string.IsNullOrWhiteSpace(cleanWord) && !correctWords.Contains(cleanWord))
            {
                hasMistakes = true;
                
                var suggestions = Algorithms.TopSuggestions(cleanWord, allWords);
                if (suggestions.Count == 0) continue;

                int selectedIndex = 0;
                bool wordResolved = false;

                while (!wordResolved)
                {
                    Console.Clear();
                    Console.WriteLine("=== Перевірка орфографії ===");
                    Console.WriteLine("Стрілки - вибір, Enter - замінити, Esc - ігнорувати\n");

                    for (int w = 0; w < words.Length; w++)
                    {
                        if (w == i)
                        {
                            Console.BackgroundColor = ConsoleColor.Red;
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        Console.Write(words[w]);
                        Console.ResetColor();
                        Console.Write(" ");
                    }
                    Console.WriteLine("\n\nМожливо, ви мали на увазі:\n");

                    for (int j = 0; j < suggestions.Count; j++)
                    {
                        if (j == selectedIndex)
                        {
                            Console.BackgroundColor = ConsoleColor.White;
                            Console.ForegroundColor = ConsoleColor.Black;
                            Console.WriteLine($" > {suggestions[j]} ");
                            Console.ResetColor();
                            
                        }
                        else
                        {
                            Console.WriteLine($"   {suggestions[j]} ");
                        }
                    }

                    var key = Console.ReadKey(true).Key;
                    switch (key)
                    {
                        case ConsoleKey.UpArrow:
                            selectedIndex = (selectedIndex == 0) ? suggestions.Count - 1 : selectedIndex - 1;
                            break;
                        case ConsoleKey.DownArrow:
                            selectedIndex = (selectedIndex == suggestions.Count - 1) ? 0 : selectedIndex + 1;
                            break;
                        case ConsoleKey.Enter:
                            string replacement = suggestions[selectedIndex];
                            if (char.IsPunctuation(words[i].Last()))
                                replacement += words[i].Last();
                            if (char.IsPunctuation(words[i].First()))
                                replacement = words[i].First() + replacement;

                            // Зберігаємо велику літеру
                            if (char.IsUpper(words[i].Trim(punctuation).FirstOrDefault()))
                                replacement = char.ToUpper(replacement[0]) + replacement.Substring(1);

                            words[i] = replacement;
                            wordResolved = true;
                            break;
                        case ConsoleKey.Escape:
                            wordResolved = true;
                            break;
                    }
                }
            }
        }

        if (!hasMistakes)
        {
            Console.WriteLine("\nПомилок не знайдено!");
        }

        return string.Join(" ", words);
    }

}