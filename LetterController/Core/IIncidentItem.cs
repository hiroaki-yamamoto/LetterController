using RimWorld;
using Verse;

namespace LetterController.Core
{
    /// <summary>
    /// Represents an immutable metadata entry for an incident definition (<see cref="IncidentDef"/>).
    /// Used for search, filtering, and UI display in the settings screen.
    /// </summary>
    /// <example>
    /// <code>
    /// IIncidentItem item = new IncidentItem("RaidEnemy", "Enemy Raid", "Core");
    /// Assert.IsTrue(item.MatchesQuery("Raid"));
    /// </code>
    /// </example>
    public interface IIncidentItem
    {
        /// <summary>
        /// Gets the unique defName identifier of the incident.
        /// </summary>
        string DefName { get; }

        /// <summary>
        /// Gets the human-readable display label of the incident.
        /// </summary>
        string Label { get; }

        /// <summary>
        /// Gets the name of the package or mod that added this incident definition.
        /// </summary>
        string ModName { get; }

        /// <summary>
        /// Gets the underlying <see cref="IncidentDef"/> instance, or <c>null</c> in unit test contexts.
        /// </summary>
        IncidentDef? Def { get; }

        /// <summary>
        /// Gets the associated <see cref="Verse.LetterDef"/> configured on this incident, if any.
        /// </summary>
        LetterDef? LetterDef { get; }

        /// <summary>
        /// Evaluates whether this incident matches the specified search query across its Label, DefName, or ModName.
        /// Evaluated with short-circuiting to minimize comparisons.
        /// </summary>
        /// <param name="query">The search query keyword.</param>
        /// <returns><c>true</c> if the query matches; otherwise, <c>false</c>.</returns>
        bool MatchesQuery(string query);
    }
}
