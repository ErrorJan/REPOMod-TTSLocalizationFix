using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace TTSLocalizationFix;

[BepInPlugin( PluginGUID, PluginName, PluginVersion )]
public class Plugin : BaseUnityPlugin
{
    internal const string PluginGUID =  "ErrorJan.REPO.TTSLocalizationFix";
    internal const string PluginName = "TTS Localization Fix";
    internal const string PluginVersion = "1.0.0"; 

    private void Awake()
    {
        Harmony harmony = new( PluginGUID );
        harmony.PatchAll( typeof( HarmonyPatches ) );

        Logger.LogInfo($"Plugin { PluginName } is loaded!");
    }

    private class HarmonyPatches
    {
        [HarmonyTranspiler]
        [HarmonyPatch( typeof( TTSVoice ), "TranslateSpecialLetters" )]
        private static IEnumerable<CodeInstruction> Transpiler( IEnumerable<CodeInstruction> instructions )
        {
            var stringReplaceMethod = AccessTools.Method( typeof( string ), "Replace", new System.Type[] { typeof( string ), typeof( string ) } );
            var thisMethod = AccessTools.Method( typeof( TTSVoice ), "TranslateSpecialLetters" );

            bool check = false;

            var num = thisMethod.GetParameters().First( ( p ) =>  p.ParameterType == typeof( string ) );

            foreach ( var instruction in instructions )
            {
                if ( check && instruction.opcode == OpCodes.Pop )
                {
                    yield return new CodeInstruction( OpCodes.Starg_S, num.Position + 1 );
                    check = false;
                    continue;
                }
                check = false;
                if ( instruction.Calls( stringReplaceMethod ) )
                {
                    check = true;
                }

                yield return instruction;
            }
        }
    }
}
