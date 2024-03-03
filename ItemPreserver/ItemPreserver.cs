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
        public override Version Version => new Version(1, 0, 2);
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
            //ServerApi.Hooks.NetSendData.Register(this, OnSendData);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeneralHooks.ReloadEvent -= ReloadConfig;
                ServerApi.Hooks.GameUpdate.Deregister(this, new HookHandler<EventArgs>(this.OnUpdate));
               // ServerApi.Hooks.NetSendData.Register(this, OnSendData);
            }
            base.Dispose(disposing);
        }

        private Dictionary<int, DateTime> lastItemUseTime = new Dictionary<int, DateTime>();

        // 增加一个字典，用于记录每个玩家的错误消息发送时间
        private Dictionary<int, DateTime> lastErrorMessageTime = new Dictionary<int, DateTime>();

        private void OnUpdate(EventArgs args)
        {
            foreach (TSPlayer tsplayer in TShock.Players)
            {
                if (!(tsplayer == null))
                {
                    int index = tsplayer.Index;
                    Player tplayer = tsplayer.TPlayer;
                    Item heldItem = tplayer.HeldItem;

                    // 增加一个布尔型变量，用于记录玩家是否已经使用过物品
                    bool hasUsedItem = false;

                    // 检查玩家是否尝试使用物品
                    if (!this.controlUseItemOld[index] && tsplayer.TPlayer.controlUseItem && this.itemUseTime[index] <= 0)
                    {
                        int type = heldItem.type; // 获取物品类型

                        // 检查正在使用的物品是否在不消耗堆叠数量的列表中
                        if (Config.ItemPreserverlist.Contains(type))
                        {
                            {
                                // 检查上次使用物品的时间
                                if (lastItemUseTime.TryGetValue(index, out DateTime lastUseTime))
                                {
                                    // 如果距离上次使用不足1分钟，则不执行操作
                                    if ((DateTime.Now - lastUseTime).TotalSeconds < Config.Cooldownand)
                                    {
                                        // 检查上次发送错误消息的时间
                                        if (!lastErrorMessageTime.TryGetValue(index, out DateTime lastErrorTime) || (DateTime.Now - lastErrorTime).TotalSeconds >= 5)
                                        {
                                            // 如果距离上次发送错误消息超过10秒，则发送错误消息
                                            tsplayer.SendErrorMessage($"请{Config.Cooldownand}分钟后再尝试使用该物品！否则将会消耗！");
                                            // 记录本次发送错误消息的时间
                                            lastErrorMessageTime[index] = DateTime.Now;
                                        }
                                        continue;
                                    }
                                }

                                // 记录本次使用物品的时间
                                lastItemUseTime[index] = DateTime.Now;

                                // 如果玩家还没有使用过物品，则增加玩家背包中选定物品的堆叠数量
                                if (!hasUsedItem)
                                {
                                    tsplayer.TPlayer.inventory[tplayer.selectedItem].stack++;
                                    tsplayer.SendData(PacketTypes.PlayerSlot, "", index, (float)tplayer.selectedItem); // 更新客户端的选定物品槽位
                                    hasUsedItem = true; // 设置玩家已经使用过物品的标志
                                }
                            }
                        }
                    }
                    this.controlUseItemOld[index] = tsplayer.TPlayer.controlUseItem;
                }
            }
        }


        #region
        /*
        private void OnSendData(SendDataEventArgs args)
        {
            foreach (TSPlayer tsplayer in TShock.Players)
            {
                if (!(tsplayer == null))
                {
                    int index = tsplayer.Index;
                    Player tplayer = tsplayer.TPlayer;
                    Item heldItem = tplayer.HeldItem;


                    // 检查玩家是否尝试使用物品
                    if (!this.controlUseItemOld[index] && tsplayer.TPlayer.controlUseItem && this.itemUseTime[index] <= 0)
                    {
                        int type = heldItem.type; // 获取物品类型

                        // 检查正在使用的物品是否在不消耗堆叠数量的列表中
                        if (Config.ItemPreserverlist.Contains(type))
                        {
                            try
                            {
                                int packetType = (int)args.MsgId;

                                // 检查是否需要拦截客户端发送的数据包
                                if (packetType == 5)
                                {
                                    // 如果目标索引是服务器的索引，认为是客户端发送的数据包，进行拦截处理
                                    if (args.remoteClient == -1)
                                    {
                                        args.Handled = true;
                                    }
                                    else
                                    {
                                        // 否则认为是服务器发送的数据包，不进行拦截处理
                                        args.Handled = false;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                TShock.Log.ConsoleError("PacketInterceptor 中发生错误: " + ex.Message);
                            }

                            try
                            {
                                int packetType = (int)args.MsgId;

                                if (packetType == 5)
                                {
                                    // 如果目标索引不是是服务器的索引，认为是服务器端发送的数据包，进行拦截处理
                                    if (!(args.remoteClient == -1))
                                    {
                                        args.Handled = true;
                                    }

                                }
                            }
                            catch (Exception ex)
                            {
                                TShock.Log.ConsoleError("PacketInterceptor 中发生错误: " + ex.Message);
                            }
                        }
                    }
                    this.controlUseItemOld[index] = tsplayer.TPlayer.controlUseItem;
                }
            }
        }*/
        #endregion
    }
}