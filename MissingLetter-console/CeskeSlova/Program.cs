using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        int i = 0;

        // Path to your TSV file
        string[] files = {
            "syn2015_word_abc_utf8.tsv",
            "syn2010_word_abc_utf8.tsv",
            "syn2005_word_abc_utf8.tsv",
            "syn2000_word_abc_utf8.tsv"};

        // Pattern to match
        string pattern = "??o?i????e?";

        // HashSet to store unique words matching the pattern
        HashSet<string> uniqueWords = new HashSet<string>();

        // Open each file for reading
        foreach (var filePath in files)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                // Read and process each line in the file
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    // Split the line into fields using tab as delimiter
                    string[] fields = line.Split('\t');

                    // Access the word column
                    string word = fields[1]; // Second column - word

                    // Check if the word matches the pattern
                    if (IsMatch(RemoveDiacritics(word), RemoveDiacritics(pattern))){
                        uniqueWords.Add(word); // Add the word to HashSet
                    }

                    i++;
                }
            }
        }
        foreach (var word in uniqueWords)
        {
            Console.WriteLine(word);
        }

        Console.WriteLine("\nTotal number of words: " + i);
        Console.WriteLine("Number of unique words matching the pattern '" + pattern + "': " + uniqueWords.Count);
    }

    // Method to check if a word matches a pattern
    static bool IsMatch(string word, string pattern)
    {
        if (word.Length != pattern.Length)
        {
            return false;
        }

        for (int i = 0; i < word.Length; i++)
        {
            if (pattern[i] != '?' && pattern[i] != word[i])
            {
                return false;
            }
        }

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

    static string ShortenLongLetters(string text)
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < text.Length; i++)
        {
            char currentChar = text[i];
            if (i < text.Length - 1 && currentChar == text[i + 1])
            {
                sb.Append(currentChar);
                // Skip the next same character
                i++;
            }
            else
            {
                sb.Append(currentChar);
            }
        }
        return sb.ToString();
    }
}
