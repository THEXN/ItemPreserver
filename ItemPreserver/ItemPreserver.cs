﻿using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace ItemPreserver
{
    [ApiVersion(2, 1)]
    public class ItemPreserver : TerrariaPlugin
    {
        public override string Author => "肝帝熙恩";
        public override string Description => "指定物品不消耗";
        public override string Name => "ItemPreserver";
        public override Version Version => new Version(1, 0, 4);
        public static Configuration Config;

        public ItemPreserver(Main game) : base(game)
        {
            LoadConfig();
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
            // ServerApi.Hooks.GameUpdate.Register(this, new HookHandler<EventArgs>(this.OnUpdate));
            ServerApi.Hooks.NpcSpawn.Register(this, OnNpcSpawn);
            //ServerApi.Hooks.NetSendData.Register(this, OnSendData);

        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                GeneralHooks.ReloadEvent -= ReloadConfig;
                //    ServerApi.Hooks.GameUpdate.Deregister(this, new HookHandler<EventArgs>(this.OnUpdate));
                // ServerApi.Hooks.NetSendData.Register(this, OnSendData);
                ServerApi.Hooks.NpcSpawn.Register(this, OnNpcSpawn);
            }
            base.Dispose(disposing);
        }

        private void OnNpcSpawn(NpcSpawnEventArgs args)
        {
            foreach (var bossDrop in Config.BossDrops)
            {
                if (Main.npc[args.NpcId].boss && Main.npc[args.NpcId].type == bossDrop.BossID)
                {
                    // 获取所有活跃玩家并检查权限
                    var activePlayers = TShock.Players.Where(p => p != null && p.Active);

                    // 对具有权限的玩家按距离从近到远排序
                    var authorizedPlayers = activePlayers.Where(p => p.HasPermission("itempreserver.receive")).OrderBy(p => Vector2.Distance(p.TPlayer.position, Main.npc[args.NpcId].position));

                    // 如果没有具有权限的玩家，则直接返回
                    if (!authorizedPlayers.Any())
                        return;

                    // 取排序后的第一个玩家
                    var nearestPlayer = authorizedPlayers.First();

                    // 给予最近的玩家掉落物品
                    foreach (var dropItem in bossDrop.DropItems)
                    {
                        nearestPlayer?.GiveItem(dropItem, 1, 0);
                    }
                }
            }
        }

    }
}
