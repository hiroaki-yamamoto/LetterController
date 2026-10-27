using System;
using HarmonyLib;
using LetterController.Core;
using RimWorld;

namespace LetterController.Patches
{
    /// <summary>
    /// Harmony patch targeting <see cref="IncidentWorker.TryExecute(IncidentParms)"/>
    /// to establish and maintain ambient <see cref="IncidentContext"/> throughout incident execution.
    /// </summary>
    [HarmonyPatch(typeof(IncidentWorker), nameof(IncidentWorker.TryExecute), typeof(IncidentParms))]
    public static class IncidentWorkerPatch
    {
        /// <summary>
        /// Prefix hook saving the outer incident context state and setting the current executing incident.
        /// </summary>
        /// <param name="__instance">The active <see cref="IncidentWorker"/> instance.</param>
        /// <param name="__state">Receives the previous incident context to restore upon completion.</param>
        [HarmonyPrefix]
        public static void Prefix(IncidentWorker __instance, out IncidentDef? __state)
        {
            __state = IncidentContext.CurrentIncident;
            IncidentContext.CurrentIncident = __instance?.def;
        }

        /// <summary>
        /// Finalizer hook guaranteeing the restoration of prior incident context state,
        /// even if an unhandled exception occurs within an incident worker or downstream mod patch.
        /// </summary>
        /// <param name="__exception">The exception thrown by the worker, if any.</param>
        /// <param name="__state">The previous incident context state saved by the prefix.</param>
        /// <returns>The original exception to propagate unmodified.</returns>
        [HarmonyFinalizer]
        public static Exception? Finalizer(Exception? __exception, IncidentDef? __state)
        {
            IncidentContext.CurrentIncident = __state;
            return __exception;
        }
    }
}
