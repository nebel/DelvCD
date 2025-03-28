using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using DelvCD.Config;
using System.Diagnostics.CodeAnalysis;

namespace DelvCD.Helpers;

public interface IPluginDisposable {
    void Dispose();
}

[SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")]
public static class Singletons
{
    public static IBuddyList BuddyList = null!;
    public static IClientState ClientState = null!;
    public static ICommandManager CommandManager = null!;
    public static ICondition Condition = null!;
    public static IDalamudPluginInterface DalamudPluginInterface = null!;
    public static IDataManager DataManager = null!;
    public static IFramework Framework = null!;
    public static IGameGui GameGui = null!;
    public static IJobGauges JobGauges = null!;
    public static IObjectTable ObjectTable = null!;
    public static IPartyList PartyList = null!;
    public static ISigScanner SigScanner = null!;
    public static ITargetManager TargetManager = null!;
    public static IUiBuilder UiBuilder = null!;
    public static IPluginLog PluginLog = null!;
    public static ITextureProvider TextureProvider = null!;
    public static ITextureSubstitutionProvider TextureSubstitutionProvider = null!;
    public static INotificationManager NotificationManager = null!;

    public static TexturesCache TexturesCache = null!;
    public static ActionHelpers ActionHelpers = null!;
    public static StatusHelpers StatusHelpers = null!;
    public static ClipRectsHelper ClipRectsHelper = null!;
    public static KeybindHelper KeybindHelper = null!;
    public static DelvCDConfig DelvCDConfig = null!;
    public static FontsManager FontsManager = null!;
    public static PluginManager PluginManager = null!;

    public static void Dispose()
    {
        TexturesCache.Dispose();
        KeybindHelper.Dispose();
        DelvCDConfig.Dispose();
        FontsManager.Dispose();
        PluginManager.Dispose();
    }
}