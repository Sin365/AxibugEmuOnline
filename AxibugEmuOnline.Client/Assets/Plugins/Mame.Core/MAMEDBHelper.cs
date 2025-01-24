using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;


namespace MAME.Core
{
    public class MAMEDBHelper
    {
        public static void LoadROMXML(string xmbString)
        {
            XElement xe = XElement.Parse(xmbString);
            IEnumerable<XElement> elements = from ele in xe.Elements("game") select ele;
            showInfoByElements(elements);
        }

        static void showInfoByElements(IEnumerable<XElement> elements)
        {
            RomInfo.romList = new List<RomInfo>();
            RomInfo.dictName2Rom = new Dictionary<string, RomInfo>();
            foreach (var ele in elements)
            {
                RomInfo rom = new RomInfo();
                rom.Name = ele.Attribute("name").Value;
                rom.Board = ele.Attribute("board").Value;
                rom.Parent = ele.Element("parent").Value;
                rom.Direction = ele.Element("direction").Value;
                rom.Description = ele.Element("description").Value;
                rom.Year = ele.Element("year").Value;
                rom.Manufacturer = ele.Element("manufacturer").Value;
                RomInfo.romList.Add(rom);
                RomInfo.dictName2Rom[rom.Name] = rom;
                //loadform.listView1.Items.Add(new ListViewItem(new string[] { rom.Description, rom.Year, rom.Name, rom.Parent, rom.Direction, rom.Manufacturer, rom.Board }));
            }
        }
    }
}
