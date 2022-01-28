using System.Diagnostics;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;

namespace ProgrammingPlaysCeleste
{
    public enum Inputs { 
        Left,
        Right,
        Up,
        Down,
        Jump,
        Dash,
        Climb
    }

    public class ProgramCelesteModule : EverestModule
    {
        public static ProgramCelesteModule Instance;

        public ProgramCelesteModule() {
            Instance = this;
        }

        public override Type SettingsType => typeof(ProgramCelesteModuleSettings);
        public static ProgramCelesteModuleSettings Settings => (ProgramCelesteModuleSettings) Instance._Settings;

        static Process movementScripts;

        HashSet<Inputs> activeInputs;

        // Because for some reason I can't use Create[PropName]Entry in CelesteModuleSettings.cs, we're using this hacky workaround:
        public override void CreateModMenuSection(TextMenu menu, bool inGame, FMOD.Studio.EventInstance snapshot)
        {
            base.CreateModMenuSection(menu, inGame, snapshot);
            ProgramCelesteModuleSettings settings = ProgramCelesteModuleSettings.Instance;
            for (int i = 0; i < settings.NumberOfInputDivisions; i++)
            {
                TextMenu.Setting inputsAllowed = new TextMenu.Setting("Inputs allowed for #" + i, "LRUDJCZ");
                TextMenu.Setting folderName = new TextMenu.Setting("Folder name for #" + i, i.ToString());
                menu.Add(inputsAllowed);
                menu.Add(folderName);
            }
        }

        public override void Load() {
            On.Monocle.Engine.Update += UpdateGame;
            On.Monocle.MInput.Update += UpdateInput;

            movementScripts = Process.Start(new ProcessStartInfo("python", @"./Mods/ProgrammingPlaysCeleste/main.py") {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            activeInputs = new HashSet<Inputs>();
        }

        public override void Unload()
        {
            On.Monocle.Engine.Update -= UpdateGame;
            On.Monocle.MInput.Update -= UpdateInput;

            movementScripts.Kill();
        }

        private void UpdateInput(On.Monocle.MInput.orig_Update orig) {
            if (Engine.Instance.IsActive) {
                orig();
            }

            if (activeInputs.Count > 0)
            {
                InputManager.SendFrameInput(activeInputs);
            }
        }

        private void StringToInput(string input) {
            foreach (char item in input) {
                switch (item) {
                    case 'L':
                        activeInputs.Add(Inputs.Left);
                        break;
                    case 'R':
                        activeInputs.Add(Inputs.Right);
                        break;
                    case 'U':
                        activeInputs.Add(Inputs.Up);
                        break;
                    case 'D':
                        activeInputs.Add(Inputs.Down);
                        break;
                    case 'J':
                        activeInputs.Add(Inputs.Jump);
                        break;
                    case 'C':
                        activeInputs.Add(Inputs.Climb);
                        break;
                    case 'Z':
                        activeInputs.Add(Inputs.Dash);
                        break;
                    default:
                        Logger.Log("Programming Plays Celeste", "Unrecognized input char: " + input);
                        break;
                }
            }
        }

        private void UpdateGame(On.Monocle.Engine.orig_Update orig, Engine self, GameTime gameTime) {
            if (Engine.Scene is Level level)
            {
                GameReader.FrameUpdate(level);

                movementScripts.StandardInput.WriteLine(GameReader.GetJSON());

                if (!movementScripts.StandardOutput.EndOfStream) {
                    string input = movementScripts.StandardOutput.ReadLine();
                    Logger.Log("Programming Plays Celeste", "Input: " + input + "(length: " + input.Length + ")");
                    StringToInput(input);
                    orig(self, gameTime);
                }
            }
            else {
                orig(self, gameTime);
            }
            return;
        }

    }
}
