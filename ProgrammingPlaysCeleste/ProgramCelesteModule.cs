using CliWrap;
using System.Collections.Generic;
using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.IO;
using System.Text;
using System.Threading;
/*
* TODO:
* Figure out some way to execute the local OS terminal instead of python (that way, when python crashes, the terminal stays open)
* Test this works with Mac and Linux computers
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
        static CommandResult movementScripts;
        CancellationTokenSource cts;
        StreamWriter inputWriter;
        Stream pythonInput;
        StreamReader outputReader;
        Stream pythonOutput;

        bool scriptReady = false;

        HashSet<Inputs> activeInputs;

        public static string currLevel = "";

        public override void Initialize()
        {
            Engine.Commands.FunctionKeyActions[9] = () =>
            {
                if (currLevel != "" && Engine.Scene is Level level) {
                    if (!(movementScripts.ExitCode == 0)) {
                        cts.Cancel();
                    }

                    StartProcess();
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

        private void StartProcess() {
            cts = new CancellationTokenSource();

            pythonInput = new MemoryStream();
            inputWriter = new StreamWriter(pythonInput);
            inputWriter.AutoFlush = true;
            PipeSource source = PipeSource.FromStream(pythonInput, true);

            pythonOutput = new MemoryStream();
            outputReader = new StreamReader(pythonOutput);
            PipeTarget target = PipeTarget.ToStream(pythonOutput);

            var cmd = Cli.Wrap("python").WithArguments("./Mods/ProgrammingPlaysCeleste/main.py").WithStandardInputPipe(source).WithWorkingDirectory(Directory.GetCurrentDirectory());
            cmd.WithStandardOutputPipe(target).WithStandardErrorPipe(target);
            var executed = cmd.ExecuteAsync();

            Logger.Log("Programming Plays Celeste", outputReader.ReadToEnd());
            // The only problem (and maybe a feature to add?) Is that we need a physical window to show what's happening. I'm thinking we create our own window here that mirrors the input and outputs we recieve.
        }

        public override void Load() {
            On.Monocle.Engine.Update += UpdateGame;
            On.Monocle.MInput.Update += UpdateInput;

            StartProcess();

            activeInputs = new HashSet<Inputs>();

            GameReader.Load();
        }

        public override void Unload()
        {
            On.Monocle.Engine.Update -= UpdateGame;
            On.Monocle.MInput.Update -= UpdateInput;
            GameReader.Unload();

            inputWriter.WriteLine("FINISHED");
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
                inputWriter.WriteLine(GameReader.GetJSON());
                GameReader.Cleanup();

                pythonOutput.Position = 0;
                Logger.Log("Programming Plays Celeste", outputReader.ReadToEnd());

                /*if (!scriptReady) {
                    if (movementScripts.StandardOutput.Contains("--READY--")) {
                        scriptReady = true;
                    }
                }
                if (scriptReady && movementScripts.StandardOutput.Contains("--START OF INPUT STRING--")) // Unless the program has been closed, we continue to read from it:
                {
                    // We keep reading until we get the OK from the main.py script:
                    while (!movementScripts.StandardOutput.Contains("--START OF INPUT STRING--")) {
                        continue;
                    }

                    int inputStartIndex = movementScripts.StandardOutput.IndexOf("--START OF INPUT STRING--");

                    while (!movementScripts.StandardOutput.Contains("--END OF INPUT STRING--")) {
                        continue;
                    }
                    string input = movementScripts.StandardOutput.Substring(inputStartIndex, movementScripts.StandardOutput.IndexOf("--END OF INPUT STRING--") - inputStartIndex);

                    input = input.Replace("--START OF INPUT STRING--", "");
                    input = input.Replace("--END OF INPUT STRING--", "");
                    input = input.Replace("\n", "");
                    StringToInput(input);

                    orig(self, gameTime);
                }
                else {
                    StringToInput("");
                    orig(self, gameTime);
                }*/
            }
            else {
                orig(self, gameTime);
            }
            return;
        }

    }
}
