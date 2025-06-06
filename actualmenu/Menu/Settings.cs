using WristMenu.Classes;
using UnityEngine;
using static WristMenu.Menu.Main;

namespace WristMenu
{
    internal class Settings
    {
        public static ExtGradient backgroundColor = new ExtGradient{};
        public static ExtGradient[] buttonColors = new ExtGradient[]
        {
            new ExtGradient{colors = GetSolidGradient(Color.black) }, // Disabled
        };

        public static Color[] textColors = new Color[]
        {
            Color.white, // Disabled
            Color.white // Enabled
        };

        public static Font currentFont = (Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font);

        public static bool fpsCounter = true;
        public static bool pingDisplay = true;
        public static bool disconnectButton = true;
        public static bool rightHanded = false;
        public static bool disableNotifications = false;

        public static KeyCode keyboardButton = KeyCode.Q;

        public static Vector3 menuSize = new Vector3(0.1f, 1f, 1f); // Depth, Width, Height
        public static int buttonsPerPage = 6;
    }
}
