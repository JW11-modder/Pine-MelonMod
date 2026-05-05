#define NOINPUTSYSTEM
using MelonLoader;
using MelonLoader.Preferences;
using System;
using System.Collections;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
#if !NOINPUTSYSTEM
using UnityEngine.InputSystem;
#endif
using UnityEngine.UI;
//using UnityEngine.UIElements;
using static MilestoneManager.Milestone;
using Button = UnityEngine.UI.Button;

namespace JModder
{
    public class JMod : MelonMod
    {
        public const string GUID = "jw11-modder.JMod";
        public const string NAME = "JMod";
        public const string AUTHOR = "jw11-modder";
        public const string VERSION = "1.0.0";
        public static MelonMod Instance { get; private set; }
        public static bool showCheatsPopup = false;

        private static GUIStyle JModStyleT = new GUIStyle();
        private static GUIStyle JModStyleH = new GUIStyle();
        private static GUIStyle JModStyleP = new GUIStyle();
        private static GUIStyle JModStylePV = new GUIStyle();
        private static GUIStyle JModStyleB = new GUIStyle();
        private static GUIStyle JModStyleS = new GUIStyle();
        private static GUIStyle JModStyleST = new GUIStyle();

        private static Color JModColor = new Color(0.0f, 0.85f, 0.85f);

        private static Texture2D consoleBackground = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);

        private static Texture2D sliderBackground = new Texture2D(1, 1, TextureFormat.RGBAFloat, false);

        private static MelonPreferences_Category ModConfCategory;
        private static MelonPreferences_Category MultiplierFloatCategory;
        private static MelonPreferences_Category MultiplierIntCategory;
        private static MelonPreferences_Category ToggleCategory;
        private static List<MelonPreferences_Category> CustomCategoryList = new List<MelonPreferences_Category>();

        public static MelonPreferences_Entry<KeyCode> configMenuToggle;

        private static CursorLockMode lastLockMode;
        private static bool lastVisibleState;

#if !NOINPUTSYSTEM
        public static Key KeycodeToKey(KeyCode keyCode)
        {
            foreach (Key key in System.Enum.GetValues(typeof(Key)))
            {
                if (key.ToString().ToLower() == keyCode.ToString().ToLower())
                {
                    return key;
                }
            }

            LogWarning("can't find Key matching KeyCode " + keyCode.ToString());
            return Key.None;
        }
#endif

        public static void LogHandler(string log, LogType level)
        {
            switch (level)
            {
                case LogType.Log:
                    Instance.LoggerInstance.Msg(log);
                    return;
                case LogType.Warning:
                case LogType.Assert:
                    Instance.LoggerInstance.Warning(log);
                    return;
                case LogType.Exception:
                case LogType.Error:
                    Instance.LoggerInstance.Error(log);
                    return;
            }
        }
        public static void Log(object message)
            => Log(message, LogType.Log);

        public static void LogWarning(object message)
            => Log(message, LogType.Warning);

        public static void LogError(object message)
            => Log(message, LogType.Error);

        internal static void Log(object message, LogType logType)
        {
            string log = message?.ToString() ?? "";

            switch (logType)
            {
                case LogType.Log:
                case LogType.Assert:
                    Instance.LoggerInstance.Msg(log); break;

                case LogType.Warning:
                    Instance.LoggerInstance.Warning(log); break;

                case LogType.Error:
                case LogType.Exception:
                    Instance.LoggerInstance.Error(log); break;
            }
        }

