using LetterController.Core;
using LetterController.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RimWorld;
using Verse;

namespace LetterController.Tests
{
    /// <summary>
    /// Unit tests verifying letter interception, suppression, pause rules, and speed control abstraction.
    /// </summary>
    [TestClass]
    public class LetterInterceptionPolicyTests
    {
        [TestMethod]
        public void ShouldSuppress_NullIncident_ReturnsFalse()
        {
            var settings = new LetterControllerSettings();
            settings.SetBehavior("RaidEnemy", LetterBehavior.Ignore);

            bool result = LetterInterceptionPolicy.ShouldSuppress(null, settings);
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShouldSuppress_IncidentWithNullOrEmptyDefName_ReturnsFalse()
        {
            var settings = new LetterControllerSettings();
            var incidentNullName = new IncidentDef { defName = null };
            var incidentEmptyName = new IncidentDef { defName = string.Empty };

            Assert.IsFalse(LetterInterceptionPolicy.ShouldSuppress(incidentNullName, settings));
            Assert.IsFalse(LetterInterceptionPolicy.ShouldSuppress(incidentEmptyName, settings));
        }

        [TestMethod]
        public void ShouldSuppress_NullSettings_ReturnsFalse()
        {
            var incident = new IncidentDef { defName = "RaidEnemy" };
            Assert.IsFalse(LetterInterceptionPolicy.ShouldSuppress(incident, null));
        }

        [TestMethod]
        public void ShouldSuppress_DefaultOrPauseBehavior_ReturnsFalse()
        {
            var settings = new LetterControllerSettings();
            var incidentDefault = new IncidentDef { defName = "RaidEnemy" };
            var incidentPause = new IncidentDef { defName = "TraderCaravanArrival" };

            settings.SetBehavior("RaidEnemy", LetterBehavior.Default);
            settings.SetBehavior("TraderCaravanArrival", LetterBehavior.Pause);

            Assert.IsFalse(LetterInterceptionPolicy.ShouldSuppress(incidentDefault, settings));
            Assert.IsFalse(LetterInterceptionPolicy.ShouldSuppress(incidentPause, settings));
        }

        [TestMethod]
        public void ShouldSuppress_IgnoreBehavior_ReturnsTrue()
        {
            var settings = new LetterControllerSettings();
            var incident = new IncidentDef { defName = "RaidEnemy" };
            settings.SetBehavior("RaidEnemy", LetterBehavior.Ignore);

            Assert.IsTrue(LetterInterceptionPolicy.ShouldSuppress(incident, settings));
        }

        [TestMethod]
        public void ShouldPause_NullLetterOrNullDef_ReturnsFalse()
        {
            var incident = new IncidentDef { defName = "RaidEnemy" };
            var settings = new LetterControllerSettings();
            settings.SetBehavior("RaidEnemy", LetterBehavior.Pause);

            Assert.IsFalse(LetterInterceptionPolicy.ShouldPause(null, 0, incident, settings));

            var letWithNullDef = new StandardLetter { def = null };
            Assert.IsFalse(LetterInterceptionPolicy.ShouldPause(letWithNullDef, 0, incident, settings));
        }

        [TestMethod]
        public void ShouldPause_DelayTicksGreaterThanZero_ReturnsFalse()
        {
            var incident = new IncidentDef { defName = "RaidEnemy" };
            var settings = new LetterControllerSettings();
            settings.SetBehavior("RaidEnemy", LetterBehavior.Pause);

            var letterDef = new LetterDef { defName = "ThreatBig" };
            var letter = new StandardLetter { def = letterDef };

            // Positive delay ticks mean the letter is queued, not yet arriving
            Assert.IsFalse(LetterInterceptionPolicy.ShouldPause(letter, 10, incident, settings));
            Assert.IsFalse(LetterInterceptionPolicy.ShouldPause(letter, 1, incident, settings));
        }

        [TestMethod]
        public void ShouldPause_NullIncidentOrNullSettings_ReturnsFalse()
        {
            var letterDef = new LetterDef { defName = "ThreatBig" };
            var letter = new StandardLetter { def = letterDef };
            var incident = new IncidentDef { defName = "RaidEnemy" };
            var settings = new LetterControllerSettings();
            settings.SetBehavior("RaidEnemy", LetterBehavior.Pause);

            Assert.IsFalse(LetterInterceptionPolicy.ShouldPause(letter, 0, null, settings));
            Assert.IsFalse(LetterInterceptionPolicy.ShouldPause(letter, 0, incident, null));
        }

        [TestMethod]
        public void ShouldPause_IgnoreOrDefaultBehavior_ReturnsFalse()
        {
            var letterDef = new LetterDef { defName = "ThreatBig" };
            var letter = new StandardLetter { def = letterDef };
            var incident = new IncidentDef { defName = "RaidEnemy" };
            var settings = new LetterControllerSettings();

            settings.SetBehavior("RaidEnemy", LetterBehavior.Default);
            Assert.IsFalse(LetterInterceptionPolicy.ShouldPause(letter, 0, incident, settings));

            settings.SetBehavior("RaidEnemy", LetterBehavior.Ignore);
            Assert.IsFalse(LetterInterceptionPolicy.ShouldPause(letter, 0, incident, settings));
        }

        [TestMethod]
        public void ShouldPause_ConfiguredToPause_ReturnsTrue()
        {
            var letterDef = new LetterDef { defName = "ThreatBig" };
            var letter = new StandardLetter { def = letterDef };
            var incident = new IncidentDef { defName = "RaidEnemy" };
            var settings = new LetterControllerSettings();
            settings.SetBehavior("RaidEnemy", LetterBehavior.Pause);

            Assert.IsTrue(LetterInterceptionPolicy.ShouldPause(letter, 0, incident, settings));
        }

        [TestMethod]
        public void HandlePostfix_InvokesPause_OnlyWhenShouldPauseIsTrue()
        {
            var letterDef = new LetterDef { defName = "ThreatBig" };
            var letter = new StandardLetter { def = letterDef };
            var incident = new IncidentDef { defName = "RaidEnemy" };
            var settings = new LetterControllerSettings();
            var mockController = new MockGameSpeedController();

            // Default behavior: should not invoke Pause
            settings.SetBehavior("RaidEnemy", LetterBehavior.Default);
            LetterInterceptionPolicy.HandlePostfix(letter, 0, incident, settings, mockController);
            Assert.AreEqual(0, mockController.PauseCallCount);

            // Pause behavior: should invoke Pause exactly once
            settings.SetBehavior("RaidEnemy", LetterBehavior.Pause);
            LetterInterceptionPolicy.HandlePostfix(letter, 0, incident, settings, mockController);
            Assert.AreEqual(1, mockController.PauseCallCount);
        }

        private sealed class MockGameSpeedController : IGameSpeedController
        {
            public int PauseCallCount { get; private set; }
            public void Pause() => PauseCallCount++;
        }
    }
}
