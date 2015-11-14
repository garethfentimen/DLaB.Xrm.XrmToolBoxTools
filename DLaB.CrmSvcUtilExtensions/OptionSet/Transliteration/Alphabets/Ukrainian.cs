using System.Collections.Generic;

namespace DLaB.CrmSvcUtilExtensions.OptionSet.Transliteration.Alphabets
{
    public static class Ukrainian
    {
        public static readonly Dictionary<char, string> Alphabet =
                new Dictionary<char, string>
                {
                    { 'а', "a" },
                    { 'б', "b" },
                    { 'в', "v" },
                    { 'г', "g" },
                    { 'д', "d" },
                    { 'е', "e" },
                    { 'ё', "yo" },
                    { 'ж', "zh" },
                    { 'з', "z" },
                    { 'и', "i" },
                    { 'й', "j" },
                    { 'к', "k" },
                    { 'л', "l" },
                    { 'м', "m" },
                    { 'н', "n" },
                    { 'о', "o" },
                    { 'п', "p" },
                    { 'р', "r" },
                    { 'с', "s" },
                    { 'т', "t" },
                    { 'у', "u" },
                    { 'ф', "f" },
                    { 'х', "h" },
                    { 'ц', "c" },
                    { 'ч', "ch" },
                    { 'ш', "sh" },
                    { 'щ', "sch" },
                    { 'ь', "" },
                    { 'ы', "y" },
                    { 'ъ', "" },
                    { 'э', "e" },
                    { 'ю', "yu" },
                    { 'я', "ya" },
                    { '/', "_" },
                    { '-', "_" },
                    { ' ', "_" },
                    { '→', "_" },
                    { ':', "_" },
                    { '"', "_" },
                    { ',', "" },
                    { '(', "" },
                    { ')', "" },
                    { '+', "i" },
                    { '.', "" },
                    { '%', "procentov" },
                    { '$', "S" },
                    { ';', "_" },
                    { 'і', "i" }
                };
    }
}
