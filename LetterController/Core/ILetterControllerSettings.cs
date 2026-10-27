namespace LetterController.Core
{
    /// <summary>
    /// Defines the contract for managing and querying incident letter behaviors.
    /// Decouples letter settings persistence from UI and patching logic to facilitate unit testing.
    /// </summary>
    /// <example>
    /// <code>
    /// ILetterControllerSettings settings = new LetterControllerSettings();
    /// settings.SetBehavior("RaidEnemy", LetterBehavior.Ignore);
    /// Assert.AreEqual(LetterBehavior.Ignore, settings.GetBehavior("RaidEnemy"));
    /// </code>
    /// </example>
    public interface ILetterControllerSettings
    {
        /// <summary>
        /// Gets the configured <see cref="LetterBehavior"/> for the specified incident defName.
        /// </summary>
        /// <param name="defName">The defName identifier of the incident.</param>
        /// <returns>The configured <see cref="LetterBehavior"/>, or <see cref="LetterBehavior.Default"/> if not configured.</returns>
        LetterBehavior GetBehavior(string? defName);

        /// <summary>
        /// Sets the desired <see cref="LetterBehavior"/> for the specified incident defName.
        /// </summary>
        /// <param name="defName">The defName identifier of the incident.</param>
        /// <param name="behavior">The target behavior.</param>
        void SetBehavior(string? defName, LetterBehavior behavior);

        /// <summary>
        /// Resets all customized incident behaviors back to <see cref="LetterBehavior.Default"/>.
        /// </summary>
        void ResetAll();

        /// <summary>
        /// Gets the total number of customized incident behavior entries.
        /// </summary>
        int CustomCount { get; }
    }
}
