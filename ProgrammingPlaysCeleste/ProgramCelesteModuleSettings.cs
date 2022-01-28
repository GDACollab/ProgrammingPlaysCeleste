using Celeste;
using Celeste.Mod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingPlaysCeleste
{
    public class ProgramCelesteModuleSettings : EverestModuleSettings
    {
        public static ProgramCelesteModuleSettings Instance;

        ProgramCelesteModuleSettings() {
            Instance = this;
        }

        [SettingRange(1, 10)]
        public int NumberOfInputDivisions { get; set; } = 2;

    }
}
