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
            string path = "F:\\Steam\\steamapps\\common\\RimWorld\\Mods\\RimTrust\\Defs\\ThingDefs_Buildings\\Buildings_TrustLedgerConsole.xml";
            XmlDocument doc = new XmlDocument();
            //Log.Message("updateXML with System.Xml before load");
            doc.Load(path);

            XmlNode ThingDef;
            XmlNode root = doc.DocumentElement;

            //Log.Message("updateXML with System.Xml before root.SelectSingleNode");
            ThingDef = root.SelectSingleNode("/Defs/ThingDef/comps/li/basePowerConsumption[1]");

            //Log.Message("updateXML with System.Xml before string newPowerConsumption");
            string newPowerConsumption = (200 - Math.Floor(RimTrust.Trade.Methods.LegacyPower * 0.000025)).ToString();
            //Log.Message("updateXML with System.Xml before assining string newPowerConsumption to LastChild.InnerText");
            ThingDef.LastChild.InnerText = newPowerConsumption;

            //Log.Message("updateXML with System.Xml before save");
            doc.Save(path);
        }
    }
}
