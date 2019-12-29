using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GrafosPLN.DesambiguationsClasses;

namespace SentimentAnalysis
{
    class SentenceFactory
    {
        internal static Sentence SentenceBuilder(System.Xml.XmlNode sen)
        {
            var newSentence = new Sentence();
            var phraseList = PhraseFactory.PhrasesBuilder(sen);
            //Adj JJ, Sust NN, Adv RB
            foreach (var ph in phraseList)
            {
                newSentence.AddPhrase(ph);
            }
            return newSentence;
        }
    }
}
