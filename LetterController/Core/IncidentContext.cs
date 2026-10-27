using RimWorld;

namespace LetterController.Core
{
    /// <summary>
    /// Provides ambient execution context tracking for active <see cref="IncidentDef"/> instances.
    /// Uses thread-static storage to guarantee thread isolation and deterministic context scoping.
    /// </summary>
    /// <example>
    /// <code>
    /// var incident = new IncidentDef { defName = "RaidEnemy" };
    /// using (IncidentContext.EnterScope(incident))
    /// {
    ///     Assert.AreEqual("RaidEnemy", IncidentContext.CurrentIncident?.defName);
    /// }
    /// Assert.IsNull(IncidentContext.CurrentIncident);
    /// </code>
    /// </example>
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

        /// <summary>
        /// Enters an ambient incident context scope that automatically restores the previous context upon disposal.
        /// Safe for nested or re-entrant executions.
        /// </summary>
        /// <param name="incident">The incident definition to set as current.</param>
        /// <returns>An <see cref="IDisposable"/> scope token that restores the prior context when disposed.</returns>
        public static IDisposable EnterScope(IncidentDef? incident)
        {
            var previous = currentIncident;
            currentIncident = incident;
            return new ContextScope(previous);
        }

        private readonly struct ContextScope : IDisposable
        {
            private readonly IncidentDef? previous;

            public ContextScope(IncidentDef? previous)
            {
                this.previous = previous;
            }

            public void Dispose()
            {
                currentIncident = previous;
            }
        }
    }
}
