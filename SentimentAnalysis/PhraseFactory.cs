using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrafosPLN.DesambiguationsClasses;
using PosTagging;
using System.Text.RegularExpressions;
using System.Xml;
using Shared.Token;

namespace SentimentAnalysis
{
    class PhraseFactory
    {
        internal static List<Phrase> PhrasesBuilder(System.Xml.XmlNode sen)
        {
            string text = sen.InnerText.ToLower();
            var postagger = new PosTagger();
            List<Word> words = new List<Word>();
            string pattern = "";
            XmlNodeList nodes = sen.ChildNodes;
            foreach (XmlNode node in nodes)
            {
                if (node.LocalName == "aspectTerms")
                {
                    var aspectTerms = node.ChildNodes;
                    foreach (XmlNode aspectTerm in aspectTerms)
                    {
                        var tt = aspectTerm.Attributes;
                        var tmp = tt["term"].Value.Trim('\"').Trim();
                        while (tmp.IndexOf('\"') >= 0)
                        {
                            int i = tmp.IndexOf('\"');
                            tmp = tmp.Remove(i, 1);
                        }
                        var term = MasDeUnEspacio(tmp);
                        term = Paretisis(tmp);
                        pattern += term + "|";
                        //pattern += aspectTerm.Attributes["term"].Value + "|";
                    }
                }
            }
            pattern = "(" + pattern.Trim('|') + ")";
            if (pattern != "()")
            {
                Regex regex = new Regex(pattern);
                text = regex.Replace(text, "/$1//");
            }
            text = MasDeUnEspacio(text);

            //var tokens = postagger.Tag("Oh , I need a hard disk.");//pasar la frase
            var tokens = postagger.Tag(text);//pasar la frase
            var tList = tokens.ToList();
            if (tList[tList.Count - 2].Word == ";")
            {
                tList.RemoveAt(tList.Count - 2);
            }
            bool ap = false; ;
            string apPart = String.Empty;
            var listTokenAT = new List<Token>();
            foreach (var token in tList)
            {
                if (token.Word == "/" && !ap)
                {
                    ap = true;
                    continue;
                }
                if (token.Word == "//")
                {
                    ap = false;
                }
                if (ap)
                {
                    apPart += " " + token.Word;
                    listTokenAT.Add(token);
                }
                if (!ap)
                {
                    if (token.Word == "//")
                    {
                        var AT = new AspectTerm(apPart.Trim());
                        words.Add(AT);
                        foreach (Token token1 in listTokenAT)
                        {
                            AT.AddToken(token1);
                        }
                        listTokenAT = new List<Token>();
                        apPart = String.Empty;
                    }
                    else
                    {
                        words.Add(new Word(token));
                    }
                }
            }
            return SplitInPhrases(words);
        }

        public static List<Phrase> SplitInPhrases(List<Word> words)
        {
            var phrases = new List<Phrase>();
            Word previusConector = null;
            Word nextConector = null;
            var wordList = new List<Word>();
            foreach (var word in words)
            {
                if (word.WordItself == "." || word.WordItself == ";" || word.Lemma.ToLower() == "and" ||
                    word.Tag == "Fc" || word.Lemma.ToLower() == "but" || word.Tag == "IN")
                {
                    int indexOfWord = words.IndexOf(word);
                    if ((word.Lemma == "and" || word.Tag == "Fc") && indexOfWord - 1 >= 0 
                        && (words[indexOfWord - 1].Tag == words[indexOfWord + 1].Tag 
                        || (words[indexOfWord - 1].Tag == "AT" && words[indexOfWord + 1].Tag == "DT" && words[indexOfWord + 2].Tag == "AT")
                        || (words[indexOfWord - 1].Tag == "JJ" && words[indexOfWord + 1].Tag == "RB" && words[indexOfWord + 2].Tag == "JJ"))
                        || indexOfWord - 1 >= 0 &&  (words[indexOfWord - 1].Tag == "RB" && word.Tag == "IN" && words[indexOfWord + 1].Tag == "JJ"))
                    {
                        wordList.Add(word);
                    }
                    else
                    {
                        nextConector = word;
                        Phrase p = new Phrase(wordList, previusConector, nextConector);
                        phrases.Add(p);
                        previusConector = nextConector;
                        wordList = new List<Word>();
                    }
                }
                else
                {
                    wordList.Add(word);
                }
            }
            return phrases;
        }

        public static List<Phrase> PhraseWithMeaning(List<Phrase> PhraseList)
        {
            for (int i = 1; i < PhraseList.Count; i++)
            {
                if (PhraseList[i].PreviousConnector.Tag == "CC" && PhraseList[i - 1].Words.Count != 0 && PhraseList[i].Words.Count != 0)
                {
                    if (PhraseList[i - 1].Words[PhraseList[i - 1].Words.Count - 1].Tag == "AT")
                    {
                        if (PhraseList[i].Words[0].Tag == "AT" || (PhraseList[i].Words[0].Tag == "DT" && PhraseList[i].Words[1].Tag == "AT"))
                        {
                            JoinTwoPhrases(PhraseList[i - 1], PhraseList[i]);
                            PhraseList.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
            return PhraseList;
        }

        public static Phrase JoinTwoPhrases(Phrase a, Phrase b)
        {
            a.Words.Add(b.PreviousConnector);
            foreach (var word in b.Words)
            {
                a.Words.Add(word);
            }
            a.NextConnector = b.NextConnector;
            return a;
        }

        public static string MasDeUnEspacio(string text)
        {
            Regex masDeUnEspacio = new Regex("\\s+");
            text = text.ToLower();
            text = masDeUnEspacio.Replace(text, " ");
            text = masDeUnEspacio.Replace(text, " ");
            return text;
        }
        public static string Paretisis(string text)
        {
            Regex parentesis = new Regex("\\(|\\)");
            text = parentesis.Replace(text, " ").ToLower();
            return text;
        }
    }
}

