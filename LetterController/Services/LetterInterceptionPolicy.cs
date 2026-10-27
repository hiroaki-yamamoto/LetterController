using LetterController.Core;
using RimWorld;
using Verse;

namespace LetterController.Services
{
    /// <summary>
    /// Abstraction for controlling the game playback speed and pause state.
    /// Decouples <see cref="Find.TickManager"/> to enable deterministic unit testing without a live game instance.
    /// </summary>
    public interface IGameSpeedController
    {
        /// <summary>
        /// Pauses the game immediately.
        /// </summary>
        void Pause();
    }

    /// <summary>
    /// Default game speed controller invoking RimWorld's native <see cref="TickManager.Pause"/>.
    /// </summary>
    public sealed class RimWorldGameSpeedController : IGameSpeedController
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="RimWorldGameSpeedController"/>.
        /// </summary>
        public static readonly RimWorldGameSpeedController Instance = new();

        private RimWorldGameSpeedController()
        {
        }

        /// <summary>
        /// Pauses the game using RimWorld's active <see cref="TickManager"/>.
        /// </summary>
        public void Pause()
        {
            Find.TickManager?.Pause();
        }
    }

    /// <summary>
    /// Pure policy service determining letter interception, suppression, and pausing behaviors.
    /// Fully decoupled from Harmony patches and static game singletons for complete testability.
    /// </summary>
    /// <example>
    /// <code>
    /// var incident = new IncidentDef { defName = "RaidEnemy" };
    /// var settings = new LetterControllerSettings();
    /// settings.SetBehavior("RaidEnemy", LetterBehavior.Ignore);
    /// bool suppress = LetterInterceptionPolicy.ShouldSuppress(incident, settings);
    /// Assert.IsTrue(suppress);
    /// </code>
    /// </example>
    public static class LetterInterceptionPolicy
    {
        /// <summary>
        /// Determines whether an arriving letter should be suppressed (ignored) based on its originating incident and user configuration.
        /// Time complexity: O(1) hash lookup.
        /// </summary>
        /// <param name="incident">The active incident that generated the letter, or <c>null</c> if none.</param>
        /// <param name="settings">The configured mod settings instance, or <c>null</c>.</param>
        /// <returns><c>true</c> if the letter should be suppressed; otherwise, <c>false</c>.</returns>
        public static bool ShouldSuppress(IncidentDef? incident, ILetterControllerSettings? settings)
        {
            if (incident == null || string.IsNullOrEmpty(incident.defName) || settings == null)
            {
                return false;
            }

            return settings.GetBehavior(incident.defName) == LetterBehavior.Ignore;
        }

        /// <summary>
        /// Determines whether the game should pause upon letter arrival based on its originating incident and user configuration.
        /// Respects delayed ticks and letter visibility rules.
        /// Time complexity: O(1) hash lookup.
        /// </summary>
        /// <param name="let">The arriving letter.</param>
        /// <param name="delayTicks">The delay in ticks before the letter is added.</param>
        /// <param name="incident">The active incident that generated the letter, or <c>null</c> if none.</param>
        /// <param name="settings">The configured mod settings instance, or <c>null</c>.</param>
        /// <returns><c>true</c> if the game should be paused; otherwise, <c>false</c>.</returns>
        public static bool ShouldPause(Letter? let, int delayTicks, IncidentDef? incident, ILetterControllerSettings? settings)
        {
            // If the letter is queued with delay ticks, defer pause until actual presentation (delayTicks <= 0)
            if (delayTicks > 0 || let?.def == null)
            {
                return false;
            }

            // Letters dismissed or ineligible for stack display should not pause
            if (!let.CanShowInLetterStack)
            {
                return false;
            }

            if (incident == null || string.IsNullOrEmpty(incident.defName) || settings == null)
            {
                return false;
            }

            return settings.GetBehavior(incident.defName) == LetterBehavior.Pause;
        }

        /// <summary>
        /// Executes postfix pause logic if conditions are met.
        /// </summary>
        /// <param name="let">The arriving letter.</param>
        /// <param name="delayTicks">The delay in ticks.</param>
        /// <param name="incident">The active incident, or <c>null</c>.</param>
        /// <param name="settings">The configured mod settings, or <c>null</c>.</param>
        /// <param name="speedController">The speed controller to execute the pause action (defaults to <see cref="RimWorldGameSpeedController.Instance"/>).</param>
        public static void HandlePostfix(Letter? let, int delayTicks, IncidentDef? incident, ILetterControllerSettings? settings, IGameSpeedController? speedController = null)
        {
            if (ShouldPause(let, delayTicks, incident, settings))
            {
                var controller = speedController ?? RimWorldGameSpeedController.Instance;
                controller.Pause();
            }
        }
    }
}
