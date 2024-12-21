using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotCast.SharedKernel.Models
{
    public static class LibraryCode
    {
        public static string Generate()
        {
            return GeneratePronounceableString(8);
        }

        private static readonly Random Random = new();

        private static string GeneratePronounceableString(int length)
        {
            var sb = new StringBuilder();
            sb.Append(Random.Next(0, 100));
            sb.Append("-");

            // Lists of common consonant and vowel combinations
            string[] consonants =
            {
                "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n",
                "p", "r", "s", "t", "v", "w", "z", "ch", "sh", "th"
            };
            string[] vowels = { "a", "e", "i", "o", "u", "ea", "ee", "oo" };

            var isConsonant = Random.Next(0, 2) == 0;

            while (sb.Length < length)
            {
                if (isConsonant)
                {
                    var c = consonants[Random.Next(consonants.Length)];
                    sb.Append(c);
                }
                else
                {
                    var v = vowels[Random.Next(vowels.Length)];
                    sb.Append(v);
                }

                isConsonant = !isConsonant;
            }

            // Trim the string to the desired length
            return sb.ToString(0, length);
        }
    }
}
