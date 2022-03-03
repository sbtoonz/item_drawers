using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using JetBrains.Annotations;
using PieceManager;
using ServerSync;
using UnityEngine;

namespace ItemDrawers
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class ItemDrawerPlugin : BaseUnityPlugin
    {
	    internal const string ModName = "Item Drawers";
	    internal const string ModVersion = "0.0.1";
        private const string ModGUID = "some.item.drawers";
        private static Harmony harmony = null!;
        
        public static ItemDrawerPlugin? Instance;
        ConfigSync configSync = new(ModGUID) 
	        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion};
        
        internal static ConfigEntry<bool> ServerConfigLocked = null!;
        
        private static ConfigEntry<bool>? _retreiveEnabled;

        private static ConfigEntry<float>? _retreiveRadius;

        private static ConfigEntry<bool>? _enabled;

        private static ConfigEntry<int>? _maxItems;

        internal static ConfigEntry<KeyCode>? _configKeyDepositAll;

        internal static ConfigEntry<KeyCode>? _configKeyWithdrawOne;

        internal static ConfigEntry<KeyCode>? _configKeyClear;

        internal static ConfigEntry<Color>? _enabledColorOpacity;

        internal static ConfigEntry<Color>? _disabledColorOpacity;

        internal static ConfigEntry<bool>? _rotateAtPlayer;
        public BuildPiece? itemdrawer { get; set; }
        public BuildPiece? itemdrawerJude { get; set; }
        
        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            itemdrawer = new BuildPiece("item_drawer", "piece_drawer");
            itemdrawer.Description.English("Item Drawer to hold your things");
            itemdrawer.Name.English("Item Drawer");
            itemdrawer.RequiredItems.Add("FineWood", 10, true);
            itemdrawer.Prefab.gameObject.GetComponent<Piece>().m_enabled = false;
            itemdrawerJude = new BuildPiece("item_drawer", "piece_judeDrawer");
            itemdrawerJude.Name.English("Drawer Stack");
            itemdrawerJude.Description.English("A Stack of drawers for storing things");
            itemdrawerJude.RequiredItems.Add("FineWood", 10,true);
            LoadConfig();
            ApplyConfig(itemdrawer.Prefab);
            ApplyConfig(itemdrawerJude.Prefab);
            harmony = new(ModGUID);
            harmony.PatchAll(assembly);
            
        }
        public void OnDestroy()
        {
            harmony.UnpatchSelf();
        }


        [HarmonyPatch(typeof(Player), "IsOverlapingOtherPiece")]
        private static class OverlapPatch
        {
	        [HarmonyPostfix]
	        [UsedImplicitly]
	        private static void Postfix(string pieceName, ref bool __result)
	        {
		        if (pieceName == "piece_drawer")
		        {
			        __result = false;
		        }
	        } 
        }

        [HarmonyPatch(typeof(ZNetScene), nameof(ZNetScene.Awake))]
        [HarmonyAfter("org.bepinex.helpers.PieceManager")]
        private static class awakerpatch
        {
	        public static void Postfix(ZNetScene __instance)
	        {
		        var temp = __instance.GetPrefab("piece_drawer");
		        var temp2 = __instance.GetPrefab("piece_judeDrawer");
		        ApplyConfig(temp);
		        ApplyConfig(temp2);
	        }
        }
        private void LoadConfig()
		{
			ServerConfigLocked = config("1 - General", "Lock Configuration", true, new ConfigDescription("If on, the configuration is locked and can be changed by server admins only."));

			_enabled = config("1 - General", "Enabled", true, "Enable creation of Item Drawers");
			_maxItems = config("General", "MaxItems", 9999, new ConfigDescription("The maximum number of items that can be stored in a drawer",new AcceptableValueRange<int>(0, 9999)));
			_retreiveEnabled = config("Item Retreival", "Enabled", true, "Drawers will retrieve dropped items matching their item");
			_retreiveRadius = config("Item Retreival", "Radius", 5f, new ConfigDescription("The distance drawers will check for dropped items", new AcceptableValueRange<float>(0, 30f), new ConfigurationManagerAttributes{Advanced = true, Browsable = true}));
			_configKeyDepositAll = config("Hotkeys", "Deposit All", KeyCode.LeftShift, "Hold while interacting to deposit all", false);
			_configKeyWithdrawOne = config("Hotkeys", "Withdraw One", KeyCode.LeftAlt, "Hold while interacting to withdraw one", false);
			_configKeyClear = config("Hotkeys", "Clear", KeyCode.LeftAlt, "Hold while interacting to clear contents (only if 0 quantity)", false);
			_enabledColorOpacity = config("1 - General", "Icon Opacity Enabled", Color.white,
				new ConfigDescription("This is the default opacity for the icon when it is enabled"));
			_disabledColorOpacity = config("1 - General", "Icon Opacity Disabled", Color.clear,
				new ConfigDescription("This is the default opacity for the icon when it is disabled"));
			_rotateAtPlayer = config("1 - General", "Should Icon on alt drawers rotate", true,
				"When set to true the icons on alt drawers will rotate towards the camera");
			configSync.AddLockingConfigEntry(ServerConfigLocked);
			
		}
        ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description, bool synchronizedSetting = true)
        {
	        ConfigEntry<T> configEntry = Config.Bind(group, name, value, description);

	        SyncedConfigEntry<T> syncedConfigEntry = configSync.AddConfigEntry(configEntry);
	        syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

	        return configEntry;
        }

        ConfigEntry<T> config<T>(string group, string name, T value, string description, bool synchronizedSetting = true) => config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        private class ConfigurationManagerAttributes
        {
	        public bool? Browsable = false;
	        public bool? Advanced = false;
        }
		private static void ApplyConfig(GameObject gameObject)
		{
			DrawerContainer container = gameObject.GetComponent<DrawerContainer>();
			container.MaxItems = _maxItems!.Value;
			container.RetreiveEnabled = _retreiveEnabled!.Value;
			container.RetrieveRadius = (int)_retreiveRadius!.Value;
			container._text.text = _maxItems.Value.ToString();
		}
    }
}
