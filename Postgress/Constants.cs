namespace Postgress
{
    public static class Constants
    {
        public enum Team
        {
            None = 0,
            Red = 1,
            Green = 2,
            Blue = 3,
        }

        public enum Zoom
        {
            Level0 = 0,
            Level1 = 1,
            Level2 = 2,
            Level3 = 3,
            Level4 = 4
        }

        public enum Commands
        {
            Watch = 'W',
            Deploy = 'D',
            Hack = 'H',
            Recharge = 'R'
        }

        public const int Base = 2;
        public const int TileSize = 16;

        public const string MoveSouth = "MS";
        public const string MoveNorth = "MN";
        public const string MoveWest = "MW";
        public const string MoveEast = "ME";
        public static string[] Moves = { MoveSouth, MoveNorth, MoveWest, MoveEast };

        public const string Token = "36fcc3cf-a638-45ae-9d09-65e610bf84fe"; // JARVIS
        public const string UserAgent = "Mozilla/5.0 (iPad; CPU OS 13_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/87.0.4280.77 Mobile/15E148 Safari/604.1";
        public const string Host = "https://3d.sytes.net";

        public const int MaxResonators = 6;

        public static class Messages
        {
            public const string Success = "Success";
            public const string FullyCharged = "Point is fully repaired";
        }

        public static class Endpoints
        {
            public const string Api = $"{Host}/api";
            public const string InView = $"{Api}/inview";
            public const string Discover = $"{Api}/discover";
            public const string Deploy = $"{Api}/deploy";
            public const string Repair = $"{Api}/repair";
            public const string Attack = $"{Api}/attack2";
            public const string Point = $"{Api}/point";
            public const string Self = $"{Api}/self";
            public const string Inventory = $"{Api}/inventory";
        }

        public static class Inventory
        {
            public static string[] Levels = { "10", "9", "8", "7", "6","5", "4", "3", "2", "1" };
            public static int[] Health = { 6500, 5250, 4000, 3500, 2500, 2000, 1500, 1000, 750, 500 };

            public static class Resonators
            {
                public const string Level1 = "0f6fb146c1b7.67f";
                public const string Level2 = "3933a803fd07.67f";
                public const string Level3 = "650d885ad292.67f";
                public const string Level4 = "a5e96d38b7e2.67f";
                public const string Level5 = "6851bb190a39.67f";
                public const string Level6 = "";
                public const string Level7 = "";
                public const string Level8 = "";
                
                //public static string[] Levels = new[] { Level5, Level4, Level3, Level2, Level1 };
            }

            public static class Bursters
            {
                public const string Level1 = "c35d7d8730aa.67f";
                public const string Level2 = "2cc217ffbe7c.67f";
                public const string Level3 = "8fcc98de3e49.67f";
                public const string Level4 = "c193489c78c9.67f";
                public const string Level5 = "7f3994b47902.67f";
                public const string Level6 = "";
                public const string Level7 = "";
                public const string Level8 = "";
            }

            public static class Keys { }

            public enum Type
            {
                Resonator = 1,
                Burster = 2,
                Link = 3
            }

        }
    }
}
