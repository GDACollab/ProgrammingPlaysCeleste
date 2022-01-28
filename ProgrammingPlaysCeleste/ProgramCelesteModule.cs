using System.Diagnostics;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using Celeste.Mod.UI;

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
        public static ProgramCelesteModuleSettings Settings => ProgramCelesteModuleSettings.Instance;

        static Process movementScripts;

        HashSet<Inputs> activeInputs;

        private void DrawInputDivisions(TextMenu menu, ref TextMenu.Item[] inputDivisions) {
            for (int i = 0; i < 10; i++)
            {
                int number = i;
                TextMenu.Item inputsAllowed = new TextMenu.Button("Inputs allowed for #" + number + ": " + Settings.InputDivisions[number]).Pressed(
                    () => {
                        menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(Settings.InputDivisions[number], value => Settings.InputDivisions[number] = value);
                    });
                TextMenu.Item folderName = new TextMenu.Button("Folder name for #" + number + ": " + Settings.InputFolderNames[number]).Pressed(
                    () => {
                        menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(Settings.InputFolderNames[number], value => Settings.InputFolderNames[number] = value);
                    });
                menu.Add(inputsAllowed);
                menu.Add(folderName);
                if (i >= Settings.NumberOfInputDivisions) {
                    inputsAllowed.Visible = false;
                    folderName.Visible = false;
                }

                inputDivisions[i * 2] = inputsAllowed;
                inputDivisions[(i * 2) + 1] = folderName;
            }
        }

        private void UpdateInputDivisions(int num, ref TextMenu.Item[] inputDivisions) {
            for (int i = 0; i < inputDivisions.Length; i++) {
                inputDivisions[i].Visible = (i < (num * 2));
            }
        }

        // Because for some reason I can't use Create[PropName]Entry in CelesteModuleSettings.cs, we're using this hacky workaround:
        public override void CreateModMenuSection(TextMenu menu, bool inGame, FMOD.Studio.EventInstance snapshot)
        {
            base.CreateModMenuSection(menu, inGame, snapshot);

            TextMenu.Slider numDivisions = new TextMenu.Slider("Number of Input Divisions", (number) => { return number.ToString(); }, 1, 10);
            numDivisions.Index = Settings.NumberOfInputDivisions;
            menu.Add(numDivisions);

            // 2 buttons * 10 total input divisions.
            TextMenu.Item[] inputDivisions = new TextMenu.Item[20];

            DrawInputDivisions(menu, ref inputDivisions);

            numDivisions.Change((value)=> {
                Settings.NumberOfInputDivisions = value;
                UpdateInputDivisions(value, ref inputDivisions);
            });
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
                    case 'X':
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
