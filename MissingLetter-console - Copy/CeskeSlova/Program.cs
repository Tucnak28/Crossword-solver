using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
public class DictionaryEntry
{
    public int Rank { get; set; }
    public string Word { get; set; }
    public int Frequency { get; set; }
}

class Program
{
static void Main(string[] args)
    {
        // Path to your TSV file
        string[] files = {
            "syn2015_word_abc_utf8.tsv",
            "syn2010_word_abc_utf8.tsv",
            "syn2005_word_abc_utf8.tsv",
            "syn2000_word_abc_utf8.tsv"};

        // Pattern to match
        string pattern = "hovnoho?noahojhospo??reni";

        // Path to the output text file 
        string outputFilePath = "matching_words.txt";

        // Clear the content of the output file at the beginning of every session
        File.WriteAllText(outputFilePath, string.Empty);

        // Load dictionary entries
        List<DictionaryEntry> dictionary = LoadDictionary(files);

        
        string finalWord = RecursiveFinish(dictionary, pattern, 0);

        Console.WriteLine("Final Word: " + finalWord);

        // Optionally, write the final word to a file
        File.WriteAllText(outputFilePath, finalWord);
    }

    static string RecursiveFinish(List<DictionaryEntry> dictionary, string pattern, int endWord)
    {
        // Base case: if the pattern contains no '?', return the pattern as is
        if (!pattern.Contains('?'))
        {
            return pattern;
        }

        // Iterate through each dictionary entry to find a matching word for the current pattern
        foreach (var entry in dictionary)
        {
            string word = entry.Word;

            // Remove diacritics for comparison
            string patternWithout = pattern.Substring(endWord, pattern.Length - endWord);

            if (word.Length < 4) continue; // Skipping words less than 4 characters long

            if (IsMatch(word, patternWithout))
            {
                // Update the pattern with the matched word
                string prefix = pattern.Substring(0, endWord);
                string suffix = pattern.Substring(endWord + word.Length);
                string updatedPattern = prefix + word + suffix;


                endWord += word.Length;

                // Recursively call the method with the updated pattern
                string result = RecursiveFinish(dictionary, updatedPattern, endWord);

                //Console.WriteLine(word);    
                // If a match is found in the recursive call, return the result
                if (!string.IsNullOrEmpty(result))
                    return RemoveDiacritics(result);
            }
        }

        // If no match is found, return an empty string
        return "";
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
            if (pattern[i] != '?' && pattern[i] != word[i])
            {
                return false;
            }
        }
        //Console.WriteLine($"word: {word} pattern: {pattern}");
        return true;
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
                        // Parse fields and create a DictionaryEntry object
                        DictionaryEntry entry = new DictionaryEntry();
                        entry.Rank = int.Parse(fields[0]);
                        entry.Word = RemoveDiacritics(fields[1]);
                        entry.Frequency = int.Parse(fields[2]);

                        dictionary.Add(entry);
                    }
                }
            }
        }
        dictionary = dictionary.OrderBy(entry => entry.Rank).ToList();
        return dictionary;
    }

}
