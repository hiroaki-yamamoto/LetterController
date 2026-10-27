using System;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LetterController.Tests
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            Console.WriteLine("Running custom test runner...");
            TestInitializer.AssemblyInitialize(null!);

            int passed = 0;
            int failed = 0;

            var asm = typeof(Program).Assembly;
            foreach (var type in asm.GetTypes())
            {
                if (type.GetCustomAttribute<TestClassAttribute>() != null)
                {
                    object? instance = null;
                    foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
                    {
                        if (method.GetCustomAttribute<TestMethodAttribute>() != null)
                        {
                            try
                            {
                                instance ??= Activator.CreateInstance(type);
                                method.Invoke(instance, null);
                                Console.WriteLine($"[PASS] {type.Name}.{method.Name}");
                                passed++;
                            }
                            catch (Exception ex)
                            {
                                var inner = ex.InnerException ?? ex;
                                Console.WriteLine($"[FAIL] {type.Name}.{method.Name}: {inner.Message}");
                                Console.WriteLine(inner.StackTrace);
                                failed++;
                            }
                        }
                    }
                }
            }

            Console.WriteLine($"\nTest Results: {passed} passed, {failed} failed.");

            AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
            {
                var name = new AssemblyName(eventArgs.Name).Name;
                if (name != null && name.StartsWith("UnityEngine"))
                {
                    return Assembly.LoadFrom(@"D:\SteamLibrary\steamapps\common\RimWorld\UnityStubs.dll");
                }
                return null;
            };

            // Also inspect Dynamic Diplomacy
            try
            {
                string ddPath = @"D:\SteamLibrary\steamapps\workshop\content\294100\3220299022\1.5\Assemblies\DynamicDiplomacy.dll";
                if (System.IO.File.Exists(ddPath))
                {
                    var ddAsm = Assembly.LoadFrom(ddPath);
                    Console.WriteLine($"Loaded DynamicDiplomacy: {ddAsm.FullName}");
                    foreach (var t in ddAsm.GetTypes())
                    {
                        if (t.Name.Contains("IncidentWorker"))
                        {
                            Console.WriteLine($"Type: {t.FullName}, BaseType: {t.BaseType?.FullName}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DD check failed: {ex.Message}");
            }

            return failed == 0 ? 0 : 1;
        }
    }
}
