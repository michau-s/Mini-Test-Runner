using MiniTest;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Loader;
using System.Security.Cryptography;

namespace MiniTestRunner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                AssemblyLoadContext context = new AssemblyLoadContext("assembly", isCollectible: true);
                try
                {
                    LoadTests(arg, context);
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine($"{arg}: File not found");
                }
                catch (Exception)
                {
                    Console.WriteLine($"{arg}: An error occured when loading the file");
                }
                context.Unload();
            }
        }

        private static void LoadTests(string arg, AssemblyLoadContext context)
        {
            // Apparently it does not work without this resolver
            AssemblyDependencyResolver resolver = new AssemblyDependencyResolver(arg);

            context.Resolving += (context, assemblyName) =>
            {
                string? resolvedPath = resolver.ResolveAssemblyToPath(assemblyName);
                if (resolvedPath != null)
                {
                    return context.LoadFromAssemblyPath(resolvedPath);
                }
                return null;
            };

            Assembly assembly = context.LoadFromAssemblyPath(arg);

            var testClasses = GetTestClasses(assembly);

            foreach (var testClass in testClasses)
            {
                var instance = Activator.CreateInstance(testClass);

                if (instance == null)
                    continue;

                var after = GetAfterEach(instance);
                var before = GetBeforeEach(instance);
                var testMethods = GetSortedTestMethods(instance);

                Console.WriteLine($"Class: {testClass.Name}");

                foreach (var method in testMethods)
                {
                    Console.WriteLine($"Method: {method.Name}");
                }

                //TODO: Ignore test methods or attributes with incompatible configurations (e.g., parameter mismatch for DataRow).
                //TODO: Write a warning message to the console in case of such configuration incompatibilities.

                RunTests(instance, testMethods, before, after);

            }
        }

        private static void RunTests(object instance, List<MethodInfo> testMethods, Delegate? before, Delegate? after)
        {
            foreach (var testMethod in testMethods)
            {
                var dataRows = testMethod.GetCustomAttributes(typeof(MiniTest.DataRowAttribute)).ToList();

                if (dataRows.Count != 0)
                {
                    foreach (var dataRow in dataRows)
                    {
                        RunTest(instance, testMethod, before, after, dataRow);
                    }
                }
                else
                {
                    RunTest(instance, testMethod, before, after);
                }
            }
        }

        private static void RunTest(object instance, MethodInfo testMethod, Delegate? before, Delegate? after, Attribute? dataRow = null)
        {
            object[]? parameters = null;

            if (dataRow != null)
            {
                var testDataField = dataRow.GetType().GetField("testData");
                parameters = testDataField?.GetValue(dataRow) as object[];
            }

            if (before != null)
            {
                before.DynamicInvoke(null);
            }

            try
            {
                testMethod.Invoke(instance, parameters);
            }
            catch (Exception e)
            {
                if (e.InnerException is AssertionException)
                    Console.WriteLine($"Test: {testMethod.Name} failed: {e.InnerException.Message}");
            }

            if (after != null)
            {
                after.DynamicInvoke(null);
            }
        }

        private static Delegate? GetAfterEach(object instance)
        {
            var afterEachMethod = instance.GetType().GetMethods()
                            .Where(method => method.GetCustomAttribute(typeof(MiniTest.AfterEachAttribute)) != null).FirstOrDefault();

            return afterEachMethod == null ? null : Delegate.CreateDelegate(typeof(Action), instance, afterEachMethod);
        }

        private static Delegate? GetBeforeEach(object instance)
        {
            var beforeEachMethod = instance.GetType().GetMethods()
                            .Where(method => method.GetCustomAttribute(typeof(MiniTest.BeforeEachAttribute)) != null).FirstOrDefault();

            return beforeEachMethod == null ? null : Delegate.CreateDelegate(typeof(Action), instance, beforeEachMethod);
        }

        private static List<MethodInfo> GetSortedTestMethods(object instance)
        {
            return instance
                .GetType()
                .GetMethods()
                .Where(method => method.GetCustomAttribute(typeof(MiniTest.TestMethodAttribute)) != null)
                .OrderBy(method => (method.GetCustomAttribute(typeof(MiniTest.PriorityAttribute)) as MiniTest.PriorityAttribute)?.Priority ?? 0)
                .ThenBy(method => method.Name)
                .ToList();
        }

        private static List<Type> GetTestClasses(Assembly assembly)
        {
            return assembly
                .GetTypes()
                .Where(type => type.IsClass
                    && type.GetCustomAttribute(typeof(MiniTest.TestClassAttribute)) != null
                    && type.GetConstructors().Any(ctor => ctor.GetParameters().Length == 0)
                )
                .ToList();
        }
    }
}
