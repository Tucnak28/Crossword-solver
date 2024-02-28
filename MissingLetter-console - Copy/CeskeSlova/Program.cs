using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;

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
        string pattern = "ho?noho?no";

        // Path to the output text file
        string outputFilePath = "matching_words.txt";

        // Clear the content of the output file at the beginning of every session
        File.WriteAllText(outputFilePath, string.Empty);

        // Call the recursive method to find matching words and build the pattern
        string finalWord = RecursiveFinish(files, pattern, 0, 0);

        Console.WriteLine("Final Word: " + finalWord);

        // Optionally, write the final word to a file
        File.WriteAllText(outputFilePath, finalWord);
    }

    static string RecursiveFinish(string[] files, string pattern, int indexFirst, int endWord)
    {
        // Base case: if the pattern contains no '?', return the pattern as is
        if (!pattern.Contains('?'))
        {
            return pattern;
        }

        // Iterate through each file to find a matching word for the current pattern
        foreach (var filePath in files)
        {
            using (StreamReader reader = new StreamReader(filePath))
            {
                // Read and process each line in the file
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] fields = line.Split('\t');
                    string word = fields[1];

                    // Remove diacritics for comparison
                    string wordWithoutDiacritics = RemoveDiacritics(word);
                    string patternWithoutDiacritics = RemoveDiacritics(pattern.Substring(endWord, pattern.Length - endWord));

                    if (word.Length < 4) continue; // Skipping words less than 4 characters long

                    if (IsMatch(wordWithoutDiacritics, patternWithoutDiacritics))
                    {
                        // Update the pattern with the matched word
                        string prefix = pattern.Substring(0, endWord);
                        string suffix = pattern.Substring(endWord + word.Length);
                        string updatedPattern = prefix + word + suffix;

                        // Recursively call the method with the updated pattern and index
                        return RecursiveFinish(files, updatedPattern, indexFirst + 1, endWord + word.Length);
                    }
                }
            }
        }

        // If no match is found, return the original pattern
        return pattern;
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
}
