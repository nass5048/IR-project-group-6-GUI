using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IR_Project_group6_C_
{
    public class InvertedIndexData
    {
        public string token;
        public List<string> locations = new List<string>();
        public string soundex;
        public int totalWords;
        public InvertedIndexData(string token, string locations)
        {
            totalWords = 1;
            this.token = token;
            this.locations.Add(locations);
            if(!string.IsNullOrEmpty(token))
                AddSoundex(token);
        }
        public void AddLocation(string location)
        {
            totalWords++;
            locations.Add(location);
        }
        public string AddSoundex(string token)
        {
            const int MaxSoundexCodeLength = 4;

            var soundexCode = new StringBuilder();
            var previousWasHOrW = false;

            token = Regex.Replace(
                token == null ? string.Empty : token.ToUpper(),
                    @"[^\w\s]",
                        string.Empty);

            if (string.IsNullOrEmpty(token))
                soundex = string.Empty.PadRight(MaxSoundexCodeLength, '0');

            soundexCode.Append(token.First());

            for (var i = 1; i < token.Length; i++)
            {   
                var numberCharForCurrentLetter =
                    GetCharNumberForLetter(token[i]);

                if (i == 1 &&
                        numberCharForCurrentLetter ==
                            GetCharNumberForLetter(soundexCode[0]))
                    continue;

                if (soundexCode.Length > 2 && previousWasHOrW &&
                        numberCharForCurrentLetter ==
                            soundexCode[soundexCode.Length - 2])
                    continue;

                if (soundexCode.Length > 0 &&
                        numberCharForCurrentLetter ==
                            soundexCode[soundexCode.Length - 1])
                    continue;

                soundexCode.Append(numberCharForCurrentLetter);

                previousWasHOrW = "HW".Contains(token[i]);
            }

            soundex = soundexCode
                    .Replace("0", string.Empty)
                        .ToString()
                            .PadRight(MaxSoundexCodeLength, '0')
                                .Substring(0, MaxSoundexCodeLength);
            return soundex;
        }
        private char GetCharNumberForLetter(char letter)
        {
            if ("BFPV".Contains(letter)) return '1';
            if ("CGJKQSXZ".Contains(letter)) return '2';
            if ("DT".Contains(letter)) return '3';
            if ('L' == letter) return '4';
            if ("MN".Contains(letter)) return '5';
            if ('R' == letter) return '6';

            return '0';
        }
    }
}
