using System;
using System.IO;
using System.Reflection;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DelvCD.Config;
using DelvCD.Helpers;

namespace DelvCD
{
    public class Plugin : IDalamudPlugin
    {
        public const string ConfigFileName = "DelvCD.json";

        public static string Version { get; private set; } = "1.6.0.1";

        public static string ConfigFileDir { get; private set; } = "";

        public static string ConfigFilePath { get; private set; } = "";

        public static string AssemblyFileDir { get; private set; } = "";

        public static IDalamudTextureWrap? IconTexture { get; private set; } = null;

        public static string Changelog { get; private set; } = string.Empty;

        public string Name => "DelvCD";

        public Plugin(
            IBuddyList buddyList,
            IClientState clientState,
            ICommandManager commandManager,
            ICondition condition,
            IDalamudPluginInterface pluginInterface,
            IDataManager dataManager,
            IFramework framework,
            IGameGui gameGui,
            IJobGauges jobGauges,
            IObjectTable objectTable,
            IPartyList partyList,
            ISigScanner sigScanner,
            ITargetManager targetManager,
            IPluginLog logger,
            ITextureProvider textureProvider,
            ITextureSubstitutionProvider textureSubstitutionProvider,
            INotificationManager notificationManager
        )
        {
            Plugin.Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? Plugin.Version;
            Plugin.ConfigFileDir = pluginInterface.GetPluginConfigDirectory();
            Plugin.ConfigFilePath = Path.Combine(pluginInterface.GetPluginConfigDirectory(), Plugin.ConfigFileName);
            
            if (pluginInterface.AssemblyLocation.DirectoryName != null)
            {
                AssemblyFileDir = pluginInterface.AssemblyLocation.DirectoryName + "\\";
            }
            else
            {
                AssemblyFileDir = Assembly.GetExecutingAssembly().Location;
            }

            ConfigHelpers.CheckVersion();

            // Register Dalamud APIs
            Singletons.BuddyList = buddyList;
            Singletons.ClientState = clientState;
            Singletons.CommandManager = commandManager;
            Singletons.Condition = condition;
            Singletons.DalamudPluginInterface = pluginInterface;
            Singletons.DataManager = dataManager;
            Singletons.Framework = framework;
            Singletons.GameGui = gameGui;
            Singletons.JobGauges = jobGauges;
            Singletons.ObjectTable = objectTable;
            Singletons.PartyList = partyList;
            Singletons.SigScanner = sigScanner;
            Singletons.TargetManager = targetManager;
            Singletons.UiBuilder = pluginInterface.UiBuilder;
            Singletons.PluginLog = logger;
            Singletons.TextureProvider = textureProvider;
            Singletons.TextureSubstitutionProvider = textureSubstitutionProvider;
            Singletons.NotificationManager = notificationManager;
            Singletons.TexturesCache = new TexturesCache();
            Singletons.ActionHelpers = new ActionHelpers();
            Singletons.StatusHelpers = new StatusHelpers();
            Singletons.ClipRectsHelper = new ClipRectsHelper();
            Singletons.KeybindHelper = new KeybindHelper();

            // Load Icon
            Plugin.IconTexture = LoadIconTexture(textureProvider);

            // Load Changelog
            Plugin.Changelog = LoadChangelog();

            // Load config
            DelvCDConfig config = ConfigHelpers.LoadConfig(Plugin.ConfigFilePath);
            Singletons.DelvCDConfig = config;

            // Initialize Fonts
            FontsManager.CopyPluginFontsToUserPath();
            Singletons.FontsManager = new FontsManager(pluginInterface.UiBuilder, config.FontConfig.Fonts.Values);

            // Initialize Text Tags
            TextTagFormatter.InitializeTextTags();

            // Start the plugin
            Singletons.PluginManager = new PluginManager(clientState, commandManager, pluginInterface, config);
            
            // Update Keybind Hints
            Singletons.KeybindHelper.UpdateKeybindHints();
        }

        private static IDalamudTextureWrap? LoadIconTexture(ITextureProvider textureProvider)
        {
            if (string.IsNullOrEmpty(AssemblyFileDir))
            {
                return null;
            }

            string iconPath = Path.Combine(AssemblyFileDir, "Media", "Images", "icon.png");
            if (!File.Exists(iconPath))
            {
                return null;
            }

            IDalamudTextureWrap? texture = null;
            try
            {
                texture = textureProvider.GetFromFile(iconPath).GetWrapOrDefault();                
            }
            catch (Exception ex)
            {
                Singletons.PluginLog.Warning($"Failed to load DelvCD Icon {ex.ToString()}");
            }

            return texture;
        }

        private static string LoadChangelog()
        {
            string? pluginPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (string.IsNullOrEmpty(pluginPath))
            {
                return string.Empty;
            }

            string changelogPath = Path.Combine(pluginPath, "changelog.md");

            if (File.Exists(changelogPath))
            {
                try
                {
                    string changelog = File.ReadAllText(changelogPath);
                    return changelog.Replace("# ", string.Empty);
                }
                catch (Exception ex)
                {
                    Singletons.PluginLog.Warning($"Error loading changelog: {ex.ToString()}");
                }
            }

            return string.Empty;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Plugin.IconTexture is not null)
                {
                    Plugin.IconTexture.Dispose();
                }

                Singletons.Dispose();
            }
        }
    }
}
