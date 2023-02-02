namespace Postgress.Entities
{
    using Newtonsoft.Json;

    using System.Collections.Generic;

    using static Postgress.Constants;

    public class PostgressResponse
    {
        [JsonProperty("status")]
        public string Status { get; set; }

        /// <summary>
        ///
        /// e.g.: 401, {"status":"Unauthorized","reason":"Request is unauthorized"}
        /// </summary>
        [JsonProperty("reason")]
        public string? Reason { get; set; }

        /// <summary>
        ///
        /// e.g. 200, {"status":"Success","error":"No such item in the inventory"} , "error": "Cooldown is active. 88 seconds to go"
        /// </summary>
        [JsonProperty("error")]
        public string? Error { get; set; }
    }

    public class DataResponse<T> : PostgressResponse
    {
        [JsonProperty("data")]
        public T? Data { get; set; }
    }

    public class InViewResponse : DataResponse<IEnumerable<Portal>>
    {
    }

    public class Portal
    {
        [JsonProperty("g")]
        public string ID { get; set; }

        [JsonProperty("c")]
        public double[] Coordinates { get; set; }

        [JsonProperty("t")]
        public Team Team { get; set; }

        [JsonProperty("e")]
        public double Energy { get; set; }
    }

    public class PortalResponse : DataResponse<PortalData>
    {

    }

    public class PortalData
    {
        [JsonProperty("g")]
        public string ID { get; set; }

        [JsonProperty("c")]
        public double[] Coordinates { get; set; }

        [JsonProperty("co")]
        public IEnumerable<Resonator> Slots { get; set; }

        [JsonProperty("i")]
        public string I { get; set; } // unknown ??? "As6053Mzc907SBMtQAPSt8p5Mjhvx66Ks08zEyPwaFT756yDzKqcDTvXBXA_O7motIoee3ByjMFF5B2xG-uSEDT-ByVx"

        [JsonProperty("l")]
        public byte Level { get; set; }  // possibly level

        [JsonProperty("o")]
        public string Owner { get; set; } // "n/a" or username 

        [JsonProperty("t")]
        public string Title { get; set; }

        [JsonProperty("te")]
        public Team Team { get; set; } // possibly team? no owner - 0, R-1, G-2, B-3 
    }

    public class Resonator
    {
        [JsonProperty("e")]
        public double Energy { get; set; }

        [JsonProperty("g")]
        public string SlotID { get; set; }

        [JsonProperty("l")]
        public byte Level { get; set; }

        [JsonProperty("o")]
        public string Owner { get; set; }
    }

    public class HackResponse : PostgressResponse
    {
        [JsonProperty("loot")]
        public IEnumerable<Loot>? Loot { get; set; }

        [JsonProperty("xp")]
        public Xp? Xp { get; set; }
    }

    public class RepairResponse : DataResponse<PortalData>
    {
        [JsonProperty("xp")]
        public Xp? Xp { get; set; }
    }

    public class Loot
    {
        [JsonProperty("g")]
        public string ID { get; set; }

        [JsonProperty("t")]
        public byte Type { get; set; } // possibly type of the item? 1-resonator, 2-burster ?

        [JsonProperty("l")]
        public string LevelOrLink { get; set; } // possibly Level of the item

        [JsonProperty("a")]
        public int Amount { get; set; } // possibly amount of items
    }

    public class Xp
    {
        [JsonProperty("cur")]
        public int Current { get; set; }

        [JsonProperty("diff")]
        public int Difference { get; set; }
    }

    public class InventoryResponse : PostgressResponse
    {
        [JsonProperty("i")]
        public List<Inventory> Inventory { get; set; }
    }

    public class Inventory
    {
        [JsonProperty("g")]
        public string ID { get; set; }

        [JsonProperty("a")]
        public int Amount { get; set; }

        // Level if resonator/burster (t=1|2), link if key (t=3)
        [JsonProperty("l")]
        public string LevelOrLink { get; set; }
        
        [JsonProperty("t")]
        public byte Type { get; set; }


        [JsonProperty("ti")]
        public string? Title { get; set; } // key only

        [JsonProperty("c")]
        public double[]? Coordinates { get; set; } // key only
    }

}
