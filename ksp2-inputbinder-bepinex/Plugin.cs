using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using KSP.Input;
using KSP.IO;
using KSP.Modding;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace Codenade.Inputbinder.BepInEx
{
    [BepInPlugin("codenade-inputbinder", "codenade-inputbinder", "0.1.1")]//PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            // Plugin startup logic
            Logger.LogInfo($"Plugin codenade-inputbinder-bepinex is loaded!");
            var harmony = new Harmony("codenade-inputbinder");
            harmony.PatchAll(typeof(KSP2Mod_Load_Postfix));
            harmony.PatchAll(typeof(InputManager_Awake_Transpiler));
            enabled = false;
        }
    }

    [HarmonyPatch(typeof(KSP2Mod), nameof(KSP2Mod.Load))]
    public class KSP2Mod_Load_Postfix
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

    [HarmonyPatch(typeof(InputManager), "Awake")]
    public class InputManager_Awake_Transpiler
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var log = new ManualLogSource("codenade-inputbinder");
            log.LogDebug("Starting Transpiler");
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
                log.LogDebug("Sequence found at " + startIndex);
                codes.RemoveRange(startIndex, seq.Length);
            }
            else
                log.LogError("Could not remove KSP's Gamepad removal code");
            return codes.AsEnumerable();
        }

        public static readonly OpCode[] seq =
        {
            OpCodes.Ldloca_S,
            OpCodes.Call,
            OpCodes.Stloc_1,
            OpCodes.Ldc_I4_0,
            OpCodes.Stloc_2,
            OpCodes.Br_S,
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
            OpCodes.Blt_S,
            OpCodes.Ldarg_0,
            OpCodes.Ldftn,
            OpCodes.Newobj,
            OpCodes.Call
        };
    }
}
