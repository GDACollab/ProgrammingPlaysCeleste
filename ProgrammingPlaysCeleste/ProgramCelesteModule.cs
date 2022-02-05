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
 * Make sure platform data accounts for moving platforms and one-way platforms (and their hitboxes!).
 * Add data for player velocity, whether or not they can jump, climb.
 * Add data for time
 * Add data for level name (and tie that in to only sending solid data when the level name is new)
 * Add options to reset the level and go through every possible script combination after X amount of time.
 * Make tutorials for how to use all of this
 * Interest survey out by 2/13?
 * Draft and tutorials done by 2/20?
 * Publish repository by 2/20?
 * To POLISH:
 * Give initialization stuff for setting goals to make searching easier.
 * Make sure main.py filters data correctly.
 * Make sure python scripts can read the coordinate data and act appropriately.
 * Test this works with Mac and Linux computers
 * Add data for strawberries?
 * Set goal to end level trigger
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

        static string currentInputs = "";

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

                if (!movementScripts.StandardOutput.EndOfStream) {
                    string input = movementScripts.StandardOutput.ReadLine();
                    currentInputs += input;
                    if (currentInputs.Contains("--END OF INPUT STRING--")) {
                        currentInputs = currentInputs.Replace("--END OF INPUT STRING--", "");
                        StringToInput(currentInputs);
                        currentInputs = "";
                        orig(self, gameTime);
                    }
                }
            }
            else {
                orig(self, gameTime);
            }
            return;
        }

    }
}
