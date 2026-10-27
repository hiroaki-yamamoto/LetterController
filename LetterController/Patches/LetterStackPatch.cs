using HarmonyLib;
using LetterController.Core;
using Verse;

namespace LetterController.Patches
{
    /// <summary>
    /// Harmony patch targeting <see cref="Verse.LetterStack.ReceiveLetter(Letter, string, int, bool)"/>
    /// to apply custom user behaviors (Ignore or Pause) to letters triggered by in-game incidents.
    /// </summary>
    [HarmonyPatch(typeof(LetterStack), nameof(LetterStack.ReceiveLetter), typeof(Letter), typeof(string), typeof(int), typeof(bool))]
    public static class LetterStackPatch
    {
        /// <summary>
        /// Prefix hook executed before a letter is processed.
        /// If the letter originated from an active incident with <see cref="LetterBehavior.Ignore"/>,
        /// the original method execution is skipped entirely, suppressing letter display and notification sound.
        /// </summary>
        /// <param name="let">The arriving letter.</param>
        /// <returns><c>false</c> to suppress the letter; <c>true</c> to continue with standard processing.</returns>
        [HarmonyPrefix]
        public static bool Prefix(Letter let)
        {
            var incident = IncidentContext.CurrentIncident;
            if (incident == null)
            {
                // Letter was not triggered by an active incident; preserve standard game behavior
                return true;
            }

            var settings = LetterControllerMod.Settings;
            if (settings == null)
            {
                return true;
            }

            var behavior = settings.GetBehavior(incident.defName);
            if (behavior == LetterBehavior.Ignore)
            {
                // Ignore: Suppress letter display and arrival sound completely
                return false;
            }

            return true;
        }

        /// <summary>
        /// Postfix hook executed after a letter is processed.
        /// If the letter originated from an active incident with <see cref="LetterBehavior.Pause"/>,
        /// and the letter was not delayed or dismissed, the game is paused immediately.
        /// </summary>
        /// <param name="let">The arriving letter.</param>
        /// <param name="delayTicks">The delay in ticks before the letter is added.</param>
        [HarmonyPostfix]
        public static void Postfix(Letter let, int delayTicks)
        {
            // If the letter is delayed into the queue, wait until it actually arrives at delayTicks <= 0
            if (delayTicks > 0 || let?.def == null)
            {
                return;
            }

            // If the letter cannot show in stack, it was dismissed or invalid
            if (!let.CanShowInLetterStack)
            {
                return;
            }

            var incident = IncidentContext.CurrentIncident;
            if (incident == null)
            {
                return;
            }

            var settings = LetterControllerMod.Settings;
            if (settings == null)
            {
                return;
            }

            var behavior = settings.GetBehavior(incident.defName);
            if (behavior == LetterBehavior.Pause)
            {
                // Pause: Force pause the game even if not covered by the native "Pause on Letter" option
                Find.TickManager?.Pause();
            }
        }
    }
}
