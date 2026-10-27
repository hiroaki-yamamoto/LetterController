using LetterController.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LetterController.Tests
{
    /// <summary>
    /// Unit tests verifying the functionality and complexity optimizations of <see cref="LetterControllerSettings"/>.
    /// </summary>
    [TestClass]
    public class SettingsTests
    {
        [TestMethod]
        public void GetBehavior_Unconfigured_ReturnsDefault()
        {
            var settings = new LetterControllerSettings();
            var behavior = settings.GetBehavior("RaidEnemy");

            Assert.AreEqual(LetterBehavior.Default, behavior);
            Assert.AreEqual(0, settings.CustomCount);
        }

        [TestMethod]
        public void SetBehavior_Ignore_PersistsAndReportsCustom()
        {
            var settings = new LetterControllerSettings();
            settings.SetBehavior("RaidEnemy", LetterBehavior.Ignore);

            Assert.AreEqual(LetterBehavior.Ignore, settings.GetBehavior("RaidEnemy"));
            Assert.AreEqual(1, settings.CustomCount);
        }

        [TestMethod]
        public void SetBehavior_Pause_PersistsAndReportsCustom()
        {
            var settings = new LetterControllerSettings();
            settings.SetBehavior("TraderCaravanArrival", LetterBehavior.Pause);

            Assert.AreEqual(LetterBehavior.Pause, settings.GetBehavior("TraderCaravanArrival"));
            Assert.AreEqual(1, settings.CustomCount);
        }

        [TestMethod]
        public void SetBehavior_BackToDefault_PrunesEntryToOptimizeSpaceComplexity()
        {
            var settings = new LetterControllerSettings();
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
            var settings = new LetterControllerSettings();
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
            var settings = new LetterControllerSettings();
            settings.SetBehavior("raidenemy", LetterBehavior.Ignore);

            Assert.AreEqual(LetterBehavior.Ignore, settings.GetBehavior("RaidEnemy"));
            Assert.AreEqual(LetterBehavior.Ignore, settings.GetBehavior("RAIDENEMY"));
        }

        [TestMethod]
        public void NullOrEmptyDefName_SafeAndHandledGracefully()
        {
            var settings = new LetterControllerSettings();
            settings.SetBehavior(null, LetterBehavior.Ignore);
            settings.SetBehavior(string.Empty, LetterBehavior.Ignore);

            Assert.AreEqual(0, settings.CustomCount);
            Assert.AreEqual(LetterBehavior.Default, settings.GetBehavior(null));
            Assert.AreEqual(LetterBehavior.Default, settings.GetBehavior(string.Empty));
        }
    }
}
