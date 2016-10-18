using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace AntiHack.Settings
{
    [Serializable]
    public class PluginSettings
    {
        #region Constructor

        public PluginSettings()
        {
            _allowedPlayers = new ulong[] {};
            _publicMessage = string.Empty;
            _enabled = true;
        }

        #endregion

        #region Static Properties

        public static PluginSettings Instance
        {
            get { return _instance ?? (_instance = new PluginSettings()); }
        }

        #endregion

        #region Private Fields

        private static PluginSettings _instance;
        private static bool _loading;

        private bool _enabled;

        private ulong[] _allowedPlayers;
        private bool _autoBan;
        private bool _autoKick;
        private string _publicMessage;

        #endregion

        #region Properties

        public bool PluginEnabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                Save();
            }
        }

        public ulong[] AllowedPlayers
        {
            get { return _allowedPlayers; }
            set
            {
                _allowedPlayers = value;
                Save();
            }
        }

        public bool AutoBan
        {
            get { return _autoBan; }
            set
            {
                _autoBan = value;
                Save();
            }
        }

        public bool AutoKick
        {
            get { return _autoKick; }
            set
            {
                _autoKick = value;
                Save();
            }
        }

        public string PulicMessage
        {
            get { return _publicMessage; }
            set
            {
                _publicMessage = value;
                Save();
            }
        }

        #endregion

        #region Loading and Saving

        /// <summary>
        ///     Loads our settings
        /// </summary>
        public void Load()
        {
            _loading = true;

            try
            {
                lock (this)
                {
                    string fileName = AntiHack.PluginPath + "AntiHack-Settings.xml";
                    if (File.Exists(fileName))
                    {
                        using (var reader = new StreamReader(fileName))
                        {
                            var x = new XmlSerializer(typeof(PluginSettings));
                            var settings = (PluginSettings)x.Deserialize(reader);
                            reader.Close();

                            _instance = settings;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AntiHack.Log.Error(ex);
            }
            finally
            {
                _loading = false;
            }
        }

        /// <summary>
        ///     Saves our settings
        /// </summary>
        public void Save()
        {
            if (_loading)
                return;

            try
            {
                lock (this)
                {
                    string fileName = AntiHack.PluginPath + "AntiHack-Settings.xml";
                    using (var writer = new StreamWriter(fileName))
                    {
                        var x = new XmlSerializer(typeof(PluginSettings));
                        x.Serialize(writer, _instance);
                        writer.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                AntiHack.Log.Error(ex);
            }
        }

        #endregion

        #region Events

        /// <summary>
        ///     Triggered when items changes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Save();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Console.WriteLine("PropertyChanged()");
            Save();
        }

        #endregion
    }
}