using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

namespace FilmPreview.Services
{
    public class ConvertSlug
    {
        public string ConvertString2Slug(string input)
        {
            string str = input.ToLower();
            str = RemoveDiacritics(str);

            str = Regex.Replace(str, @"[^\w\s]", "");
            str = Regex.Replace(str, @"\s+", "-");

            return str;
        }
        static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }
        public string ConvertSlug2String(string input)
        {
            string str = input.Replace("-", " ");

            str = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str);

            return str;
        }

    }
}
