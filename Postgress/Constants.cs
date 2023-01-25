﻿namespace Postgress
{
    public static class Constants
    {
        public const string UserName = "JARVIS";
        public const short UserTeam = 2;
        public const int CoolDownSec = 90;
        public const string Token = "36fcc3cf-a638-45ae-9d09-65e610bf84fe";
        public const string UserAgent = "Mozilla/5.0 (iPad; CPU OS 13_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) CriOS/87.0.4280.77 Mobile/15E148 Safari/604.1";
        public const string Host = "https://3d.sytes.net:8080";


        public static class Messages
        {
            public const string Success = "Success";
        }

        public static class Endpoints
        {
            public const string Api = $"{Host}/api";
            public const string InView = $"{Api}/inview";
            public const string Discover = $"{Api}/discover";
        }

        public static class Inventory
        {
            public static class Resonators
            {
                public const string Level1 = "0f6fb146c1b7.67f";
                public const string Level2 = "3933a803fd07.67f";
                public const string Level3 = "650d885ad292.67f";
                public const string Level4 = "d606dc9676be.67f";
                public const string Level5 = "6851bb190a39.67f";
                public const string Level6 = "";
                public const string Level7 = "";
                public const string Level8 = "";
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
            
        }
    }
}