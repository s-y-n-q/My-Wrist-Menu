using BepInEx;
using System.ComponentModel;

namespace WristMenu.Patches
{
    [Description(WristMenu.PluginInfo.Description)]
    [BepInPlugin(WristMenu.PluginInfo.GUID, WristMenu.PluginInfo.Name, WristMenu.PluginInfo.Version)]
    public class HarmonyPatches : BaseUnityPlugin
    {
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
