using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LetterController.Tests
{
    /// <summary>
    /// Custom in-process test runner for executing MSTest test suites inside isolated environments.
    /// Discovers all classes marked with <see cref="TestClassAttribute"/> and methods marked with
    /// <see cref="TestMethodAttribute"/>, supporting <see cref="TestInitializeAttribute"/> and <see cref="TestCleanupAttribute"/>.
    /// </summary>
    public static class Program
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("========================================");
            Console.WriteLine(" LetterController Test Suite Execution");
            Console.WriteLine("========================================");

            TestInitializer.AssemblyInitialize(null!);

            int passed = 0;
            int failed = 0;

            var asm = typeof(Program).Assembly;
            var testClasses = asm.GetTypes()
                .Where(t => t.GetCustomAttribute<TestClassAttribute>() != null)
                .OrderBy(t => t.Name);

            foreach (var type in testClasses)
            {
                var initMethod = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(m => m.GetCustomAttribute<TestInitializeAttribute>() != null);
                var cleanupMethod = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .FirstOrDefault(m => m.GetCustomAttribute<TestCleanupAttribute>() != null);

                var testMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                    .Where(m => m.GetCustomAttribute<TestMethodAttribute>() != null)
                    .OrderBy(m => m.Name);

                foreach (var method in testMethods)
                {
                    object? instance = null;
                    try
                    {
                        instance = Activator.CreateInstance(type);
                        initMethod?.Invoke(instance, null);
                        method.Invoke(instance, null);
                        cleanupMethod?.Invoke(instance, null);

                        Console.WriteLine($"[PASS] {type.Name}.{method.Name}");
                        passed++;
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            if (instance != null)
                            {
                                cleanupMethod?.Invoke(instance, null);
                            }
                        }
                        catch
                        {
                            // Ignore secondary cleanup errors during failure report
                        }

                        var inner = ex.InnerException ?? ex;
                        Console.WriteLine($"[FAIL] {type.Name}.{method.Name}: {inner.Message}");
                        Console.WriteLine(inner.StackTrace);
                        failed++;
                    }
                }
            }

            Console.WriteLine("----------------------------------------");
            Console.WriteLine($"Total Tests: {passed + failed} | Passed: {passed} | Failed: {failed}");
            Console.WriteLine("========================================");

            return failed == 0 ? 0 : 1;
        }
    }
}
