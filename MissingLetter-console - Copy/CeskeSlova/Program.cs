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

    public List<DictionaryEntry> Children { get; set; } // List of possible next words

    public DictionaryEntry()
    {
        Children = new List<DictionaryEntry>();
    }
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
        string pattern = "ho?noho?no";

        // Path to the output text file 
        string outputFilePath = "matching_words.txt";

        // Clear the content of the output file at the beginning of every session
        File.WriteAllText(outputFilePath, string.Empty);

        // Load dictionary entries
        List<DictionaryEntry> dictionary = LoadDictionary(files);

        // Find all matching words for the pattern
        List<string> matchedWords = FindMatchingWords(dictionary, pattern);

        // Sort matched words by score (frequency)
        //matchedWords = matchedWords.OrderByDescending(w => w.score).ToList();

        // Sort matched words by the number of separated words
        matchedWords = matchedWords.OrderBy(w => CountWords(w, separator)).ToList();

        // Write matched words to the output file
        using (StreamWriter writer = new StreamWriter(outputFilePath))
        {
            foreach (var word in matchedWords)
            {
                //writer.WriteLine($"{word} {score}");
                writer.WriteLine($"{word}");
            }
        }

        Console.WriteLine("Matching words found and written to matching_words.txt.");

        // Create the root entry
        DictionaryEntry rootEntry = new DictionaryEntry
        {
            Rank = 1,
            Word = "Root",
            Frequency = 100,
            Children = new List<DictionaryEntry>()
        };

        // Create child entries
        DictionaryEntry child1 = new DictionaryEntry
        {
            Rank = 2,
            Word = "Child1",
            Frequency = 50,
            Children = new List<DictionaryEntry>()
        };

        DictionaryEntry child2 = new DictionaryEntry
        {
            Rank = 3,
            Word = "Child2",
            Frequency = 75,
            Children = new List<DictionaryEntry>()
        };

        // Create grandchild entries
        DictionaryEntry grandchild1 = new DictionaryEntry
        {
            Rank = 4,
            Word = "Grandchild1",
            Frequency = 25,
            Children = new List<DictionaryEntry>()
        };

        DictionaryEntry grandchild2 = new DictionaryEntry
        {
            Rank = 5,
            Word = "Grandchild2",
            Frequency = 30,
            Children = new List<DictionaryEntry>()
        };

        // Add grandchild entries to child1
        child1.Children.Add(grandchild1);
        child1.Children.Add(grandchild2);

        // Add child entries to the root entry
        rootEntry.Children.Add(child1);
        rootEntry.Children.Add(child2);

        //PrintWordTree(rootEntry);
    }

    static List<string> FindMatchingWords(List<DictionaryEntry> dictionary, string pattern)
    {
        List<string> matchedWords = new List<string>();
        List<DictionaryEntry> wordTree = new List<DictionaryEntry>();
        RecursiveMatching(dictionary, wordTree, pattern, 0, matchedWords, 0);

        foreach (var entry in wordTree)
        {
            PrintWordTree(entry); // Print the word tree
        }

        return matchedWords;
    }

    static DictionaryEntry RecursiveMatching(List<DictionaryEntry> dictionary, List<DictionaryEntry> wordTree, string pattern, int endWord, List<string> matchedWords, int nested, DictionaryEntry previousWord = null)
    {
        // Base case: if the pattern contains no '?', add the current word to the list of matched words
        if (!pattern.Contains(secretChar))
        {
            matchedWords.Add((pattern));
            return previousWord;
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





                DictionaryEntry result = RecursiveMatching(dictionary, wordTree, updatedPattern, endWord + word.Length + 1, matchedWords, nested + 1, entry);

                //Console.WriteLine($"word: {entry.Word}           level: {nested}");

                if (result == null) continue;

                previousWord.Children.Add(entry);
                if (nested == 1)
                {
                    wordTree.Add(previousWord);
                }

            }
        }

        return null;
    }

    static void PrintWordTree(DictionaryEntry root)
    {
        if (root == null)
            return;

        Stack<(DictionaryEntry entry, string indent, int level)> stack = new Stack<(DictionaryEntry, string, int)>();
        stack.Push((root, "", 0));

        while (stack.Count > 0)
        {
            var (currentEntry, currentIndent, level) = stack.Pop();

            Console.WriteLine($"{currentIndent}{currentEntry.Word} (Level: {level})");

            foreach (var childEntry in currentEntry.Children)
            {
                stack.Push((childEntry, currentIndent + "  ", level + 1));
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
        /*if (word.Length < 3)
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
