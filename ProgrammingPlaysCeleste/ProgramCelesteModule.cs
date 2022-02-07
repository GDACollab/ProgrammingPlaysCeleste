using System.Diagnostics;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
/*
 * Let's make some TODOs:
 * Make storage of coordinates consistent (Some are in the center, but some are not I think?)
 * Make tutorials for how to use all of this
 * IMAGES!
 * Interest survey out by 2/13?
 * Draft and tutorials done by 2/20?
 * Publish repository by 2/20?
 * To POLISH:
 * Acutally initialize the code for searching for goals to avoid spending more processing time on it.
 * Figure out some way to execute the local OS terminal instead of python (that way, when python crashes, the terminal stays open)
 * Test this works with Mac and Linux computers
 * Set goal to end level trigger on the final level
 */

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
        static Process movementScripts;
        private readonly ProcessStartInfo movementStartInfo = new ProcessStartInfo("python", @"./Mods/ProgrammingPlaysCeleste/main.py")
        {
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            CreateNoWindow = false
        };

        bool scriptReady = false;

        HashSet<Inputs> activeInputs;

        public static string currLevel = "";

        public override void Initialize()
        {
            Engine.Commands.FunctionKeyActions[9] = () =>
            {
                if (currLevel != "" && Engine.Scene is Level level) {
                    if (!movementScripts.HasExited) {
                        movementScripts.Kill();
                    }

                    movementScripts.Start();
                    scriptReady = false;

                    if (MInput.Keyboard.Check(Microsoft.Xna.Framework.Input.Keys.LeftControl))
                    {
                        SaveData.Instance.CurrentSession = new Session(new AreaKey((int)Char.GetNumericValue(currLevel[0]), SaveData.Instance.CurrentSession.Area.Mode));
                        Engine.Scene = new LevelLoader(SaveData.Instance.CurrentSession);
                        
                    }
                    else {
                        // We have to treat the game as if we've just reloaded the level:
                        GameReader.GetLevelData((Level self, Player.IntroTypes intro, bool fromLoader) => { return; }, level, default, default);
                    }
                }
            };
        }

        public override void Load() {
            On.Monocle.Engine.Update += UpdateGame;
            On.Monocle.MInput.Update += UpdateInput;

            movementScripts = Process.Start(movementStartInfo);

            activeInputs = new HashSet<Inputs>();

            GameReader.Load();
        }

        public override void Unload()
        {
            On.Monocle.Engine.Update -= UpdateGame;
            On.Monocle.MInput.Update -= UpdateInput;
            GameReader.Unload();

            movementScripts.StandardInput.Write("FINISHED");
        }

        private void UpdateInput(On.Monocle.MInput.orig_Update orig) {
            if (Engine.Instance.IsActive)
            {
                orig();
            }

            if (activeInputs.Count > 0)
            {
                InputManager.SendFrameInput(activeInputs);
            }
        }

        private void StringToInput(string input) {
            activeInputs.Clear();
            string printStr = "";
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
                        printStr += item;
                        break;
                }
            }
            Logger.Log("Programming Plays Celeste", "Full String: " + input + " Unidentified Inputs:" + printStr);
        }

        private void UpdateGame(On.Monocle.Engine.orig_Update orig, Engine self, GameTime gameTime) {
            if (Engine.Scene is Level level)
            {
                currLevel = level.Session.MapData.Filename;
                GameReader.FrameUpdate(level);
                movementScripts.StandardInput.WriteLine(GameReader.GetJSON());
                GameReader.Cleanup();

                if (!scriptReady) {
                    string input = movementScripts.StandardOutput.ReadLine();
                    if (input.Contains("--READY--")) {
                        scriptReady = true;
                    }
                }
                if (!movementScripts.StandardOutput.EndOfStream) // Unless the program has been closed, we continue to read from it:
                {
                    // We keep reading until we get the OK from the main.py script:
                    string input = "";
                    while (!input.Contains("--START OF INPUT STRING--")) {
                        input += movementScripts.StandardOutput.ReadLine();
                    }

                    int inputStartIndex = input.IndexOf("--START OF INPUT STRING--");
                    string startInput = input.Substring(inputStartIndex);

                    while (!startInput.Contains("--END OF INPUT STRING--")) {
                        startInput+= movementScripts.StandardOutput.ReadLine();
                    }
                    startInput = startInput.Replace("--START OF INPUT STRING--", "");
                    startInput = startInput.Replace("--END OF INPUT STRING--", "");
                    startInput = startInput.Replace("\n", "");
                    StringToInput(startInput);
                    orig(self, gameTime);
                }
                else {
                    StringToInput("");
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
