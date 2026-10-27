namespace LetterController.Core
{
    /// <summary>
    /// Represents the desired in-game behavior for a specific letter type.
    /// </summary>
    public enum LetterBehavior
    {
        /// <summary>
        /// Default behavior. Respects the game's native "Pause on Letter" option.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Suppresses the letter completely. It will not be displayed on the letter stack,
        /// and no letter arrival sound will play.
        /// </summary>
        Ignore = 1,

        /// <summary>
        /// Forces the game to pause when the letter arrives, even if its letter type
        /// is not covered by the game's native "Pause on Letter" option.
        /// </summary>
        Pause = 2
    }
}
