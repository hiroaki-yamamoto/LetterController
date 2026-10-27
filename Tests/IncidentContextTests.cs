using LetterController.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RimWorld;

namespace LetterController.Tests
{
    /// <summary>
    /// Unit tests verifying the ambient scope, RAII scope management, and thread isolation of <see cref="IncidentContext"/>.
    /// </summary>
    [TestClass]
    public class IncidentContextTests
    {
        [TestMethod]
        public void CurrentIncident_DefaultIsNull()
        {
            IncidentContext.CurrentIncident = null;
            Assert.IsNull(IncidentContext.CurrentIncident);
        }

        [TestMethod]
        public void IncidentContext_ThreadStaticIsolation()
        {
            var def = new IncidentDef { defName = "RaidEnemy" };
            IncidentContext.CurrentIncident = def;

            bool threadHadNull = false;
            var thread = new Thread(() =>
            {
                threadHadNull = (IncidentContext.CurrentIncident == null);
            });

            thread.Start();
            thread.Join();

            Assert.IsTrue(threadHadNull, "Background thread should not inherit ThreadStatic ambient context.");
            Assert.AreEqual("RaidEnemy", IncidentContext.CurrentIncident?.defName);

            // Clean up
            IncidentContext.CurrentIncident = null;
        }

        [TestMethod]
        public void EnterScope_SetsCurrentIncidentAndRestoresOnDispose()
        {
            IncidentContext.CurrentIncident = null;
            var raidDef = new IncidentDef { defName = "RaidEnemy" };

            using (IncidentContext.EnterScope(raidDef))
            {
                Assert.AreEqual("RaidEnemy", IncidentContext.CurrentIncident?.defName);
            }

            Assert.IsNull(IncidentContext.CurrentIncident, "Scope disposal should restore prior null context.");
        }

        [TestMethod]
        public void EnterScope_NestedScopes_RestoreInLIFOOrder()
        {
            IncidentContext.CurrentIncident = null;
            var outerDef = new IncidentDef { defName = "OuterIncident" };
            var innerDef = new IncidentDef { defName = "InnerIncident" };

            using (IncidentContext.EnterScope(outerDef))
            {
                Assert.AreEqual("OuterIncident", IncidentContext.CurrentIncident?.defName);

                using (IncidentContext.EnterScope(innerDef))
                {
                    Assert.AreEqual("InnerIncident", IncidentContext.CurrentIncident?.defName);
                }

                Assert.AreEqual("OuterIncident", IncidentContext.CurrentIncident?.defName);
            }

            Assert.IsNull(IncidentContext.CurrentIncident);
        }

        [TestMethod]
        public void EnterScope_ExceptionInScope_ProperlyRestoresContext()
        {
            IncidentContext.CurrentIncident = null;
            var testDef = new IncidentDef { defName = "FaultyIncident" };

            try
            {
                using (IncidentContext.EnterScope(testDef))
                {
                    Assert.AreEqual("FaultyIncident", IncidentContext.CurrentIncident?.defName);
                    throw new InvalidOperationException("Simulated incident failure.");
                }
            }
            catch (InvalidOperationException)
            {
                // Expected
            }

            Assert.IsNull(IncidentContext.CurrentIncident, "Prior context must be restored despite exception.");
        }
    }
}
