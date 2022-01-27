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
    public class ProgrammingPlaysCelesteSettings : EverestModuleSettings
    {
        [SettingRange(1, 10)]
        public int NumberOfInputDivisions { get; set; } = 2;
        public void CreateInputDivisionEntry(TextMenu menu, bool inGame)
        {
            Logger.Log("Programming Plays Celeste", "?A?A?>WEFDSAJKFLSDKJFLKSDFJLOSDKFJLDS");
            for (int i = 0; i < NumberOfInputDivisions; i++) {
                Logger.Log("Programming Plays Celeste", "?A?A?>");
                Setting inputsAllowed = new Setting("Inputs allowed for #" + i, "LRUDJCZ");
                Setting folderName = new Setting("Folder name for #" + i, i.ToString());
                menu.Add(inputsAllowed);
                menu.Add(folderName);
            }
        }

        #region Test

        public bool TestExample { get; set; } = false;

        #endregion
    }
}
