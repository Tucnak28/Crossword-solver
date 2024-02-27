using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace SearchCrossword.Controllers
{
    public class HomeController : Controller
    {
        private readonly string[] files = {
            "syn2015_word_abc_utf8.tsv",
            "syn2010_word_abc_utf8.tsv",
            "syn2005_word_abc_utf8.tsv",
            "syn2000_word_abc_utf8.tsv"};

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult SearchWords(string pattern)
        {
            HashSet<string> uniqueWords = new HashSet<string>();
            pattern = pattern.ToLower();
            foreach (var filePath in files)
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] fields = line.Split('\t');
                        string word = fields[1].ToLower(); ;

                        if (IsMatch(RemoveDiacritics(word), RemoveDiacritics(pattern)))
                        {
                            uniqueWords.Add( word);
                        }
                    }
                }
            }
            List<string> sortedList = uniqueWords.OrderBy(word => word).ToList();
            return Json(sortedList);
        }

        private static bool IsMatch(string word, string pattern)
        {
            if (word.Length != pattern.Length)
            {
                return false;
            }

            for (int i = 0; i < word.Length; i++)
            {
                if (pattern[i] != '.' && pattern[i] != word[i])
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
    }
}