        public static void Init(MelonMod __instance)
        {

            Instance = __instance;

            JModStyleH.alignment = TextAnchor.MiddleCenter;
            JModStyleH.fontSize = 20;
            JModStyleH.fontStyle = FontStyle.Bold;
            JModStyleH.normal.textColor = JModColor;

            JModStyleP.fontSize = 16;
            JModStyleP.normal.textColor = JModColor;

            JModStylePV.fontSize = 16;
            JModStylePV.fontStyle = FontStyle.Bold;
            JModStylePV.normal.textColor = JModColor;

            JModStyleB.alignment = TextAnchor.UpperCenter;
            JModStyleB.fontSize = 24;
            JModStyleB.fontStyle = FontStyle.Bold;
            JModStyleB.normal.textColor = JModColor;
            JModStyleB.normal.background = consoleBackground;

            consoleBackground.SetPixel(0, 0, new Color(0.003f, 0.003f, 0.01f, 0.92f));
            consoleBackground.Apply();

            sliderBackground.SetPixel(0, 0, new Color(0.05f, 0.05f, 0.05f));
            sliderBackground.Apply();

            ModConfCategory = MelonPreferences.CreateCategory("JModConfiguration");
            configMenuToggle = ModConfCategory.CreateEntry("ToggleKey", KeyCode.F7, "Main Menu Toggle Key");
            CustomCategoryList.Clear();
            foreach (var category in MelonPreferences.Categories)
            {
                switch (category.Identifier)
                {
                    case "FloatMultipliers":
                        {
                            MultiplierFloatCategory = category;
                            Log("Float Multipliers loaded!");
                            break;
                        }
                    case "IntMultipliers":
                        {
                            MultiplierIntCategory = category;
                            Log("Int Multipliers loaded!");
                            break;
                        }
                    case "Toggles":
                        {
                            ToggleCategory = category;
                            Log("Toggles loaded!");
                            break;
                        }
                    case "JModConfiguration":
                        {
                            break;
                        }
                    default:
                        {
                            CustomCategoryList.Add(category);
                            Log("Custom category: " + category.DisplayName + " loaded!");
                            break;
                        }

                }
            }

            Log("JMod Init complete!");
            Log("Menu key: " + configMenuToggle.Value.ToString());
        }

        public static bool SwitchMenu()
        {
            if (!showCheatsPopup)
            {
                lastLockMode = UnityEngine.Cursor.lockState;
                lastVisibleState = UnityEngine.Cursor.visible;
                UnityEngine.Cursor.lockState = CursorLockMode.None;
                UnityEngine.Cursor.visible = true;
            }
            else
            {
                UnityEngine.Cursor.lockState = lastLockMode;
                UnityEngine.Cursor.visible = lastVisibleState;
            }
            showCheatsPopup = !showCheatsPopup;
            return showCheatsPopup;
        }

        public static void ShowMenu()
        {
            if (showCheatsPopup)
            {

                JModStyleT = GUI.skin.GetStyle("toggle");
                JModStyleT.fontSize = 16;
                JModStyleT.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
                JModStyleT.onNormal.textColor = JModColor;

                JModStyleS = GUI.skin.GetStyle("horizontalslider");
                JModStyleS.normal.background = sliderBackground;

                JModStyleST = GUI.skin.GetStyle("horizontalsliderthumb");

                GUI.Window(0, new Rect(Screen.width / 2 - 425, Screen.height / 2 - 425, 850, 850), ModMenuWindow, "MOD OPTIONS", JModStyleB);

            }
        }

