using System.Diagnostics;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using Celeste.Mod.UI;
/*
 * Let's make some TODOs:
 * Data for easily turning the solids array into a grid (subtract levelOffset from x and y? And maybe include width and height of level?)
 * Add options in the python script to set a new combination every time you reload (TEST IF THIS WORKS)
 * Make tutorials for how to use all of this
 * Interest survey out by 2/13?
 * Draft and tutorials done by 2/20?
 * Publish repository by 2/20?
 * To POLISH:
 * Acutally initialize the code for searching for goals to avoid spending more processing time on it.
 * Filter output from python to only accept print statements from main.py (maybe like a header before the print statement?)
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

        HashSet<Inputs> activeInputs;

        public override void Load() {
            On.Monocle.Engine.Update += UpdateGame;
            On.Monocle.MInput.Update += UpdateInput;

            movementScripts = Process.Start(new ProcessStartInfo("python", @"./Mods/ProgrammingPlaysCeleste/main.py") {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = false
            });

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
                GameReader.FrameUpdate(level);
                movementScripts.StandardInput.WriteLine(GameReader.GetJSON());
                GameReader.Cleanup();

                // Unless the program has been closed, we continue to read from it:
                if (!movementScripts.StandardOutput.EndOfStream)
                {
                    // We keep reading until we get the OK from the main.py script:
                    string input = "";
                    while (!input.Contains("--END OF INPUT STRING--")) {
                        input += movementScripts.StandardOutput.ReadLine();
                    }
                    input = input.Replace("--END OF INPUT STRING--", "");
                    input = input.Replace("\n", "");
                    StringToInput(input);
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
