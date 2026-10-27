using HarmonyLib;
using LetterController.UI;
using UnityEngine;
using Verse;

namespace LetterController.Core
{
    /// <summary>
    /// The entry point Mod class for Letter Controller.
    /// Manages the mod lifecycle, initializes Harmony patches, and hosts the mod settings window.
    /// </summary>
    /// <example>
    /// <code>
    /// // Mod lifecycle entry point managed by RimWorld mod loader.
    /// </code>
    /// </example>
    public class LetterControllerMod : Mod
    {
        /// <summary>
        /// Gets the singleton settings instance for the Letter Controller mod.
        /// </summary>
        public static LetterControllerSettings Settings { get; private set; } = null!;

        private readonly LetterControllerSettingsUi settingsUi = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="LetterControllerMod"/> class.
        /// Loads user settings and applies Harmony patches.
        /// </summary>
        /// <param name="content">The mod content pack.</param>
        public LetterControllerMod(ModContentPack content) : base(content)
        {
            Settings = GetSettings<LetterControllerSettings>();

            var harmony = new Harmony("rimworld.mod.lettercontroller");
            harmony.PatchAll();
        }

        /// <summary>
        /// Sets the mod settings instance directly (primarily used for test harness initialization).
        /// </summary>
        /// <param name="settings">The settings instance.</param>
        public static void SetSettingsForTesting(LetterControllerSettings? settings)
        {
            Settings = settings!;
        }

        /// <summary>
        /// Gets the category name displayed in the RimWorld Mod Options menu.
        /// Accessible from Options > Mod Options on the main menu and during active gameplay.
        /// </summary>
        /// <returns>The localized category title.</returns>
        public override string SettingsCategory()
        {
            return "LetterController_Category".Translate();
        }

        /// <summary>
        /// Draws the mod settings window contents.
        /// </summary>
        /// <param name="inRect">The available client rectangle in the mod options dialog.</param>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            settingsUi.Draw(inRect, Settings);
        }
    }
}
