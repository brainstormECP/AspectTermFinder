using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using GrafosPLN.DesambiguationsClasses;

namespace SentimentAnalysis
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LeerXmlTask4 task4 = new LeerXmlTask4();
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var pp = task4.ReadSentencesFromFile(openFileDialog1.FileName);
                //escribir en el fichero keywords
                StreamWriter file = new StreamWriter("\\keyWords.txt");
                StreamReader reader = new StreamReader("\\pattern.txt");
                List<string> patternsList = new List<string>();
                string pS = reader.ReadLine();
                while (pS != "<EOF>" && pS != null)
                {
                    patternsList.Add(pS);
                    pS = reader.ReadLine();
                }
                reader.Close();
                progressBar1.Maximum = pp.Count;
                int atCount = 0;
                int correctCount = 0;
                XmlDocument doc = new XmlDocument();
                doc.Load(openFileDialog1.FileName);
                int neutralCount = 0;
                var nodeList = doc.GetElementsByTagName("sentence");
                var dictAp = new Dictionary<string, Dictionary<string, string>>();
                foreach (XmlNode sen in nodeList)
                {
                  XmlNodeList nodes = sen.ChildNodes;
                  dictAp.Add(sen.Attributes["id"].Value,new Dictionary<string, string>());
                  foreach (XmlNode node in nodes)
                  {
                      if (node.LocalName == "aspectTerms")
                      {
                          var aspectTerms = node.ChildNodes;
                          foreach (XmlNode aspectTerm in aspectTerms)
                          {
                              var tt = aspectTerm.Attributes;
                              var text = PhraseFactory.Paretisis(tt["term"].Value);
                              text = PhraseFactory.MasDeUnEspacio(text);
                              dictAp.Add(text + sen.Attributes["id"],tt["polarity"].Value);
                              if (tt["polarity"].Value == "neutral")
                              {
                                  neutralCount++;
                              }
                          }
                      }
                  }
                }
                foreach (var sentence in pp)
                {
                    progressBar1.Value++;
                    this.Refresh();
                    //file.Write(sentence + "\n");
                    bool hasAT = false;
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (var phrase in sentence.Phrase)
                    {
                        var apList = SentimentAnalizer.ExtractAspectTerm(phrase);
                        if (apList.Count > 0)
                        {
                            int atTempCount = 0;
                            foreach (var ap in apList)
                            {
                                stringBuilder.Append(ap + " (" + dictAp[ap.ToString()] + "), ");
                                atTempCount++;
                            }
                            if (atTempCount > 0)
                            {
                                atCount += atTempCount;
                                hasAT = true;
                            }
                            stringBuilder.Append(": ");
                            bool hasPattern = false;
                            foreach (var pattern in patternsList)
                            {
                                var listOfWord = new List<Word>();
                                if (SentimentAnalizer.PatternAnalysis(phrase, pattern, ref listOfWord))
                                {
                                    hasPattern = true;
                                    if (listOfWord.Count > 0)
                                    {
                                        foreach (var word in listOfWord)
                                        {
                                            stringBuilder.Append(word + " ");
                                        }
                                        stringBuilder.Append("\n");
                                    }
                                    correctCount += atTempCount;
                                    break;
                                }
                            }
                            if (!hasPattern)
                            {
                                stringBuilder.Append("\n"); 
                            }
                        }
                        
                    }
                    if (hasAT)
                    {
                        file.Write(sentence + "\n");
                        file.WriteLine(sentence.CategoriesToString());
                        file.Write(stringBuilder.ToString());
                    }
                }
                file.Write("Cantidad AT: " + atCount + " Con Polaridad: " + correctCount + " Neutrales: " + neutralCount);
                file.Close();

                ////Otra opcion para probar
                //StreamWriter file = new StreamWriter("\\prueba.txt");
                //var pp = task4.ReadSentencesFromFile(openFileDialog1.FileName);
                //StreamReader reader = new StreamReader("\\pattern.txt");
                //List<string> patternsList = new List<string>();
                //string pS = reader.ReadLine();
                //while (pS != "<EOF>" && pS != null)
                //{
                //    patternsList.Add(pS);
                //    pS = reader.ReadLine();
                //}
                //reader.Close();
                //foreach (var sentence in pp)
                //{
                //    progressBar1.Value++;
                //    this.Refresh();
                //    file.Write(sentence + "\n");
                //    foreach (var phrase in sentence.Phrase)
                //    {
                //        var apList = SentimentAnalizer.ExtractAspectTerm(phrase);
                //        if (apList.Count > 0)
                //        {
                //            foreach (var ap in apList)
                //            {
                //                file.Write(ap + ", ");
                //            }
                //            file.Write(": ");
                //            foreach (var pattern in patternsList)
                //            {
                //                var listOfWord = new List<Word>();
                //                SentimentAnalizer.PatternAnalysis(phrase, pattern, ref listOfWord);
                //                if (listOfWord.Count > 0)
                //                {
                //                    foreach (var word in listOfWord)
                //                    {
                //                        file.Write(word + " ");
                //                    }
                //                    file.Write("\n");
                //                }
                //            }
                //            file.Write("\n");
                //        }
                //    }
                //}
                //file.Close();
            }
        }
    }
}
