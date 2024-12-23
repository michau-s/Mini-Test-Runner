using MiniTest;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
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

            var testMethodParameters = testMethod.GetParameters();

            //if ((parameters == null && testMethodParameters.Length > 0) ||
            //    (parameters != null && parameters.Length != testMethodParameters.Length))
            //{
            //    Console.WriteLine($"Warning! {testMethod.Name}: Parameter Mismatch, skipping test method...");
            //    return;
            //}

            //if (parameters != null)
            //{
            //    for (int i = 0; i < testMethodParameters.Length; i++)
            //    {
            //        if (parameters[i] != null && testMethodParameters[i].ParameterType.IsAssignableFrom(parameters[i].GetType()))
            //        {
            //            Console.WriteLine($"Warning! {testMethod.Name}: Parameter Mismatch, skipping test method...");
            //            return;
            //        }
            //    }
            //}

            //if(parameters != null && testMethod.GetParameters().Length != parameters.Length || (parameters == null && testMethodParameters != null) || (parameters != null && testMethodParameters == null))
            //{
            //    return;
            //}

            //for (int i = 0; i < testMethodParameters.Length; i++)
            //{
            //    if (parameters == null)
            //        break;
            //    if (parameters[i].GetType() != testMethodParameters[i].ParameterType)
            //    {
            //        Console.WriteLine($"Warning! {testMethod.Name}: Parameter Mismatch, skipping test");
            //    }
            //}

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
            var testClasses = assembly
                .GetTypes()
                .Where(type => type.IsClass
                    && type.GetCustomAttribute(typeof(MiniTest.TestClassAttribute)) != null
                )
                .ToList();

            foreach (var testClass in testClasses)
            {
                if (!testClass.GetConstructors().Any(ctor => ctor.GetParameters().Length == 0))
                    Console.WriteLine($"Warning! {testClass.Name} has no parameterless constructor!");

            }

            return testClasses
                .Where(type => type.GetConstructors().Any(ctor => ctor.GetParameters().Length == 0))
                .ToList();
        }
    }
}
