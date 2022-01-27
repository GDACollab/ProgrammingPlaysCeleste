using System.Diagnostics;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;

namespace ProgrammingPlaysCeleste
{
    public class ProgramCeleste : EverestModule
    {
        static Process movementScripts;

        static string activeInput;

        public override void Load() {
            On.Monocle.Engine.Update += UpdateGame;
            On.Monocle.MInput.Update += UpdateInput;

            movementScripts = Process.Start(new ProcessStartInfo("python", @"./Mods/ProgrammingPlaysCeleste/main.py") {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            });

            activeInput = "";
        }

        public override void Unload()
        {
            On.Monocle.Engine.Update -= UpdateGame;
            On.Monocle.MInput.Update -= UpdateInput;

            movementScripts.Kill();
        }

        private static void UpdateInput(On.Monocle.MInput.orig_Update orig) {
            if (Engine.Instance.IsActive) {
                orig();
            }

            if (activeInput != "")
            {
                Logger.Log("Programming Plays Celeste", "INPUT");
                InputManager.SendFrameInput(activeInput);
            }
        }

        private static void UpdateGame(On.Monocle.Engine.orig_Update orig, Engine self, GameTime gameTime) {
            Logger.Log("Programming Plays Celeste", MInput.GamePads[0].CurrentState.ThumbSticks.Left.ToString());
            if (Engine.Scene is Level level)
            {
                GameReader.FrameUpdate(level);

                movementScripts.StandardInput.WriteLine(GameReader.GetJSON());

                if (!movementScripts.StandardOutput.EndOfStream) {
                    string input = movementScripts.StandardOutput.ReadLine();
                    Logger.Log("Programming Plays Celeste", "Input: " + input + "(length: " + input.Length + ")");
                    activeInput = input;
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
