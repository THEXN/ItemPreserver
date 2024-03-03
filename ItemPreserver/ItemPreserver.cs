using System.Numerics;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace LifemaxExtra
{
    [ApiVersion(2, 1)]
    public class ItemPreserver : TerrariaPlugin
    {
        public override string Author => "肝帝熙恩";
        public override string Description => "指定物品不消耗";
        public override string Name => "ItemPreserver";
        public override Version Version => new Version(1, 0, 1);
        public static Configuration Config;
        private bool[] controlUseItemOld;
        private int[] itemUseTime;

        public ItemPreserver(Main game) : base(game)
        {
            LoadConfig();
            this.controlUseItemOld = new bool[255];
            this.itemUseTime = new int[255];
        }

        private static void LoadConfig()
        {
            Config = Configuration.Read(Configuration.FilePath);
            Config.Write(Configuration.FilePath);
        }

        private static void ReloadConfig(ReloadEventArgs args)
        {
            LoadConfig();
            args.Player?.SendSuccessMessage("[{0}] 重新加载配置完毕。", typeof(ItemPreserver).Name);
        }

        public override void Initialize()
        {
            GeneralHooks.ReloadEvent += ReloadConfig;
            ServerApi.Hooks.GameUpdate.Register(this, new HookHandler<EventArgs>(this.OnUpdate));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeneralHooks.ReloadEvent -= ReloadConfig;
                ServerApi.Hooks.GameUpdate.Deregister(this, new HookHandler<EventArgs>(this.OnUpdate));
            }
            base.Dispose(disposing);
        }

        private void OnUpdate(EventArgs args)
        {
            foreach (TSPlayer tsplayer in TShock.Players)
            {
                if (!(tsplayer == null))
                {
                    int index = tsplayer.Index;
                    Player tplayer = tsplayer.TPlayer;
                    Item heldItem = tplayer.HeldItem;

                    if (!this.controlUseItemOld[index] && tsplayer.TPlayer.controlUseItem && this.itemUseTime[index] <= 0)
                    {
                        int type = heldItem.type; // 获取物品类型

                        // 检查正在使用的物品是否在不消耗堆叠数量的列表中
                        if (Config.ItemPreserverlist.Contains(type))
                        {
                            // 如果是，则减少玩家背包中选定物品的堆叠数量
                            tsplayer.TPlayer.inventory[tplayer.selectedItem].stack++;
                            tsplayer.SendData(PacketTypes.PlayerSlot, "", index, (float)tplayer.selectedItem); // 更新客户端的选定物品槽位
                        }
                    }
                    this.controlUseItemOld[index] = tsplayer.TPlayer.controlUseItem;
                }
            }
        }
    }
}
