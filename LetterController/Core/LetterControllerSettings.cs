using System;
using System.Collections.Generic;
using Verse;

namespace LetterController.Core
{
    /// <summary>
    /// Stores user configurations for incident letter behavior in RimWorld's mod options.
    /// Manages serialization via RimWorld's Scribe system and provides O(1) behavior lookups per incident.
    /// </summary>
    public class LetterControllerSettings : ModSettings
    {
        /// <summary>
        /// Backing dictionary storing custom letter behaviors keyed by incident defName.
        /// Entries with <see cref="LetterBehavior.Default"/> are removed to optimize space complexity.
        /// </summary>
        private Dictionary<string, LetterBehavior> customBehaviors = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the configured <see cref="LetterBehavior"/> for the specified incident defName.
        /// </summary>
        /// <param name="defName">The defName of the incident.</param>
        /// <returns>The configured behavior, or <see cref="LetterBehavior.Default"/> if not configured.</returns>
        public LetterBehavior GetBehavior(string? defName)
        {
            if (string.IsNullOrEmpty(defName))
            {
                return LetterBehavior.Default;
            }

            return customBehaviors.TryGetValue(defName, out var behavior)
                ? behavior
                : LetterBehavior.Default;
        }

        /// <summary>
        /// Sets the <see cref="LetterBehavior"/> for the specified incident defName.
        /// When setting to <see cref="LetterBehavior.Default"/>, the entry is pruned to conserve memory.
        /// Changes take effect immediately without requiring a game reload.
        /// </summary>
        /// <param name="defName">The defName of the incident.</param>
        /// <param name="behavior">The desired behavior.</param>
        public void SetBehavior(string? defName, LetterBehavior behavior)
        {
            if (string.IsNullOrEmpty(defName))
            {
                return;
            }

            if (behavior == LetterBehavior.Default)
            {
                customBehaviors.Remove(defName);
            }
            else
            {
                customBehaviors[defName] = behavior;
            }
        }

        /// <summary>
        /// Resets all configured incident letter behaviors back to <see cref="LetterBehavior.Default"/>.
        /// </summary>
        public void ResetAll()
        {
            customBehaviors.Clear();
        }

        /// <summary>
        /// Gets the number of customized incident behavior entries.
        /// </summary>
        public int CustomCount => customBehaviors.Count;

        /// <summary>
        /// Saves and loads mod settings data.
        /// </summary>
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref customBehaviors, "customBehaviors", LookMode.Value, LookMode.Value);

            if (Scribe.mode == LoadSaveMode.PostLoadInit && customBehaviors == null)
            {
                customBehaviors = new Dictionary<string, LetterBehavior>(StringComparer.OrdinalIgnoreCase);
            }
        }
    }
}
