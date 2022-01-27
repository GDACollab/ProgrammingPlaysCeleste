using System.Diagnostics;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;

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

    public class ProgramCeleste : EverestModule
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
