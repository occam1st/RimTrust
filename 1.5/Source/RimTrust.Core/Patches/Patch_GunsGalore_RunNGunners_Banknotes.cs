using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Verse;
using Unity.Mathematics;
using System.Xml.Linq;
using RimWorld;

namespace RimTrust.Core.Patches
{
    public class Patch_GunsGalore_RunNGunners_Banknotes
    {

        public static void Patch_GunsGalore_RunNGunners_Accept_Banknotes()
        {
            //Log.Message("RimTrust: updateXML with System.Xml start for Guns Galore Run'N'Gunners accept Banknotes");
            string path = System.IO.Path.Combine(GenFilePaths.ModsFolderPath);
            path = path.Replace("Mods", "");
            path = path.Replace("RimWorld", "");
            path = path.Replace("common", "");
            path = path.Remove(path.Length - 1);
            path = path.Remove(path.Length - 1);
            path = System.IO.Path.Combine(path, "workshop", "content", "294100", "2984651947", "Defs", "TraderKindDefs", "TraderKinds_RunNGunners.xml");
            //Log.Message(path);

            if (System.IO.File.Exists(path))
            {
                bool BankNoteAccepted = false;
                //Log.Message("RimTrust: opening GG RnG XML file");
                //Log.Message("RimTrust: opening GG RnG XML path is " + path);
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(path);
                //Log.Message("RimTrust: setting root to stockGenerators");
                //Log.Message("RimTrust: start of foreach loop parsing nodeList2 for stockGenerators");
                //int i = 0;
                //int j = 0;
                XmlNodeList nodeList2 = xmldoc.GetElementsByTagName("TraderKindDef");
                foreach (XmlNode node2 in nodeList2)
                {
                    //if (i == 0) Log.Message("RimTrust: found " + nodeList2.Count + " nodes with TraderKindDef");

                    //Log.Message("RimTrust: in foreach loop nodeList2 checking for TraderKindDef with iteration " + i);


                    //Log.Message("RimTrust: setting root to stockGenerators");
                    XmlNodeList nodeList = xmldoc.SelectNodes("/Defs/TraderKindDef/stockGenerators");
                    //Log.Message("RimTrust: start of foreach loop parsing nodeList for Banknotes");
                    foreach (XmlNode node in nodeList)
                    {
                        //if (j == 0) Log.Message("RimTrust: found " + nodeList.Count + " nodes with stockGenerators");
                        //Log.Message("RimTrust: in foreach loop nodeList checking for stockGenerators with iteration " + j);
                        //Log.Message("RimTrust: BankNoteAccepted equals " + BankNoteAccepted);
                        XmlNode targetNode = node;

                        if (BankNoteAccepted == false)
                        {

                            //Log.Message("RimTrust: checking for BankNote InnerText of targetNode.Childnode[0]");
                            if (targetNode.LastChild.InnerText == "BankNote")
                            {
                                BankNoteAccepted = true;
                            }
                            else
                            { 
                                //targetNode = xmldoc.SelectSingleNode("/Defs/TraderKindDef/stockGenerators");
                                //Log.Message("RimTrust: before trying to add element with attribute for mayrequire occam1st.RimTrust");
                                XmlElement id = xmldoc.CreateElement("li");
                                id.SetAttribute("MayRequire", "occam1st.RimTrust");
                                id.SetAttribute("Class", "StockGenerator_BuySingleDef");
                                //Log.Message("RimTrust: before adding innertext BankNote");
                                XmlElement thingDef = xmldoc.CreateElement("thingDef");
                                thingDef.InnerText = "BankNote";
                                //Log.Message("RimTrust: before inserting id element to targetnode");
                                XmlNode targetNode2 = targetNode.InsertAfter(id, targetNode.LastChild);
                                //Log.Message("RimTrust: before inserting thingDef element to targetnode");
                                targetNode2.InsertAfter(thingDef, targetNode2.LastChild);
                                BankNoteAccepted = false;
                                xmldoc.Save(path);
                            }   
                        }
                        //j++;
                    }
                    //i++;
                    //j = 0;
                }
                
            }
        }
    }
}
