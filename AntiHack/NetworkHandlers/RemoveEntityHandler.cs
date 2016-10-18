using System.Linq;
using System.Reflection;
using System.Timers;
using AntiHack.Settings;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using Sandbox.Game.Entities;
using SEModAPIExtensions.API;
using SEModAPIInternal.API.Common;
using SEModAPIInternal.API.Server;
using VRage.Library.Collections;
using VRage.Network;

namespace AntiHack.NetworkHandlers
{
    public class RemoveEntityHandler : NetworkHandlerBase
    {
        private readonly Timer _kickTimer = new Timer(30000);
        private bool? _unitTestResult;

        public override bool CanHandle(CallSite site)
        {
            if (!site.MethodInfo.Name.Equals("RemoveEntity_Implementation"))
                return false;

            if (_unitTestResult.HasValue)
                return _unitTestResult.Value;

            ParameterInfo[] parameters = site.MethodInfo.GetParameters();
            if (parameters.Length != 2)
            {
                _unitTestResult = false;
                return false;
            }

            if (parameters[0].ParameterType != typeof(long)
                || parameters[1].ParameterType != typeof(MyTrashRemovalOperation))
            {
                _unitTestResult = false;
                return false;
            }

            _unitTestResult = true;
            return true;
        }

        public override bool Handle(ulong remoteUserId, CallSite site, BitStream stream, object obj)
        {
            if (!PluginSettings.Instance.PluginEnabled)
                return false;

            if (PlayerManager.Instance.IsUserAdmin(remoteUserId) || PluginSettings.Instance.AllowedPlayers.Contains(remoteUserId))
                return false;

            string playername = PlayerMap.Instance.GetFastPlayerNameFromSteamId(remoteUserId);

            AntiHack.Log.Warn($"Player {playername}:{remoteUserId} attempted to remove a grid without admin rights!");

            if (!string.IsNullOrEmpty(PluginSettings.Instance.PulicMessage))
                ChatManager.Instance.SendPublicChatMessage(PluginSettings.Instance.PulicMessage.Replace("%player%", playername));

            if (PluginSettings.Instance.AutoBan)
            {
                _kickTimer.Elapsed += (sender, args) =>
                                      {
                                          MySandboxGame.Static.Invoke(() => MyMultiplayer.Static.BanClient(remoteUserId, true));
                                          AntiHack.Log.Warn($"Banned player {playername}:{remoteUserId}");
                                      };
                _kickTimer.AutoReset = false;
                _kickTimer.Start();
            }
            else if (PluginSettings.Instance.AutoKick)
            {
                _kickTimer.Elapsed += (sender, args) =>
                                      {
                                          MySandboxGame.Static.Invoke(() => MyMultiplayer.Static.KickClient(remoteUserId));
                                          AntiHack.Log.Warn($"Kicked player {playername}:{remoteUserId}");
                                      };
                _kickTimer.AutoReset = false;
                _kickTimer.Start();
            }

            return true;
        }
    }
}