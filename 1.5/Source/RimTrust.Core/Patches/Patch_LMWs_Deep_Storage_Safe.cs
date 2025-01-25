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

namespace RimTrust.Core.Patches
{
    public class Patch_LMWsDeepStorageSafe
    {

        public static void Patch_LMWsDeepStorageSafeForBanknotes()
        {
            //Log.Message("RimTrust: updateXML with System.Xml start for LMW Deep Storage Safe for Banknotes");
            string path = System.IO.Path.Combine(GenFilePaths.ModsFolderPath);
            path = path.Replace("Mods", "");
            path = path.Replace("RimWorld", "");
            path = path.Replace("common", "");
            path = path.Remove(path.Length - 1);
            path = path.Remove(path.Length - 1);
            path = System.IO.Path.Combine(path, "workshop", "content", "294100", "1617282896", "1.5", "Defs", "Deep_Storage.xml");
            //Log.Message(path);

            if (System.IO.File.Exists(path))
            {
                //Log.Message("RimTrust: opening LMWs XML file");
                //Log.Message("RimTrust: opening LMWs XML path is " + path);
                XmlDocument xmldoc = new XmlDocument();
                xmldoc.Load(path);
                //Log.Message("RimTrust: setting root to ThingDef");
                string defName = "LWM_Safe";
                //Log.Message("RimTrust: selecting Node with LWM_Safe");
                XmlNodeList nodeList = xmldoc.SelectNodes("/Defs/ThingDef/defName");
                //Log.Message("RimTrust: start of foreach loop parsing nodeList for LWM_Safe");
                foreach (XmlNode node in nodeList)
                {
                    //Log.Message("RimTrust: current node is " + node.InnerText.ToString() );
                    if (node.InnerText.ToString() == defName)
                    {
                        //Log.Message("RimTrust: trying to find target node");
                        
                        XmlNode targetNode = node.ParentNode;

                        targetNode = targetNode.ChildNodes[11].ChildNodes[2].ChildNodes[1].ChildNodes[1];
                        if (targetNode.ChildNodes[4].InnerText != "BankNote")
                        {
                            //Log.Message("RimTrust: before trying to add element with attribute for mayrequire occam1st.RimTrust");
                            XmlElement id = xmldoc.CreateElement("li");
                            id.SetAttribute("MayRequire","occam1st.RimTrust");
                            //Log.Message("RimTrust: before adding innertext BankNote");
                            id.InnerText = "BankNote";
                            //Log.Message("RimTrust: before inserting element to targetnode");
                            targetNode.InsertAfter(id, targetNode.LastChild);
                            xmldoc.Save(path);
                        }
                    }
                }
                
            }
        }
    }
}
