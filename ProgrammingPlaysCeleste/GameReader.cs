using Celeste;
using Celeste.Mod;
using Microsoft.Xna.Framework;
using Monocle;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


// A lot of this is modified code from https://github.com/EverestAPI/CelesteTAS-EverestInterop

/* JSON Summary:
 * playerPos: float[2], indicates player position (The center of where they are)
 * playerSize: float[2], indicates player hitbox size (X, Y).
 * player: Dictionary<string, object>, {position: float[2], size: float[2], speed: float[2], onGround: bool, wallJump (if you can wall jump or climb. -1 is left facing wall, 1 is right facing wall, 2 if you're somehow touching both.): int, numDashes: int, dashTimer (how long until you're able to dash): float, stamina (how long you can stay wall climbing): int}
 * tileSize: float[2], the width and height for the solids' hitboxes.
 * goal: float[2], indicates the goal for the player to reach. Updates on a new level transition.
 * solids: List<float[]>, array of solid tile positions (the center of where they are). Updates on a new level position.
 * platforms: List<object>, array of objects {position: float[2], size: float[2], type: string} to show platforms, their position (the center of where they are), and size indicates their hitbox size.
 * hazards: List<object>, array of objects that kill on collision {position: float[2], size: float[2]}
 * levelName: The name of the current level/screen you're on.
 */
namespace ProgrammingPlaysCeleste
{

    public static class GameReader
    {

        private static float[] position;
        private static float[] size;
        private static Dictionary<string, object> jsonData;
        private static Dictionary<string, object> playerData;
        private static List<float[]> solids;
        private static List<object> hazards;
        private static List<object> platforms;

        private static readonly DWallJumpCheck WallJumpCheck;
        private static readonly FieldInfo dashCooldownTimer;
        
        static GameReader() {
            position = new float[2];
            jsonData = new Dictionary<string, object>();
            solids = new List<float[]>();
            hazards = new List<object>();
            platforms = new List<object>();
            playerData = new Dictionary<string, object>();

            dashCooldownTimer = typeof(Player).GetField("dashCooldownTimer", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

            MethodInfo wallJumpCheck = typeof(Player).GetMethod("WallJumpCheck", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            WallJumpCheck = (DWallJumpCheck)wallJumpCheck.CreateDelegate(typeof(DWallJumpCheck));
        }

        public static void Load() {
            On.Celeste.Level.LoadLevel += GetLevelData;
            On.Celeste.Level.Begin += LevelBeginGetData;
        }

        public static void Unload() {
            On.Celeste.Level.LoadLevel -= GetLevelData;
            On.Celeste.Level.Begin -= LevelBeginGetData;
        }

        public static void FrameUpdate(Level activeLevel) {
            Player player = activeLevel.Tracker.GetEntity<Player>();
            if (player != null) {
                position = new float[] { player.Center.X, player.Center.Y };
                size = new float[] { player.Collider.Width, player.Collider.Height };
                playerData["position"] = position;
                playerData["size"] = size;
                playerData["numDashes"] = player.Dashes;
                playerData["dashTimer"] = dashCooldownTimer.GetValue(player);
                // TODO: Check this works.
                playerData["onGround"] = player.LoseShards;
                int canJump = 0;
                if (WallJumpCheck(player, -1)) {
                    canJump = -1;
                }
                if (WallJumpCheck(player, 1)) {
                    // In case you're able to wall-jump simultaneously?
                    canJump = Math.Abs(canJump) + 1;
                }
                playerData["wallJump"] = canJump;
                playerData["stamina"] = player.Stamina;
                playerData["speed"] = new float[] { player.Speed.X, player.Speed.Y };
                jsonData["player"] = playerData;
                UpdateLevelData(activeLevel);
            }
        }

        public static void Cleanup() {
            // Remove the solids if they've already been sent (we do this once per level load, otherwise it's a constant stream of unnecessary data the user already knows)
            if (jsonData.ContainsKey("solids"))
            {
                jsonData.Remove("solids");
            }
        }

        public static string GetJSON() {
            return JsonConvert.SerializeObject(jsonData);
        }

        private static void LevelBeginGetData(On.Celeste.Level.orig_Begin orig, Level self) {
            orig(self);
            jsonData["tileSize"] = new float[] { self.SolidTiles.Grid.CellWidth, self.SolidTiles.Grid.CellHeight };
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


            jsonData["solids"] = solids;

            Vector2 goal = new Vector2(0, 0);
            // TODO: Revise heavily. This is a pretty hack-y workaround to find the next place to go to.
            string path = $"./Mods/ProgrammingPlaysCeleste/courses/{self.Session.MapData.Filename}.txt";
            if (System.IO.File.Exists(path))
            {
                string[] courseRoute = System.IO.File.ReadAllLines(path);
                int index;
                for (index = 0; courseRoute[index] != $"lvl_{self.Session.Level}" && index < courseRoute.Length; index++);
                if (courseRoute[index] == $"lvl_{self.Session.Level}" && index != courseRoute.Length - 1) {
                    string goalNext = courseRoute[index + 1].Replace("lvl_", "");
                    LevelData nextLevel = self.Session.MapData.Levels.Find((LevelData data) => {
                        return data.Name == goalNext;
                    });
                    if (nextLevel != null)
                    {
                        // Hacky workaround for finding goals based on the next level:
                        goal = nextLevel.Spawns[0];
                        Logger.Log("Programming Plays Celeste", $"New Goal: {goal}");
                    }
                }
            }
            jsonData["goal"] = new float[] { goal.X, goal.Y };

            jsonData["levelName"] = "lvl_" + self.Session.Level;
        }


        public static void UpdateLevelData(Level level) {
            hazards.Clear();
            platforms.Clear();
            foreach (Entity e in level.Entities) {
                if (e.Collidable) {
                    if (e.GetType() == typeof(Spikes))
                    {
                        Dictionary<string, object> hazard = new Dictionary<string, object>();
                        hazard.Add("position", new float[] { e.Position.X, e.Position.Y });
                        hazard.Add("size", new float[] { e.Collider.Width, e.Collider.Height });
                        hazards.Add(hazard);
                    }

                    if (e.GetType() != typeof(SolidTiles) && e is Platform p) {
                        Dictionary<string, object> platform = new Dictionary<string, object>();
                        platform.Add("position", new float[] { p.Center.X, p.Center.Y });
                        platform.Add("size", new float[] { p.Collider.Width, p.Collider.Height });
                        platform.Add("type", e.GetType().ToString());
                        platforms.Add(platform);
                    }
                }
            }
            jsonData["hazards"] = hazards;
            jsonData["platforms"] = platforms;
        }

        private delegate bool DWallJumpCheck(Player player, int dir);
    }
}
