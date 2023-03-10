using System.Text.RegularExpressions;
class Program
{
    static async Task Main()
    {
        var fileExist = false;
        string path = string.Empty;
        while(!fileExist)
        {
            Console.Write("Please enter file path : ");
            path = Console.ReadLine();
            if (File.Exists(path))
                fileExist = true;
            else
                Console.WriteLine("File doesn't exist");
        }

        var wordStats = new List<WordsStats>();
        var startTime = DateTime.Now;
        int counter = 0;
        using (StreamReader sr = new StreamReader(path))
        {
            string text = File.ReadAllText(path);
            Regex reg_exp = new Regex("[^a-zA-Z0-9 ’-]");
            text = reg_exp.Replace(text, " ");
            string[] words = text.Split(new char[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var result = words.OrderBy(word => word.ToString()).Distinct().ToArray();
            string delim = " ,.";
            string[] fields = null;
            string line = string.Empty;

            ParallelOptions parallelOptions = new()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount * 2
            };

            Console.WriteLine("Processing....");
            while (!sr.EndOfStream)
            {
                line = sr.ReadLine().Trim();
                fields = line.Split(delim.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                counter += fields.Length;
                await Parallel.ForEachAsync(result, parallelOptions, async (item, cancellationToken) =>
                {
                    wordStats.Add(CountStringOccurrences(text, item));
                });
            }
            sr.Close();
        }

        var maxResult = wordStats.GroupBy(x => x.Word,
           (word, xs) => new
           {
               Word = word,
               MaxValue = xs.Max(x => x.Count)
           }).OrderByDescending(x => x.MaxValue).FirstOrDefault();

        var sevenCharacters = wordStats.Where(x => x.Word.Length == 7).ToList();

        var sevenResults = sevenCharacters.GroupBy(x => x.Word,
           (word, xs) => new
           {
               Word = word,
               MaxValue = xs.Max(x => x.Count)
           }).OrderByDescending(x => x.MaxValue).FirstOrDefault();


        Console.WriteLine($"Most frequent word: {maxResult.Word} occurred {maxResult.MaxValue} times");
        Console.WriteLine($"Most frequent 7-character word: {sevenResults.Word} occurred {sevenResults.MaxValue} times");
        Console.WriteLine($"The total word count is {counter}");
        Console.WriteLine($"Took {DateTime.Now - startTime} to process");
        Console.WriteLine($"Finished.....");
        Console.ReadLine();
    }
    
    public static WordsStats CountStringOccurrences(string text, string word)
    {
        int count = 0;
        int i = 0;
        while ((i = text.IndexOf(word, i)) != -1)
        {
            i += word.Length;
            count++;
        }

        return new WordsStats { Word = word, Count = count };
    }
}

public class WordsStats
{
    public string Word { get; set; }
    public long Count { get; set; }
}

