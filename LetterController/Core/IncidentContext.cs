using System;
using RimWorld;

namespace LetterController.Core
{
    /// <summary>
    /// Provides ambient execution context tracking for active <see cref="IncidentDef"/> instances.
    /// Uses thread-static storage to guarantee thread isolation and deterministic context scoping.
    /// </summary>
    public static class IncidentContext
    {
        [ThreadStatic]
        private static IncidentDef? currentIncident;

        /// <summary>
        /// Gets or sets the <see cref="IncidentDef"/> that is currently executing on this thread.
        /// Returns <c>null</c> when code is executing outside of an incident lifecycle.
        /// </summary>
        public static IncidentDef? CurrentIncident
        {
            get => currentIncident;
            set => currentIncident = value;
        }
    }
}
