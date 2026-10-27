using LetterController.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LetterController.Tests
{
    /// <summary>
    /// Unit tests verifying the functionality, interface conformance, and complexity optimizations of <see cref="LetterControllerSettings"/>.
    /// </summary>
    [TestClass]
    public class SettingsTests
    {
        [TestMethod]
        public void InterfaceConformance_ImplementsILetterControllerSettings()
        {
            var settings = new LetterControllerSettings();
            Assert.IsInstanceOfType<ILetterControllerSettings>(settings);
        }

        [TestMethod]
        public void GetBehavior_Unconfigured_ReturnsDefault()
        {
            ILetterControllerSettings settings = new LetterControllerSettings();
            var behavior = settings.GetBehavior("RaidEnemy");

            Assert.AreEqual(LetterBehavior.Default, behavior);
            Assert.AreEqual(0, settings.CustomCount);
        }

        [TestMethod]
        public void SetBehavior_Ignore_PersistsAndReportsCustom()
        {
            ILetterControllerSettings settings = new LetterControllerSettings();
            settings.SetBehavior("RaidEnemy", LetterBehavior.Ignore);

            Assert.AreEqual(LetterBehavior.Ignore, settings.GetBehavior("RaidEnemy"));
            Assert.AreEqual(1, settings.CustomCount);
        }

        [TestMethod]
        public void SetBehavior_Pause_PersistsAndReportsCustom()
        {
            ILetterControllerSettings settings = new LetterControllerSettings();
            settings.SetBehavior("TraderCaravanArrival", LetterBehavior.Pause);

            Assert.AreEqual(LetterBehavior.Pause, settings.GetBehavior("TraderCaravanArrival"));
            Assert.AreEqual(1, settings.CustomCount);
        }

        [TestMethod]
        public void SetBehavior_BackToDefault_PrunesEntryToOptimizeSpaceComplexity()
        {
            ILetterControllerSettings settings = new LetterControllerSettings();
            settings.SetBehavior("RaidEnemy", LetterBehavior.Ignore);
            Assert.AreEqual(1, settings.CustomCount);

            // Setting back to Default should prune the dictionary key to minimize space complexity (O(K))
            settings.SetBehavior("RaidEnemy", LetterBehavior.Default);

            Assert.AreEqual(LetterBehavior.Default, settings.GetBehavior("RaidEnemy"));
            Assert.AreEqual(0, settings.CustomCount);
        }

        [TestMethod]
        public void ResetAll_ClearsAllConfiguredBehaviors()
        {
            ILetterControllerSettings settings = new LetterControllerSettings();
            settings.SetBehavior("RaidEnemy", LetterBehavior.Ignore);
            settings.SetBehavior("SolarFlare", LetterBehavior.Pause);
            Assert.AreEqual(2, settings.CustomCount);

            settings.ResetAll();

            Assert.AreEqual(0, settings.CustomCount);
            Assert.AreEqual(LetterBehavior.Default, settings.GetBehavior("RaidEnemy"));
            Assert.AreEqual(LetterBehavior.Default, settings.GetBehavior("SolarFlare"));
        }

        [TestMethod]
        public void GetBehavior_CaseInsensitiveDefName_ResolvesCorrectly()
        {
            ILetterControllerSettings settings = new LetterControllerSettings();
            settings.SetBehavior("raidenemy", LetterBehavior.Ignore);

            Assert.AreEqual(LetterBehavior.Ignore, settings.GetBehavior("RaidEnemy"));
            Assert.AreEqual(LetterBehavior.Ignore, settings.GetBehavior("RAIDENEMY"));
            Assert.AreEqual(LetterBehavior.Ignore, settings.GetBehavior("rAiDeNeMy"));
        }

        [TestMethod]
        public void NullOrEmptyDefName_SafeAndHandledGracefully()
        {
            ILetterControllerSettings settings = new LetterControllerSettings();
            settings.SetBehavior(null, LetterBehavior.Ignore);
            settings.SetBehavior(string.Empty, LetterBehavior.Ignore);

            Assert.AreEqual(0, settings.CustomCount);
            Assert.AreEqual(LetterBehavior.Default, settings.GetBehavior(null));
            Assert.AreEqual(LetterBehavior.Default, settings.GetBehavior(string.Empty));
        }
    }
}
