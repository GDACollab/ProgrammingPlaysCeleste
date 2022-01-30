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
        private static Dictionary<string, string> jsonData;
        private static List<string> solids;
        
        static GameReader() {
            position = new double[2];
            jsonData = new Dictionary<string, string>();
            solids = new List<string>();
        }

        public static void FrameUpdate(Level activeLevel) {
            Player player = activeLevel.Tracker.GetEntity<Player>();
            if (player != null) {
                position = GetAdjustedPos(player);
                jsonData["playerPos"] = "[" + position[0] + "," + position[1] + "]";
                GetLevelData();
            }
        }

        public static string GetJSON() {
            return JsonConvert.SerializeObject(jsonData);
        }

        private static void GetLevelData() {
            /*List<Entity> solidTilesList = Engine.Scene.Tracker.GetEntities<SolidTiles>();
            solidTilesList.ForEach(entity => {
                if (entity is SolidTiles && entity.Collidable) {
                    string entityData = "";
                    entityData += "{\"position\": [" + entity.X + "," + entity.Y + "],";
                    entityData += "\"bounds\": {\"top\": " + entity.Top + ", \"left\": " + entity.Left + ", \"right\": " + entity.Right + ", \"bottom\": " + entity.Bottom + "}}";
                    solids.Add(entityData);
                }
            });*/

            // This is a multi-step process, but here's how Celeste's levels work. The game is divided into Chapters (like the Forsaken City).
            // Each chapter has multiple screens (called Levels in the game's code).

            // So, first we need to get the current level:
            Level level = (Level) Engine.Scene;

            // Then, we need to search through the level's current entities:
            foreach (Entity e in level.Entities) {
                // We want to make sure that this is something Madeline can interact with. Otherwise, what is the script gonna do with it?
                if (!(e is Decal) && !(e is BackgroundTiles) && e.Collidable && e.Active && e.Visible) {
                    // Alright, now we pick and choose from what's left:

                    // SolidTiles is all of the solid tiles (It also technically counts as a Platform, which we might want to use when adding other platforms)
                    if (e is SolidTiles g) {
                        Grid t = g.Grid;

                        // We cooullld use the current grid to get the current cells, but that's also a terrible idea, because I'm pretty sure most of its functions
                        // are used to check collisions. So, we're probabblly going to want to send the raw data. We're also going to want to only do this once per level
                        // to save time.

                        /*for (int i = 0; i < t.CellsX; i++) {
                            for (int j = 0; j < t.CellsY; j++) {
                                printStr += t[i, j].ToString() + " ";
                            }
                            printStr += "\n";
                        }
                        Logger.Log("Programming Plays Celeste", printStr);*/
                        /*
                        Logger.Log("Programming Plays Celeste", "Bounds: " + h.Top + " " + h.Bottom + " " + h.Left + " " + h.Right + " " + h.Left);*/
                    }
                    Logger.Log("Programming Plays Celeste", e.ToString());
                }
            }
            jsonData["solids"] = JsonConvert.SerializeObject(solids);
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
