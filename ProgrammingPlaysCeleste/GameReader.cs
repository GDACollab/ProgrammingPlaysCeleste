using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
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
        private static Dictionary<string, object> jsonData;
        private static List<float[]> solids;
        
        static GameReader() {
            position = new double[2];
            jsonData = new Dictionary<string, object>();
            solids = new List<float[]>();
        }

        public static void Load() {
            On.Celeste.Level.LoadLevel += GetLevelData;
            On.Monocle.EntityList.DebugRender += Debug;
        }

        public static void Unload() {
            On.Celeste.Level.LoadLevel -= GetLevelData;
            On.Monocle.EntityList.DebugRender -= Debug;
        }

        public static void FrameUpdate(Level activeLevel) {
            Player player = activeLevel.Tracker.GetEntity<Player>();
            if (player != null) {
                position = GetAdjustedPos(player);
                jsonData["playerPos"] = position;
            }
        }

        public static string GetJSON() {
            return JsonConvert.SerializeObject(jsonData);
        }

        private static void GetLevelData(On.Celeste.Level.orig_LoadLevel orig, Level self, Player.IntroTypes playerIntro, bool isFromLoader) {
            orig(self, playerIntro, isFromLoader);
            SolidTiles tiles = self.SolidTiles;
            Grid g = tiles.Grid;

            solids.Clear();

            // This is modified code from https://github.com/EverestAPI/CelesteTAS-EverestInterop/blob/master/CelesteTAS-EverestInterop/EverestInterop/Hitboxes/HitboxSimplified.cs
            // This gets the whole of the current screen:
            int left = (int)Math.Max(0.0f, (self.LevelOffset.X - g.AbsoluteLeft) / g.CellWidth);
            int right = (int)Math.Min(g.CellsX - 1, Math.Ceiling((self.LevelOffset.X + self.Bounds.Width - (double)g.AbsoluteLeft)/g.CellWidth));
            int top = (int)Math.Max(0.0f, (self.LevelOffset.Y - g.AbsoluteTop) / g.CellHeight);
            int bottom = (int)Math.Min(g.CellsY - 1, Math.Ceiling((self.LevelOffset.Y + self.Bounds.Height - (double)g.AbsoluteTop) / g.CellHeight));

            for (int x = left; x <= right; ++x)
            {
                for (int y = top; y <= bottom; ++y)
                {
                    if (g[x, y]) {
                        solids.Add(new float[] { x * g.CellWidth + g.AbsolutePosition.X, y * g.CellHeight + g.AbsolutePosition.Y });
                    }
                }
            }

            Logger.Log("Programming Plays Celeste", $"{self.Session.Level} {self.Session.MapData.Filename}");


            jsonData["solids"] = solids;

            Vector2 goal = new Vector2(0, 0);
            // TODO: Revise heavily. This is a pretty hack-y workaround to find the next place to go to.
            string path = $"./Mods/ProgrammingPlaysCeleste/courses/{self.Session.MapData.Filename}.txt";
            if (System.IO.File.Exists(path))
            {
                string[] courseRoute = System.IO.File.ReadAllLines(path);
                Logger.Log("Programming Plays Celeste", courseRoute.ToString());
                int index;
                for (index = 0; courseRoute[index] != $"lvl_{self.Session.Level}" && index < courseRoute.Length; index++);
                string goalNext = courseRoute[index + 1].Replace("lvl_", "");
                LevelData nextLevel = self.Session.MapData.Levels.Find((LevelData data) => {
                    return data.Name == goalNext;
                });
                if (nextLevel != null) {
                    // Hacky workaround for finding goals based on the next level:
                    goal = nextLevel.Spawns[0];
                    Logger.Log("Programming Plays Celeste", $"New Goal: {goal}");
                }
            }
            jsonData["goal"] = new float[] { goal.X, goal.Y };
        }

        private static void Debug(On.Monocle.EntityList.orig_DebugRender orig, EntityList self, Camera camera) {
            orig(self, camera);
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
