using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Verse;

namespace RimTrust.Core.Patches
{
    public class UpdateXML
    {
        
        public static void UpdateTLCBasePowerConsumptionFromLegacyPower()
        {
            //Log.Message("updateXML with System.Xml start");
            string path = System.IO.Path.Combine(GenFilePaths.ModsFolderPath, "RimTrust", "Defs", "ThingDefs_Buildings", "Buildings_TrustLedgerConsole.xml");
            string path2 = System.IO.Path.Combine(GenFilePaths.ModsFolderPath);
            path2 = path2.Replace("Mods", "");
            path2 = path2.Replace("RimWorld", "");
            path2 = path2.Replace("common", "");
            path2 = path2.Remove(path2.Length - 1);
            path2 = path2.Remove(path2.Length - 1);
            path2 = System.IO.Path.Combine(path2, "workshop", "content", "294100", "2590341520", "Defs", "ThingDefs_Buildings", "Buildings_TrustLedgerConsole.xml");
            //Log.Message(path2);

            if (System.IO.File.Exists(path))
            { 
                XmlDocument doc = new XmlDocument();
                doc.Load(path);
                XmlNode ThingDef;
                XmlNode root = doc.DocumentElement;
                ThingDef = root.SelectSingleNode("/Defs/ThingDef/comps/li/basePowerConsumption[1]");
                string newPowerConsumption = (200 - Math.Floor(RimTrust.Trade.Methods.LegacyPower * 0.000025)).ToString();
                ThingDef.LastChild.InnerText = newPowerConsumption;
                doc.Save(path);
            }
            if (System.IO.File.Exists(path2))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(path2);
                XmlNode ThingDef;
                XmlNode root = doc.DocumentElement;
                ThingDef = root.SelectSingleNode("/Defs/ThingDef/comps/li/basePowerConsumption[1]");
                string newPowerConsumption = (200 - Math.Floor(RimTrust.Trade.Methods.LegacyPower * 0.000025)).ToString();
                ThingDef.LastChild.InnerText = newPowerConsumption;
                doc.Save(path2);
            }

        }
    }
}
