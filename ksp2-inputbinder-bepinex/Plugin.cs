using BepInEx;
using HarmonyLib;
using KSP.Input;
using KSP.IO;
using KSP.Modding;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Codenade.Inputbinder.BepInEx
{
    [BepInPlugin("codenade-inputbinder", "codenade-inputbinder", "0.3.0")]//PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            var harmony = new Harmony("codenade-inputbinder");
            harmony.PatchAll(typeof(LoadMod));
            harmony.PatchAll(typeof(NoControllerAutoremove));
            //harmony.PatchAll(typeof(NoMouseGlitch));
            harmony.PatchAll(typeof(NoMouseGlitch2));
            enabled = false;
        }
    }

    [HarmonyPatch(typeof(KSP2Mod), nameof(KSP2Mod.Load))]
    public class LoadMod
    {
        static void Postfix(KSP2Mod __instance)
        {
            if (__instance.ModName == "Inputbinder" && __instance.ModAuthor == "Codenade")
            {
                Assembly assembly = Assembly.LoadFrom(__instance.ModRootPath + IOProvider.DirectorySeparatorCharacter.ToString() + "ksp2-inputbinder.dll");
                GameObject o = new GameObject("Codenade.Inputbinder");
                o.AddComponent(assembly.GetType("Codenade.Inputbinder.Inputbinder"));
                o.SetActive(true);
            }
        }
    }

    [HarmonyPatch(typeof(global::Mouse), nameof(global::Mouse.Update))]
    public class NoMouseGlitch2
    {
        static bool Prefix(ref Mouse.ControlScheme ____controlScheme)
        {
            if (UnityEngine.InputSystem.Mouse.current.HasMouseInput())
            {
                ____controlScheme = global::Mouse.ControlScheme.Mouse;
                if (!global::Mouse.IsProcessingEvents)
                {
                    global::Mouse.SetActive(true);
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(global::Mouse), "set_Position")]
    public class NoMouseGlitch
    {
        static bool Prefix(Vector2 value, ref Vector2 ___systemPosition)
        {
            ___systemPosition = value;
            return false;
        }
    }

    [HarmonyPatch(typeof(InputManager), "Awake")]
    public class NoControllerAutoremove
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
