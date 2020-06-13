namespace TrafficManager.Patch._VehicleAI {
    using System.Reflection;
    using TrafficManager.Util;
    using UnityEngine;
    using HarmonyLib;
    using System.Collections.Generic;
    using System.Reflection.Emit;
    using CSUtil.Commons;

    public static class StartPathFindCommons {
        // protected override bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays, bool undergroundTarget)        delegate bool TargetDelegate(out uint unit, ref Randomizer randomizer, uint buildIndex,
        delegate bool TargetDelegate(ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays, bool undergroundTarget);

        //protected virtual bool StartPathFind(ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays)
        delegate bool TargetDelegate2(ushort vehicleID, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays);

        public static MethodBase TargetMethod<T>() =>
            TranspilerUtil.DeclaredMethod<TargetDelegate>(typeof(T), "StartPathFind");

        public static MethodBase TargetMethod2<T>() =>
            TranspilerUtil.DeclaredMethod<TargetDelegate2>(typeof(T), "StartPathFind");


        /// <summary>
        /// replaces max pos of 4800 with 8000 to support 81 tiles.
        /// </summary>
        public static IEnumerable<CodeInstruction> ReplaceMaxPosTranspiler(IEnumerable<CodeInstruction> instructions) {
            // TODO [issue #] Should this be done in TMPE?
            // see https://github.com/CitiesSkylinesMods/TMPE/issues/895#issuecomment-643111138
            const float vanilaMaxPos = 4800f; // vanilla 25 tiles compatible value
            const float newMaxPos = 8000f; // 81 tiles compatible value.

            int counter = 0;
            foreach (var instruction in instructions) {
                bool is_ldfld_minCornerOffset =
                    instruction.opcode == OpCodes.Ldc_R4 && ((float)instruction.operand) == vanilaMaxPos;
                if (is_ldfld_minCornerOffset) {
                    counter++;
                    yield return new CodeInstruction(OpCodes.Ldc_R4, operand: newMaxPos);
                } else {
                    yield return instruction;
                }
            }

            // if another mod has already made such replacement then this assertion fails and we would know :)
            Shortcuts.Assert(counter > 0, "counter>0");
            Log._Debug($"StartPathFindCommons.ReplaceMaxPosTranspiler() successfully " +
                $"replaced {counter} instances of ldc.r4 {vanilaMaxPos} with {newMaxPos}");
            yield break;
        }
    }
}