using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            return 1;
        }

        string inputPath = args[0];
        string outputPath = args[1];

        try
        {
            string[] words;
            HashSet<char> letters = new HashSet<char>();

            // --- Чтение ---
            using (var reader = new StreamReader(inputPath, Encoding.UTF8))
            {
                string firstLine = reader.ReadLine();
                if (firstLine == null)
                {
                    Console.Error.WriteLine("Ошибка: пустой входной файл.");
                    return 1;
                }

                int n = int.Parse(firstLine.Trim());
                words = new string[n];

                int idx = 0;
                while (idx < n)
                {
                    string line = reader.ReadLine();
                    if (line == null)
                    {
                        Console.Error.WriteLine(
                            $"Ошибка: ожидалось {n} слов, прочитано {idx}.");
                        return 1;
                    }
                    line = line.Trim();
                    if (line.Length == 0) continue; // пропускаем пустые строки

                    words[idx] = line;
                    foreach (char c in line) letters.Add(c);
                    idx++;
                }
            }

            // Построение графа зависимостей
            var graph = new Dictionary<char, HashSet<char>>();
            var indegree = new Dictionary<char, int>();

            foreach (char c in letters)
            {
                graph[c] = new HashSet<char>();
                indegree[c] = 0;
            }

            for (int i = 0; i + 1 < words.Length; i++)
            {
                string a = words[i];
                string b = words[i + 1];

                int len = Math.Min(a.Length, b.Length);
                int j = 0;
                while (j < len && a[j] == b[j]) j++;

                if (j == len) continue; // одно слово — префикс другого

                char from = a[j];
                char to = b[j];

                if (graph[from].Add(to))
                    indegree[to]++;
            }

            // Топологическая сортировка
            var zero = new SortedSet<char>();
            //зануляем всех у кого ноль
            foreach (var kv in indegree)
                if (kv.Value == 0) zero.Add(kv.Key);

            var result = new StringBuilder();
            //перебираем всех кто в 0
            while (zero.Count > 0)
            {
                //берем первого
                char cur = zero.Min;
                zero.Remove(cur);
                result.Append(cur);

                //у всех кто после него, делаем меньше
                foreach (char next in graph[cur])
                {
                    if (--indegree[next] == 0)
                        //кто 0, добавляем в zero
                        zero.Add(next);
                }
            }

            if (result.Length != letters.Count)
            {
                Console.Error.WriteLine(
                    "Ошибка: в глоссарии противоречие (цикл в графе зависимостей).");
                return 1;
            }

            // --- Запись ---
            using (var writer = new StreamWriter(outputPath, false, Encoding.UTF8))
            {
                writer.WriteLine(result.ToString());
            }

            Console.WriteLine($"Готово: {outputPath} → {result}");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Ошибка: {ex.Message}");
            return 1;
        }
    }
}