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
        private const string ModName = "Item Drawers";
        private const string ModVersion = "1.0";
        private const string ModGUID = "some.item.drawers";
        private static Harmony harmony = null!;
        
        public static ItemDrawerPlugin? Instance;
        ConfigSync configSync = new(ModGUID) 
	        { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion};
        
        internal static ConfigEntry<bool> ServerConfigLocked = null!;
        
        private static ConfigEntry<bool> _retreiveEnabled;

        private static ConfigEntry<float> _retreiveRadius;

        private static ConfigEntry<bool> _enabled;

        private static ConfigEntry<int> _maxItems;

        internal static ConfigEntry<KeyCode> _configKeyDepositAll;

        internal static ConfigEntry<KeyCode> _configKeyWithdrawOne;

        internal static ConfigEntry<KeyCode> _configKeyClear;
        public BuildPiece? itemdrawer { get; set; }
        
        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            itemdrawer = new BuildPiece("item_drawer", "piece_drawer");
            itemdrawer.Description.English("Item Drawer to hold your shit");
            itemdrawer.Name.English("Item Drawer");
            itemdrawer.RequiredItems.Add("Wood", 1, true);
            LoadConfig();
            ApplyConfig(itemdrawer.Prefab);
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
		        ApplyConfig(temp);
	        }
        }
        private void LoadConfig()
		{
			ServerConfigLocked = config("1 - General", "Lock Configuration", true, "If on, the configuration is locked and can be changed by server admins only.");

			_enabled = config("General", "Enabled", true, "Enable creation of Item Drawers");
			_maxItems = config("General", "MaxItems", 9999, "The maximum number of items that can be stored in a drawer");
			_retreiveEnabled = config("Item Retreival", "Enabled", true, "Drawers will retreive dropped items matching their item");
			_retreiveRadius = config("Item Retreival", "Radius", 5f, "The distance drawers will check for dropped items");
			_configKeyDepositAll = config("Hotkeys", "Deposit All", KeyCode.LeftShift, "Hold while interacting to deposit all", false);
			_configKeyWithdrawOne = config("Hotkeys", "Withdraw One", KeyCode.LeftAlt, "Hold while interacting to withdraw one", false);
			_configKeyClear = config("Hotkeys", "Clear", KeyCode.LeftAlt, "Hold while interacting to clear contents (only if 0 quantity)", false);
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

		private static void ApplyConfig(GameObject gameObject)
		{
			DrawerContainer container = gameObject.GetComponent<DrawerContainer>();
			container.MaxItems = _maxItems.Value;
			container.RetreiveEnabled = _retreiveEnabled.Value;
			container.RetrieveRadius = (int)_retreiveRadius.Value;
			container._text.text = _maxItems.Value.ToString();
		}
    }
}
