using LetterController.Core;
using LetterController.Patches;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RimWorld;
using Verse;

namespace LetterController.Tests
{
    /// <summary>
    /// Unit tests verifying Harmony patch behaviors for <see cref="IncidentWorkerPatch"/> and <see cref="LetterStackPatch"/>.
    /// </summary>
    [TestClass]
    public class PatchTests
    {
        [TestMethod]
        public void IncidentWorkerPatch_PrefixAndFinalizer_ManagesContextLifecycle()
        {
            IncidentContext.CurrentIncident = null;
            var incidentDef = new IncidentDef { defName = "RaidEnemy" };
            var worker = new IncidentWorker_RaidEnemy { def = incidentDef };

            // Prefix saves prior state (null) and sets worker's incident
            IncidentWorkerPatch.Prefix(worker, out var state);
            Assert.IsNull(state);
            Assert.AreEqual("RaidEnemy", IncidentContext.CurrentIncident?.defName);

            // Finalizer restores saved state
            var ex = IncidentWorkerPatch.Finalizer(null, state);
            Assert.IsNull(ex);
            Assert.IsNull(IncidentContext.CurrentIncident);
        }

        [TestMethod]
        public void IncidentWorkerPatch_Finalizer_PreservesExceptionAndRestoresContext()
        {
            var priorDef = new IncidentDef { defName = "PriorIncident" };
            IncidentContext.CurrentIncident = priorDef;

            var newDef = new IncidentDef { defName = "FaultyIncident" };
            var worker = new IncidentWorker_RaidEnemy { def = newDef };

            IncidentWorkerPatch.Prefix(worker, out var state);
            Assert.AreEqual("PriorIncident", state?.defName);
            Assert.AreEqual("FaultyIncident", IncidentContext.CurrentIncident?.defName);

            var expectedEx = new InvalidOperationException("Worker failed");
            var returnedEx = IncidentWorkerPatch.Finalizer(expectedEx, state);

            Assert.AreSame(expectedEx, returnedEx);
            Assert.AreEqual("PriorIncident", IncidentContext.CurrentIncident?.defName);

            // Clean up
            IncidentContext.CurrentIncident = null;
        }

        [TestMethod]
        public void IncidentWorkerPatch_NestedExecution_ReentrantSafety()
        {
            IncidentContext.CurrentIncident = null;
            var outerDef = new IncidentDef { defName = "OuterRaid" };
            var innerDef = new IncidentDef { defName = "InnerRaid" };

            var outerWorker = new IncidentWorker_RaidEnemy { def = outerDef };
            var innerWorker = new IncidentWorker_RaidEnemy { def = innerDef };

            IncidentWorkerPatch.Prefix(outerWorker, out var outerState);
            Assert.AreEqual("OuterRaid", IncidentContext.CurrentIncident?.defName);

            IncidentWorkerPatch.Prefix(innerWorker, out var innerState);
            Assert.AreEqual("InnerRaid", IncidentContext.CurrentIncident?.defName);

            IncidentWorkerPatch.Finalizer(null, innerState);
            Assert.AreEqual("OuterRaid", IncidentContext.CurrentIncident?.defName);

            IncidentWorkerPatch.Finalizer(null, outerState);
            Assert.IsNull(IncidentContext.CurrentIncident);
        }

        [TestMethod]
        public void LetterStackPatch_Prefix_AllowsWhenNoActiveIncidentContext()
        {
            IncidentContext.CurrentIncident = null;
            var letter = new StandardLetter { def = new LetterDef { defName = "ThreatBig" } };

            bool prefixResult = LetterStackPatch.Prefix(letter);
            Assert.IsTrue(prefixResult, "Letters without incident context must proceed normally.");
        }

        [TestMethod]
        public void LetterStackPatch_Prefix_SuppressesWhenIgnoreConfigured()
        {
            var incident = new IncidentDef { defName = "RaidEnemy" };
            var settings = new LetterControllerSettings();
            settings.SetBehavior("RaidEnemy", LetterBehavior.Ignore);
            LetterControllerMod.SetSettingsForTesting(settings);

            using (IncidentContext.EnterScope(incident))
            {
                var letter = new StandardLetter { def = new LetterDef { defName = "ThreatBig" } };
                bool prefixResult = LetterStackPatch.Prefix(letter);

                Assert.IsFalse(prefixResult, "Prefix should return false to suppress the letter.");
            }
        }

        [TestMethod]
        public void LetterStackPatch_Prefix_AllowsWhenDefaultOrPauseConfigured()
        {
            var incident = new IncidentDef { defName = "RaidEnemy" };
            var settings = new LetterControllerSettings();
            settings.SetBehavior("RaidEnemy", LetterBehavior.Pause);
            LetterControllerMod.SetSettingsForTesting(settings);

            using (IncidentContext.EnterScope(incident))
            {
                var letter = new StandardLetter { def = new LetterDef { defName = "ThreatBig" } };
                bool prefixResult = LetterStackPatch.Prefix(letter);

                Assert.IsTrue(prefixResult, "Prefix should return true when behavior is Pause.");
            }
        }
    }
}
