using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using AntiHack.NetworkHandlers;
using AntiHack.Settings;
using NLog;
using SEModAPIExtensions.API.Plugin;
using SEModAPIInternal.API.Server;

namespace AntiHack
{
    public class AntiHack : IPlugin
    {
        public static Logger Log = LogManager.GetLogger("PluginLog");
        internal static AntiHack Instance;

        public static string PluginPath { get; set; }

        #region Public Properties

        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Anti-Hack")]
        [Description("Enables or disables this plugin")]
        public bool Enabled
        {
            get { return PluginSettings.Instance.PluginEnabled; } 
            set { PluginSettings.Instance.PluginEnabled = value; }
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Anti-Hack")]
        [Description("SteamIDs of players allowed to delete ships and use space master")]
        public ulong[] AllowedPlayers
        {
            get { return PluginSettings.Instance.AllowedPlayers; }
            set { PluginSettings.Instance.AllowedPlayers = value; }
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Anti-Hack")]
        [Description("The server will automatically kick any players it detects deleting grids when they shouldn't")]
        public bool AutoKick
        {
            get { return PluginSettings.Instance.AutoKick; }
            set { PluginSettings.Instance.AutoKick = value; }
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Anti-Hack")]
        [Description("The server will automatically BAN any players it detects deleting grids when they shouldn't")]
        public bool AutoBan
        {
            get { return PluginSettings.Instance.AutoBan; } 
            set {PluginSettings.Instance.AutoBan = value; }
        }

        [Browsable(true)]
        [ReadOnly(false)]
        [Category("Anti-Hack")]
        [Description("This message is broadcast to the entire server when a cheater is detected. '%player%' is replaced with the offending player's name")]
        public string Message
        {
            get { return PluginSettings.Instance.PulicMessage; }
            set { PluginSettings.Instance.PulicMessage = value; }
        }
        #endregion

        public void Init()
        {
            Log.Debug("Initializing AntiHack plugin at path {0}\\", Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location));
            DoInit(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\");
        }

        public void Update()
        {
        }

        public void Shutdown()
        {
        }

        public void InitWithPath(string modPath)
        {
            Log.Debug("Initializing AntiHack plugin at path {0}\\", Path.GetDirectoryName(modPath));
            DoInit(Path.GetDirectoryName(modPath) + "\\");
        }

        private void DoInit(string path)
        {
            Instance = this;
            PluginPath = path;
            PluginSettings.Instance.Load();

            ServerNetworkManager.Instance.RegisterNetworkHandlers(new NetworkHandlerBase[]
                                                                  {
                                                                      new EntityCloseHandler(),
                                                                      new RemoveEntityHandler(),
                                                                      new RemoveTrashHandler(),
                                                                  });

            Log.Info($"Plugin '{Name}' initialized. (Version: {Version}  ID: {Id})");
        }

        #region IPlugin Members

        public Guid Id
        {
            get
            {
                var guidAttr = (GuidAttribute)typeof(AntiHack).Assembly.GetCustomAttributes(typeof(GuidAttribute), true)[0];
                return new Guid(guidAttr.Value);
            }
        }

        public string Name
        {
            get { return "AnitHack Plugin"; }
        }

        public Version Version
        {
            get { return typeof(AntiHack).Assembly.GetName().Version; }
        }

        #endregion
    }
}