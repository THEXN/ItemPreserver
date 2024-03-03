using System;
using System.IO;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using TShockAPI;
using static MonoMod.Cil.RuntimeILReferenceBag.FastDelegateInvokers;

namespace LifemaxExtra
{
    public class Configuration
    {
        public static readonly string FilePath = Path.Combine(TShock.SavePath, "不消耗物品列表.json");
        [JsonProperty("不消耗物品列表")]
        public int[] ItemPreserverlist = { 43, 560, 1331, 70, 5120, 1133, 544, 556, 557, 1293, 3601, 2673, 4988, 4961, 4271, 2767, 7844, 1958, 3828 };
        [JsonProperty("冷却时间(谨慎修改)")]
        public int Cooldownand = 60;

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
                return new Configuration();
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                using (var sr = new StreamReader(fs))
                {
                    var cf = JsonConvert.DeserializeObject<Configuration>(sr.ReadToEnd());
                    return cf;
                }
            }
        }
    }
}
