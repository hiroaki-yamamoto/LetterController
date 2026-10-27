using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LetterController.Tests
{
    /// <summary>
    /// Assembly-level test lifecycle coordinator.
    /// Configures assembly resolution so that RimWorld game assemblies and Harmony
    /// can be located dynamically during unit test discovery and execution.
    /// </summary>
    [TestClass]
    public class TestInitializer
    {
        private static bool isInitialized;

        /// <summary>
        /// Registers dynamic assembly resolution hooks for RimWorld dependencies before any unit test runs.
        /// </summary>
        /// <param name="context">The MSTest execution context.</param>
        [AssemblyInitialize]
        public static void AssemblyInitialize(TestContext context)
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;

            AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
            {
                var assemblyName = new AssemblyName(eventArgs.Name).Name;
                if (string.IsNullOrEmpty(assemblyName))
                {
                    return null;
                }

                string rimWorldDir = Path.GetFullPath(@"D:\SteamLibrary\steamapps\common\RimWorld");
                string candidate = Path.Combine(rimWorldDir, assemblyName + ".dll");
                if (File.Exists(candidate))
                {
                    return Assembly.LoadFrom(candidate);
                }

                string workshopHarmony = Path.GetFullPath(@"D:\SteamLibrary\steamapps\workshop\content\294100\2009463077\1.7\Assemblies\0Harmony.dll");
                if (assemblyName == "0Harmony" && File.Exists(workshopHarmony))
                {
                    return Assembly.LoadFrom(workshopHarmony);
                }

                return null;
            };
        }
    }
}
