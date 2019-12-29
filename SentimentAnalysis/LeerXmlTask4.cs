using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using GrafosPLN.DesambiguationsClasses;
using GrafosPLN.FileReadingClasses;

namespace SentimentAnalysis
{
    class LeerXmlTask4: SentencesReader
    {
        public override List<Sentence> ReadSentencesFromFile(string filePath)
        {
            List<Sentence> lista = new List<Sentence>();
            XmlDocument doc = new XmlDocument();
            doc.Load(filePath);
            var nodeList = doc.GetElementsByTagName("sentence");

            foreach (XmlNode sen in nodeList)
            {
                lista.Add(SentenceFactory.SentenceBuilder(sen));
            }
            return lista;
        }
    }
}
