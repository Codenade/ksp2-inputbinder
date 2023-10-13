using KSP.Input;
using KSP.Modding;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine;
using BepInEx;
using HarmonyLib;
using System.Reflection.Emit;
using KSP.UI;

namespace Codenade.Inputbinder
{
    [BepInPlugin(id, displayName, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string id = "codenade-inputbinder";
        public const string displayName = "Inputbinder";
        public static System.Version version = System.Version.Parse(MyPluginInfo.PLUGIN_VERSION);

        private void Awake()
        {
            var harmony = new Harmony(id);
            harmony.PatchAll(typeof(PatchLoadMod));
            harmony.PatchAll(typeof(PatchNoControllerAutoremove));
            harmony.PatchAll(typeof(PatchNoMouseGlitch));
            harmony.PatchAll(typeof(PatchSettingsMenuManager));
            enabled = false;
        }
    }

    [HarmonyPatch(typeof(KSP2ModManager), nameof(KSP2ModManager.LoadAllMods))]
    internal static class PatchLoadMod
    {
        static void Postfix()
        {
            GameObject o = new GameObject("Codenade.Inputbinder");
            o.AddComponent<Inputbinder>();
            o.SetActive(true);
        }
    }

    [HarmonyPatch(typeof(Mouse), nameof(Mouse.Update))]
    internal static class PatchNoMouseGlitch
    {
        static bool Prefix(ref Mouse.ControlScheme ____controlScheme)
        {
            if (UnityEngine.InputSystem.Mouse.current.HasMouseInput())
            {
                ____controlScheme = Mouse.ControlScheme.Mouse;
                if (!Mouse.IsProcessingEvents)
                {
                    Mouse.SetActive(true);
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(SettingsMenuManager), "ShowInputSettings")]
    internal static class PatchSettingsMenuManager
    {
        static bool Prefix(SettingsMenuManager __instance)
        {
            typeof(SettingsMenuManager).GetMethod("ToggleMenu", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(__instance, new object[] { Inputbinder.Instance.BindingUI });
            typeof(SettingsMenuManager).GetMethod("UpdateResetButton", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).Invoke(__instance, new object[] { "Input" });
            return false;
        }
    }

    [HarmonyPatch(typeof(InputManager), "Awake")]
    internal static class PatchNoControllerAutoremove
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var log = global::BepInEx.Logging.Logger.CreateLogSource("codenade-inputbinder");
            var sequenceFound = false;
            var startIndex = -1;

            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Ret)
                    break;
                if (startIndex != -1)
                {
                    if (codes[i].opcode != seq[i - startIndex])
                        startIndex = -1;
                    else if (i - startIndex + 1 == seq.Length)
                    {
                        sequenceFound = true;
                        break;
                    }
                }
                else if (codes[i].opcode == seq[0])
                    startIndex = i;
            }
            if (sequenceFound)
            {
                codes.RemoveRange(startIndex, seq.Length);
            }
            else
                log.LogError("Could not remove KSP's Gamepad removal code");
            return codes.AsEnumerable();
        }

        private static readonly OpCode[] seq =
        {
            OpCodes.Ldloca_S,
            OpCodes.Call,
            OpCodes.Stloc_1,
            OpCodes.Ldc_I4_0,
            OpCodes.Stloc_2,
            OpCodes.Br,
            OpCodes.Ldloc_1,
            OpCodes.Ldloc_2,
            OpCodes.Ldelem_Ref,
            OpCodes.Call,
            OpCodes.Ldloc_2,
            OpCodes.Ldc_I4_1,
            OpCodes.Add,
            OpCodes.Stloc_2,
            OpCodes.Ldloc_2,
            OpCodes.Ldloc_1,
            OpCodes.Ldlen,
            OpCodes.Conv_I4,
            OpCodes.Blt,
            OpCodes.Ldarg_0,
            OpCodes.Ldftn,
            OpCodes.Newobj,
            OpCodes.Call
        };
    }
}