        private static void ModMenuWindow(int windowId)
        {
            var yAxis = 40;
            var xAxis = 20;
            GUI.Label(new Rect(xAxis, yAxis, 810, 20), "Toggle Mod Options", JModStyleH);
            yAxis += 35;
            ShowBoolMenu(ref xAxis, ref yAxis, ref ToggleCategory);
            yAxis += 10;
            GUI.Label(new Rect(xAxis, yAxis, 810, 20), "Multiplier Mod Options", JModStyleH);
            yAxis += 25;
            ShowFloatMenu(ref xAxis, ref yAxis, ref MultiplierFloatCategory);
            ShowIntMenu(ref xAxis, ref yAxis, ref MultiplierIntCategory);
            yAxis += 15;

            //foreach (MelonPreferences_Category category in CustomCategoryList)
            for (int i = 0; i < CustomCategoryList.Count ; i++)
            {
                var tmpcat = CustomCategoryList[i];
                GUI.Label(new Rect(xAxis, yAxis, 810, 20), tmpcat.DisplayName, JModStyleH);
                yAxis += 35;
                switch (tmpcat.Entries[0].GetType().ToString())
                {
                    case "bool":
                        {
                            ShowBoolMenu(ref xAxis, ref yAxis, ref tmpcat);
                            CustomCategoryList[i] = tmpcat;
                            continue;
                        }
                    case "float":
                        {
                            ShowFloatMenu(ref xAxis, ref yAxis, ref tmpcat);
                            CustomCategoryList[i] = tmpcat;
                            continue;
                        }
                    case "int":
                        {
                            ShowIntMenu(ref xAxis, ref yAxis, ref tmpcat);
                            CustomCategoryList[i] = tmpcat;
                            continue;
                        }
                    default:
                        continue;
                }
            }

            if (GUI.Button(new Rect(325, 810, 200, 35), "Save settings and close"))
            {
                if (Event.current.type == EventType.MouseDown && Event.current.button == 0 || Input.GetMouseButtonDown(0))
                {
                    // This if statement Uses up the current MouseDown event so that
                    // subsequent code or GUI elements ignore this MouseDown event. 
                    Event.current.Use();
                }
                MelonPreferences.Save();
                SwitchMenu();
                Event.current.Use();
            }
        }

        public static void ShowBoolMenu(ref int xAxis, ref int yAxis, ref MelonPreferences_Category cat)
        {
            foreach (MelonPreferences_Entry<bool> toggle in cat.Entries)
            {
                toggle.Value = GUI.Toggle(new Rect(xAxis, yAxis, 400, 20), toggle.Value, toggle.DisplayName, JModStyleT);
                xAxis += 405;
                if (xAxis > 800)
                {
                    xAxis = 20;
                    yAxis += 25;
                }
            }
            if (xAxis != 20)
            {
                xAxis = 20;
                yAxis += 25;
            }
        }
        public static void ShowFloatMenu(ref int xAxis, ref int yAxis, ref MelonPreferences_Category cat)
        {
            foreach (MelonPreferences_Entry<float> mult in cat.Entries)
            {
                string multLabel = mult.DisplayName;
                ValueRange<float> range = (ValueRange<float>)mult.Validator;
                multLabel += " (" + range.MinValue.ToString() + " - " + range.MaxValue.ToString() + ")";
                GUI.Label(new Rect(xAxis, yAxis, 780, 20), multLabel, JModStyleP);
                GUI.Label(new Rect(xAxis + 780, yAxis, 40, 20), mult.Value.ToString("0.0"), JModStylePV);
                yAxis += 25;
                mult.Value = GUI.HorizontalSlider(new Rect(xAxis, yAxis, 810, 20), mult.Value, range.MinValue, range.MaxValue, JModStyleS, JModStyleST);
                yAxis += 15;
            }
        }
        public static void ShowIntMenu(ref int xAxis, ref int yAxis, ref MelonPreferences_Category cat)
        {
            foreach (MelonPreferences_Entry<int> mult in cat.Entries)
            {
                string multLabel = mult.DisplayName;
                ValueRange<int> range = (ValueRange<int>)mult.Validator;
                multLabel += " (" + range.MinValue.ToString() + " - " + range.MaxValue.ToString() + ")";
                GUI.Label(new Rect(xAxis, yAxis, 780, 20), multLabel, JModStyleP);
                GUI.Label(new Rect(xAxis + 780, yAxis, 40, 20), mult.Value.ToString(), JModStylePV);
                yAxis += 25;
                mult.Value = (int)GUI.HorizontalSlider(new Rect(xAxis, yAxis, 810, 20), (float)mult.Value, (float)range.MinValue, (float)range.MaxValue, JModStyleS, JModStyleST);
                yAxis += 15;
            }
        }
    }
}
