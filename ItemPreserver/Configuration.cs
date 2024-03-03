using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;

namespace ItemPreserver
{
    public class Configuration
    {
        public static readonly string FilePath = Path.Combine(TShock.SavePath, "ItemPreserverConfig.json");

        public List<BossDrop> BossDrops { get; set; }

        public Configuration()
        {
            BossDrops = new List<BossDrop>();
        }

        private void GenerateDefaultConfig()
        {
            BossDrops = new List<BossDrop>
            {
                new BossDrop { BossID = NPCID.EyeofCthulhu, DropItems = new List<int> { 43 } },
                new BossDrop { BossID = NPCID.KingSlime, DropItems = new List<int> { 560 } },
                new BossDrop { BossID = NPCID.BrainofCthulhu, DropItems = new List<int> { 1331 } },
                new BossDrop { BossID = NPCID.EaterofWorldsHead, DropItems = new List<int> { 70 } },
                new BossDrop { BossID = NPCID.QueenBee, DropItems = new List<int> { 1133 } },
                new BossDrop { BossID = NPCID.SkeletronHead, DropItems = new List<int> { 1307 } },
                new BossDrop { BossID = NPCID.WallofFlesh, DropItems = new List<int> { 267 } },
                new BossDrop { BossID = NPCID.TheDestroyer, DropItems = new List<int> { 556 } },
                new BossDrop { BossID = NPCID.SkeletronPrime, DropItems = new List<int> { 557 } },
                new BossDrop { BossID = NPCID.Retinazer, DropItems = new List<int> { 544 } },
                new BossDrop { BossID = NPCID.QueenSlimeBoss, DropItems = new List<int> { 4988 } },
                new BossDrop { BossID = NPCID.Deerclops, DropItems = new List<int> { 5120 } },
                new BossDrop { BossID = NPCID.DukeFishron, DropItems = new List<int> { 2673 } },
                new BossDrop { BossID = NPCID.MoonLordCore, DropItems = new List<int> { 3601 } },
                new BossDrop { BossID = NPCID.HallowBoss, DropItems = new List<int> { 4961 } }
            };

            Write(FilePath);
        }


        public void Write(string path)
        {
            using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                var str = JsonConvert.SerializeObject(this, Formatting.Indented);
                using (var sw = new StreamWriter(fs))
                {
                    sw.Write(str);
                }
            }
        }

        public static Configuration Read(string path)
        {
            if (!File.Exists(path))
            {
                var config = new Configuration();
                config.GenerateDefaultConfig();
                return config;
            }
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    var cf = JsonConvert.DeserializeObject<Configuration>(sr.ReadToEnd());
                    return cf;
                }
            }
        }

        public class BossDrop
        {
            public int BossID { get; set; }
            public List<int> DropItems { get; set; }
        }
    }
}



