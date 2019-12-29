using GrafosPLN.DesambiguationsClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shared.Enum;

namespace SentimentAnalysis
{
    class SentimentAnalizer
    {
        public static bool PatternAnalysis(Phrase phrase, string pattern, ref List<Word> keyWords)
        {

            var patternList = pattern.Split();
            int fixCount = 0;
            foreach (var s in patternList)
            {
                if (s[0] == '[')
                {
                    fixCount++;
                }
            }
            int validCount = phrase.Words.Count - (patternList.Length - fixCount);
            for (int i = 0; i <= validCount; i++)
            {
                int indexOfPattern = 0;
                int indexOfWord = i;
                int foundCount = 0;
                while (indexOfPattern < patternList.Length && indexOfWord < phrase.Words.Count)
                {
                    char startChar = patternList[indexOfPattern][0];
                    if (!PatternCompare(phrase.Words[indexOfWord], patternList[indexOfPattern], ref keyWords))
                    {
                        if (startChar == '[')
                        {
                            indexOfPattern++;
                        }
                        else
                        {
                            break; 
                        }
                    }
                    else
                    {
                        if (startChar != '[')
                        {
                            foundCount++;
                        }
                        indexOfPattern++;
                        indexOfWord++;
                    }
                }
                if (foundCount >= (patternList.Length - fixCount))
                {
                    return true;
                }
            }
            keyWords.Clear();
            return false;
        }

        public static bool PatternCompare(Word word, string patternWord, ref List<Word> wordsList)
        {
            if (patternWord[0] == '[' || patternWord[0] == '(')
            {
                patternWord = patternWord.Remove(0, 1);
                patternWord = patternWord.Remove(patternWord.Length - 1, 1);
                var list = patternWord.Split(',');
                foreach (var wrd in list)
                {
                    if (CompareWordInPattern(word, wrd, ref wordsList))
                    {
                        return true;
                    }
                }
                return false;
            }
            return CompareWordInPattern(word, patternWord, ref wordsList);
        }

        public static bool CompareWordInPattern(Word word, string patternWord, ref List<Word> wordsList)
        {
            if (patternWord.Length > 1)
            {
                if (patternWord[patternWord.Length - 2] == '/')
                {
                    var charType = patternWord[patternWord.Length - 1];
                    patternWord = patternWord.Remove(patternWord.Length - 1);
                    patternWord = patternWord.Remove(patternWord.Length - 1);
                    switch (charType)
                    {
                        case 'l':
                            return word.Lemma == patternWord;
                        case '!': if (word.Tag == patternWord)
                            {
                                wordsList.Add(word);
                                return true;
                            }
                            return false;
                        default:
                            return false;
                    }
                } 
            }
            return word.Tag == patternWord;
        }

        public static List<Word> ExtractAspectTerm(Phrase phrase)
        {
            return phrase.Words.Where(u => u.Tag == "AT").ToList();
        }
    }
}
