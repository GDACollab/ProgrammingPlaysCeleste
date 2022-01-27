using Celeste;
using Celeste.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Celeste.TextMenu;

namespace ProgrammingPlaysCeleste
{
    [SettingName("modoptions_programmingplaysceleste_title")]
    class ProgrammingPlaysCelesteSettings : EverestModuleSettings
    {
        public int NumberOfInputDivisions { get; set; } = 2;
        public void CreateInputDivisionEntry(TextMenu menu, bool inGame) {
            Option<string> divisions = new Option<string>("Input Divisions");
            for (int i = 0; i < NumberOfInputDivisions; i++) {
                divisions.Add("Inputs allowed for #" + i, "LRUDJCZ");
                divisions.Add("Folder name for #" + i, i.ToString());
            }
            menu.Add(divisions);
        }
    }
}
