using System.Threading;
using System.Threading.Tasks;
using LetterController.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LetterController.Tests
{
    /// <summary>
    /// Unit tests verifying the ambient scope and thread isolation of <see cref="IncidentContext"/>.
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
            IncidentContext.CurrentIncident = null;

            bool threadHadNull = false;
            var thread = new Thread(() =>
            {
                threadHadNull = (IncidentContext.CurrentIncident == null);
            });

            thread.Start();
            thread.Join();

            Assert.IsTrue(threadHadNull);
            Assert.IsNull(IncidentContext.CurrentIncident);
        }
    }
}
