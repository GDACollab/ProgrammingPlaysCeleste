using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Monocle.MInput;

namespace ProgrammingPlaysCeleste
{
    // Most of this is adapted from https://github.com/EverestAPI/CelesteTAS-EverestInterop/blob/master/CelesteTAS-EverestInterop/TAS/Manager.cs
    public static class InputManager
    {
        static GamePadState activeState;

        private delegate void UpdateVirtualInputs();

        private static readonly UpdateVirtualInputs UpdateInputs;

        static InputManager() {
            MethodInfo updateInputs = typeof(MInput).GetMethod("UpdateVirtualInputs", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            UpdateInputs = (UpdateVirtualInputs) updateInputs.CreateDelegate(typeof(UpdateVirtualInputs));
        }

        private static void AssignSticks(HashSet<Inputs> inputs, ref GamePadThumbSticks sticks, GamePadData activePad) {
            float x;
            float y;
            Vector2 currentInput = activePad.GetLeftStick();

            if (inputs.Contains(Inputs.Left))
            {
                x = -1.0f;
            }
            else if (inputs.Contains(Inputs.Right)) {
                x = 1.0f;
            } else
            {
                x = currentInput.X;
            }

            if (inputs.Contains(Inputs.Down))
            {
                y = -1.0f;
            }
            else if (inputs.Contains(Inputs.Up))
            {
                y = 1.0f;
            }
            else
            {
                y = currentInput.Y;
            }

            sticks = new GamePadThumbSticks(new Vector2(x, y), new Vector2(0.0f, 0.0f));
        }

        public static void SendFrameInput(HashSet<Inputs> input) {
            GamePadData activePad = default;

            bool found = false;
            for (int i = 0; i < 4; i++)
            {
                if (GamePads[i].Attached)
                {
                    activePad = GamePads[i];
                    found = true;
                }
            }

            if (!found)
            {
                GamePads[0].Attached = true;
                activePad = GamePads[0];
            }



            GamePadDPad pad = default;
            GamePadThumbSticks sticks = default;
            AssignSticks(input, ref sticks, activePad);

            Buttons aPressedActive = activePad.Check(Buttons.A) ? Buttons.A : 0;
            Buttons xPressedActive = activePad.Check(Buttons.X) ? Buttons.X : 0;

            
            GamePadButtons buttons = new GamePadButtons((input.Contains(Inputs.Jump) ? Buttons.A : aPressedActive)
                | (input.Contains(Inputs.Dash) ? Buttons.X : xPressedActive));

            activeState = new GamePadState(sticks, new GamePadTriggers(0.0f, input.Contains(Inputs.Climb)? 1.0f : activePad.Axis(Buttons.RightTrigger, 0.0f)), buttons, pad);
            // Celeste controls:
            // A - Jump
            // X - Dash
            // Right Trigger - Climb
            // Left Stick - Move

            if (found)
            {
                activePad.Update();
            }
            activePad.CurrentState = activeState;

            UpdateInputs();
        }
    }
}
