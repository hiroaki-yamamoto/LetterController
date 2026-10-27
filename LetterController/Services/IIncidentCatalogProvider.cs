using RimWorld;
using Verse;

namespace LetterController.Services
{
    /// <summary>
    /// Provides an abstraction for retrieving available incident definitions in the game.
    /// Decouples incident cataloging from static RimWorld DefDatabase lookups to facilitate automated testing.
    /// </summary>
    /// <example>
    /// <code>
    /// IIncidentCatalogProvider provider = DefDatabaseIncidentCatalogProvider.Instance;
    /// IEnumerable&lt;IncidentDef&gt; defs = provider.GetAllIncidents();
    /// </code>
    /// </example>
    public interface IIncidentCatalogProvider
    {
        /// <summary>
        /// Retrieves all loaded incident definitions in the game environment.
        /// </summary>
        /// <returns>A collection of all loaded <see cref="IncidentDef"/> instances.</returns>
        IEnumerable<IncidentDef> GetAllIncidents();
    }

    /// <summary>
    /// Default catalog provider implementation querying RimWorld's <see cref="DefDatabase{T}"/>.
    /// </summary>
    public sealed class DefDatabaseIncidentCatalogProvider : IIncidentCatalogProvider
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="DefDatabaseIncidentCatalogProvider"/>.
        /// </summary>
        public static readonly DefDatabaseIncidentCatalogProvider Instance = new();

        private DefDatabaseIncidentCatalogProvider()
        {
        }

        /// <summary>
        /// Retrieves all incident definitions from <see cref="DefDatabase{IncidentDef}.AllDefsListForReading"/>.
        /// </summary>
        /// <returns>The list of all loaded incident definitions.</returns>
        public IEnumerable<IncidentDef> GetAllIncidents()
        {
            return DefDatabase<IncidentDef>.AllDefsListForReading;
        }
    }
}
