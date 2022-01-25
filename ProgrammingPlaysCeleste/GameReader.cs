using Celeste;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// A lot of this is modified code from https://github.com/EverestAPI/CelesteTAS-EverestInterop

namespace ProgrammingPlaysCeleste
{

    public static class GameReader
    {
        public static double[] position;
        private static Dictionary<string, string> jsonData;
        
        static GameReader() {
            position = new double[2];
            jsonData = new Dictionary<string, string>();
        }

        public static void FrameUpdate(Level activeLevel) {
            Player player = activeLevel.Tracker.GetEntity<Player>();
            if (player != null) {
                position = GetAdjustedPos(player);
                jsonData["playerPos"] = position.ToString();
            }
        }

        public static string GetJSON() {
            return JsonConvert.SerializeObject(jsonData);
        }


        // Directly modified from https://github.com/EverestAPI/CelesteTAS-EverestInterop/blob/master/CelesteTAS-EverestInterop/TAS/GameInfo.cs
        private static double[] GetAdjustedPos(Actor actor)
        {
            Vector2 intPos = actor.Position;
            Vector2 subpixelPos = actor.PositionRemainder;
            double x = intPos.X;
            double y = intPos.Y;
            double subX = subpixelPos.X;
            double subY = subpixelPos.Y;

            double[] positionArr = new double[2];

            // euni: ensure .999/.249 round away from .0/.25
            // .00/.25/.75 let you distinguish which 8th of a pixel you're on, quite handy when doing subpixel manip
            if (Math.Abs(subX) % 0.25 < 0.01 || Math.Abs(subX) % 0.25 > 0.24)
            {
                if (x > 0 || x == 0 && subX > 0)
                {
                    x += Math.Floor(subX * 100) / 100;
                }
                else
                {
                    x += Math.Ceiling(subX * 100) / 100;
                }
            }
            else
            {
                x += subX;
            }

            if (Math.Abs(subY) % 0.25 < 0.01 || Math.Abs(subY) % 0.25 > 0.24)
            {
                if (y > 0 || y == 0 && subY > 0)
                {
                    y += Math.Floor(subY * 100) / 100;
                }
                else
                {
                    y += Math.Ceiling(subY * 100) / 100;
                }
            }
            else
            {
                y += subY;
            }

            positionArr[0] = x;
            positionArr[1] = y;

            return positionArr;
        }
    }
}
