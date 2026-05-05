using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace PineMelonMod
{
    public class LegacyInput
    {

        public LegacyInput()
        {
            p_mousePosition = TInput.GetProperty("mousePosition");
            p_mouseDelta = TInput.GetProperty("mouseScrollDelta");
            m_getKey = TInput.GetMethod("GetKey", new Type[] { typeof(KeyCode) });
            m_getKeyDown = TInput.GetMethod("GetKeyDown", new Type[] { typeof(KeyCode) });
            m_getKeyUp = TInput.GetMethod("GetKeyUp", new Type[] { typeof(KeyCode) });
            m_getMouseButton = TInput.GetMethod("GetMouseButton", new Type[] { typeof(int) });
            m_getMouseButtonDown = TInput.GetMethod("GetMouseButtonDown", new Type[] { typeof(int) });
            m_getMouseButtonUp = TInput.GetMethod("GetMouseButtonUp", new Type[] { typeof(int) });
            m_resetInputAxes = TInput.GetMethod("ResetInputAxes", ArgumentUtility.EmptyTypes);
        }

        public static Type TInput => t_Input ??= AccessTools.TypeByName("UnityEngine.Input");
        private static Type t_Input;

        private static PropertyInfo p_mousePosition;
        private static PropertyInfo p_mouseDelta;
        private static MethodInfo m_getKey;
        private static MethodInfo m_getKeyDown;
        private static MethodInfo m_getKeyUp;
        private static MethodInfo m_getMouseButton;
        private static MethodInfo m_getMouseButtonDown;
        private static MethodInfo m_getMouseButtonUp;
        private static MethodInfo m_resetInputAxes;

        public Vector2 MousePosition => (Vector3)p_mousePosition.GetValue(null, null);
        public Vector2 MouseScrollDelta => (Vector2)p_mouseDelta.GetValue(null, null);

        public bool GetKey(KeyCode key) => (bool)m_getKey.Invoke(null, new object[] { key });
        public bool GetKeyDown(KeyCode key) => (bool)m_getKeyDown.Invoke(null, new object[] { key });
        public bool GetKeyUp(KeyCode key) => (bool)m_getKeyUp.Invoke(null, new object[] { key });

        public bool GetMouseButton(int btn) => (bool)m_getMouseButton.Invoke(null, new object[] { btn });
        public bool GetMouseButtonDown(int btn) => (bool)m_getMouseButtonDown.Invoke(null, new object[] { btn });
        public bool GetMouseButtonUp(int btn) => (bool)m_getMouseButtonUp.Invoke(null, new object[] { btn });

        public void ResetInputAxes() => m_resetInputAxes.Invoke(null, ArgumentUtility.EmptyArgs);
    }
    public static class ArgumentUtility
    {
        /// <summary>
        /// Equivalent to <c>new Type[0]</c>
        /// </summary>
        public static readonly Type[] EmptyTypes = new Type[0];

        /// <summary>
        /// Equivalent to <c>new object[0]</c>
        /// </summary>
        public static readonly object[] EmptyArgs = new object[0];

        /// <summary>
        /// Equivalent to <c>new Type[] { typeof(string) }</c>
        /// </summary>
        public static readonly Type[] ParseArgs = new Type[] { typeof(string) };
    }
}