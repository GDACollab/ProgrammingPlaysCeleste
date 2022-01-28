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
        public static ProgramCelesteModuleSettings Instance { get; private set; }

        public ProgramCelesteModuleSettings() {
            Instance = this;
        }

        [SettingIgnore]
        public int NumberOfInputDivisions { get; set; } = 2;

        [SettingIgnore] //Because we don't want the array to be dynamically resized for some reason or another (we'd lose settings progress!), we just initialize it with everything we need:
        public string[] InputDivisions { get; set; } = { "LRUDJCX", "LRUDJCX", "LRUDJCX", "LRUDJCX", "LRUDJCX", "LRUDJCX", "LRUDJCX", "LRUDJCX", "LRUDJCX", "LRUDJCX" };

        [SettingIgnore] // Same with folder names:
        public string[] InputFolderNames { get; set; } = { "head", "feet", "face", "neck", "stomach", "waist", "knee", "hand", "finger", "arm" };
    }
}
