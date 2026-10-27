using System;
using RimWorld;
using Verse;

namespace LetterController.Core
{
    /// <summary>
    /// Represents an immutable metadata item for an incident definition (<see cref="IncidentDef"/>),
    /// encapsulating incident details, originating mod information, and associated letter data.
    /// Optimized for fast search, filtering, and UI display in the settings screen.
    /// </summary>
    public class IncidentItem
    {
        /// <summary>
        /// Gets the unique defName identifier of the incident.
        /// </summary>
        public string DefName { get; }

        /// <summary>
        /// Gets the human-readable display label of the incident.
        /// If the incident definition lacks a label, falls back to <see cref="DefName"/>.
        /// </summary>
        public string Label { get; }

        /// <summary>
        /// Gets the name of the package or mod that added this incident definition.
        /// </summary>
        public string ModName { get; }

        /// <summary>
        /// Gets the underlying <see cref="IncidentDef"/> instance, or <c>null</c> in unit test contexts.
        /// </summary>
        public IncidentDef? Def { get; }

        /// <summary>
        /// Gets the associated <see cref="Verse.LetterDef"/> configured on this incident, if any.
        /// </summary>
        public LetterDef? LetterDef => Def?.letterDef;

        /// <summary>
        /// Initializes a new instance of the <see cref="IncidentItem"/> class from an <see cref="IncidentDef"/>.
        /// </summary>
        /// <param name="def">The incident definition to encapsulate.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="def"/> is null.</exception>
        public IncidentItem(IncidentDef def)
        {
            Def = def ?? throw new ArgumentNullException(nameof(def));
            DefName = def.defName ?? string.Empty;

            if (!string.IsNullOrEmpty(def.label))
            {
                Label = def.LabelCap.ToString();
            }
            else if (!string.IsNullOrEmpty(def.defName))
            {
                Label = def.defName;
            }
            else
            {
                Label = "LetterController_UnknownIncident".Translate().ToString();
            }

            ModName = def.modContentPack?.Name ?? "LetterController_CoreGame".Translate().ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IncidentItem"/> class with explicit properties (primarily used for unit testing).
        /// </summary>
        /// <param name="defName">The unique defName.</param>
        /// <param name="label">The human-readable label.</param>
        /// <param name="modName">The originating mod or package name.</param>
        /// <param name="def">The optional incident definition.</param>
        public IncidentItem(string defName, string label, string modName, IncidentDef? def = null)
        {
            DefName = defName ?? string.Empty;
            Label = label ?? string.Empty;
            ModName = modName ?? string.Empty;
            Def = def;
        }
    }
}
