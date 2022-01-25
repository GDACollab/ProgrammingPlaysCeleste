using Celeste.Mod;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProgrammingPlaysCeleste
{
    // Most of this is adapted from https://github.com/EverestAPI/CelesteTAS-EverestInterop/blob/master/CelesteTAS-EverestInterop/TAS/Manager.cs
    public static class InputManager
    {
        static GamePadState activeState;

        public static void SendFrameInput(string input) {
            GamePadDPad pad = default;
            GamePadThumbSticks sticks = new GamePadThumbSticks(new Vector2(1.0f, 1.0f), new Vector2(1.0f, 1.0f));
            activeState = new GamePadState(sticks, new GamePadTriggers(0, 0), new GamePadButtons(), pad);
            // Celeste controls:
            // A - Jump
            // X - Dash
            // Right Trigger - Climb
            // Left Stick - Move
            //KeyboardState keyboard = new KeyboardState(Keys.Right);
            //MInput.Keyboard.CurrentState = keyboard;
            bool found = false;
            for (int i = 0; i < 4; i++)
            {
                MInput.GamePads[i].Update();
                if (MInput.GamePads[i].Attached)
                {
                    found = true;
                    MInput.GamePads[i].CurrentState = activeState;
                }
            }

            if (!found)
            {
                MInput.GamePads[0].CurrentState = activeState;
                MInput.GamePads[0].Attached = true;
            }
        }
    }
}
