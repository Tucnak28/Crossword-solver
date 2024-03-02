using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

public class DictionaryEntry
{
    public int Rank { get; set; }
    public string Word { get; set; }
    public int Frequency { get; set; }
}

class Program
{
    public static char secretChar = '?';
    public static char separator = ' ';


    static void Main(string[] args)
    {
        
        // Path to your TSV file
        string[] files = { "sorted_words.tsv" };

        // Pattern to match
        //string pattern = "?l?klelvr?kl?k????u";
        string pattern = "vlkklelvr?klik????u";

        // Path to the output text file 
        string outputFilePath = "matching_words.txt";

        // Clear the content of the output file at the beginning of every session
        File.WriteAllText(outputFilePath, string.Empty);

        // Load dictionary entries
        List<DictionaryEntry> dictionary = LoadDictionary(files);

        // Find all matching words for the pattern
        List<(string word, int score)> matchedWords = FindMatchingWords(dictionary, pattern);

        // Sort matched words by score (frequency)
        //matchedWords = matchedWords.OrderByDescending(w => w.score).ToList();

        // Sort matched words by the number of separated words
        matchedWords = matchedWords.OrderBy(w => CountWords(w.word, separator)).ToList();

        // Write matched words to the output file
        using (StreamWriter writer = new StreamWriter(outputFilePath))
        {
            foreach ((string word, int score) in matchedWords)
            {
                //writer.WriteLine($"{word} {score}");
                writer.WriteLine($"{word}");
            }
        }

        Console.WriteLine("Matching words found and written to matching_words.txt.");
    }

    static List<(string word, int score)> FindMatchingWords(List<DictionaryEntry> dictionary, string pattern)
    {
        List<(string word, int score)> matchedWords = new List<(string, int)>();
        RecursiveMatching(dictionary, pattern, 0, matchedWords, 0);
        return matchedWords;
    }

    static void RecursiveMatching(List<DictionaryEntry> dictionary, string pattern, int endWord, List<(string, int)> matchedWords, int score)
    {
        // Base case: if the pattern contains no '?', add the current word to the list of matched words
        if (!pattern.Contains(secretChar))
        {
            Console.WriteLine(pattern);
            matchedWords.Add((pattern, score));
            return;
        }

        // Iterate through each dictionary entry to find matching words for the current pattern
        foreach (var entry in dictionary)
        {
            string word = entry.Word;

            string patternWithout = pattern.Substring(endWord, pattern.Length - endWord);


            if (IsMatch(word, patternWithout))
            {
                string prefix = pattern.Substring(0, endWord);
                string suffix = pattern.Substring(endWord + word.Length);

                // Add a space after the added word
                string updatedPattern = prefix + word + separator + suffix;

                // Apply rules
                if (!ApplyRules(updatedPattern, word))
                {
                    //Console.WriteLine(updatedPattern + "            " + word);
                    continue; // Skip this dictionary entry if it doesn't meet the rules
                }

                score = entry.Frequency;

                RecursiveMatching(dictionary, updatedPattern, endWord + word.Length + 1, matchedWords, score);
            }
        }
    }


    // Method to check if a word matches a pattern
    static bool IsMatch(string word, string pattern)
    {
        if (word.Length > pattern.Length)
        {
            return false;
        }

        pattern = pattern.Substring(0, word.Length);

        for (int i = 0; i < word.Length; i++)
        {
            if (pattern[i] != secretChar && pattern[i] != word[i])
            {
                return false;
            }
        }
        return true;
    }

    static List<DictionaryEntry> LoadDictionary(string[] files)
    {
        List<DictionaryEntry> dictionary = new List<DictionaryEntry>();

        foreach (var filePath in files)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] fields = line.Split('\t');
                    if (fields.Length >= 3)
                    {
                        DictionaryEntry entry = new DictionaryEntry();
                        entry.Rank = int.Parse(fields[0]);
                        entry.Word = RemoveDiacritics(fields[1]).ToLower();
                        entry.Frequency = int.Parse(fields[2]);

                        dictionary.Add(entry);
                    }
                }
            }
        }

        return dictionary;
    }

    static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        string normalizedString = text.Normalize(NormalizationForm.FormD);
        StringBuilder stringBuilder = new StringBuilder();

        foreach (char c in normalizedString)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                stringBuilder.Append(c);
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    // Method to count the number of words separated by a given separator
    static int CountWords(string word, char separator)
    {
        string[] words = word.Split(separator);
        return words.Length;
    }

    // Apply rules to filter the combinations
    static bool ApplyRules(string pattern, string word)
    {
        // Define your rules here
        // For example, you could check for minimum word length or other criteria
        // Return true if the entry meets the rules, false otherwise

        // Example rule: Check for minimum word length
        /*if (word.Length < 2)
        {
            return false; // Reject dictionary entries with words less than 2 characters long
        }*/

        // Example rule: Disallow adjacent single-letter words
        string[] words = pattern.Split(separator);
        if (words[words.Length - 1].Length == 1 && word.Length == 1)
        {
            //Console.WriteLine("hmmm");
            return false; // Reject patterns with adjacent single-letter words
        }

        if (words[words.Length - 1].Length == 2 && word.Length == 2)
        {
            //Console.WriteLine("hmmm");
            return false; // Reject patterns with adjacent single-letter words
        }

        return true; // Entry meets all rules
    }

}
