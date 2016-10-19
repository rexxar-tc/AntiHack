using System.Linq;
using System.Reflection;
using System.Timers;
using AntiHack.Settings;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using Sandbox.ModAPI;
using SEModAPIExtensions.API;
using SEModAPIInternal.API.Common;
using SEModAPIInternal.API.Server;
using VRage.Library.Collections;
using VRage.Network;

namespace AntiHack.NetworkHandlers
{
    public class EntityCloseHandler : NetworkHandlerBase
    {
        private static bool? _unitTestResult;

        private readonly Timer _kickTimer = new Timer(10000);

        public override bool CanHandle(CallSite site)
        {
            if (site.MethodInfo.Name != "OnEntityClosedRequest")
                return false;

            if (_unitTestResult == null)
            {
                //static void OnEntityClosedRequest(long entityId)
                ParameterInfo[] parameters = site.MethodInfo.GetParameters();
                if (parameters.Length != 1)
                {
                    _unitTestResult = false;
                    return false;
                }

                if (parameters[0].ParameterType != typeof(long))
                {
                    _unitTestResult = false;
                    return false;
                }

                _unitTestResult = true;
            }

            return _unitTestResult.Value;
        }

        public override bool Handle(ulong remoteUserId, CallSite site, BitStream stream, object obj)
        {
            if (!PluginSettings.Instance.PluginEnabled)
                return false;

            if (PlayerManager.Instance.IsUserAdmin(remoteUserId) || PlayerManager.Instance.IsUserPromoted(remoteUserId) || PluginSettings.Instance.AllowedPlayers.Contains(remoteUserId))
                return false;
            
            string playername = PlayerMap.Instance.GetFastPlayerNameFromSteamId(remoteUserId);

            AntiHack.Log.Warn($"Player {playername}:{remoteUserId} attempted to delete a grid without admin rights!");

            if (!string.IsNullOrEmpty(PluginSettings.Instance.PulicMessage))
                MySandboxGame.Static.Invoke(() => MyAPIGateway.Utilities.SendMessage(PluginSettings.Instance.PulicMessage.Replace("%player%", playername)));
                //ChatManager.Instance.SendPublicChatMessage(PluginSettings.Instance.PulicMessage.Replace("%player%", playername));

            if (PluginSettings.Instance.AutoBan)
            {
                _kickTimer.Elapsed += (sender, args) =>
                                      {
                                          MyMultiplayer.Static.BanClient(remoteUserId, true);
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