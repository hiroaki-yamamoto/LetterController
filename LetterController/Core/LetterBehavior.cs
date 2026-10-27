namespace LetterController.Core
{
    /// <summary>
    /// Represents the desired in-game behavior for letters triggered by a specific incident.
    /// </summary>
    /// <example>
    /// <code>
    /// var behavior = LetterBehavior.Ignore;
    /// Assert.IsTrue(behavior == LetterBehavior.Ignore);
    /// </code>
    /// </example>
    public enum LetterBehavior
    {
        /// <summary>
        /// Default behavior (0). Respects vanilla game mechanics and the "Pause on Letter" gameplay option.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Suppresses the letter completely (1). It will not be displayed on the letter stack,
        /// and no letter arrival notification sound will play.
        /// </summary>
        Ignore = 1,

        /// <summary>
        /// Forces the game to pause immediately (2) when the incident letter arrives, even if its letter type
        /// is not covered by the game's native "Pause on Letter" option.
        /// </summary>
        Pause = 2
    }
}
