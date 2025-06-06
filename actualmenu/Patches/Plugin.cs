using BepInEx;
using System.ComponentModel;
using UnityEngine;
using WristMenu.Menu;

namespace WristMenu.Patches
{
    [Description(WristMenu.PluginInfo.Description)]
    [BepInPlugin(WristMenu.PluginInfo.GUID, WristMenu.PluginInfo.Name, WristMenu.PluginInfo.Version)]
    public class HarmonyPatches : BaseUnityPlugin
    {
        private void OnGUI()
        {
            Main.ForceMenu = GUILayout.Toggle(Main.ForceMenu, "Force Open Menu");

            if (Main.ForceMenu)
            {
                if (Main.menu == null)
                {
                    Main.CreateMenu();
                    Main.RecenterMenu(false, true);
                    if (Main.reference == null)
                    {
                        Main.CreateReference(false);
                    }
                }
                else
                {
                    Main.RecenterMenu(false, true);
                }
            }
        }
        private void OnEnable()
        {
            Menu.ApplyHarmonyPatches();
        }

        private void OnDisable()
        {
            Menu.RemoveHarmonyPatches();
        }
    }
}
