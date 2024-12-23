using MiniTest;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Loader;
using System.Security.Cryptography;
using System.Xml.XPath;

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
                    List<TestData> data = LoadTests(arg, context);

                    (int passed, int total) results = (0,0);

                    foreach (var test in data)
                    {
                        var testResults = RunTests(test.Instance, test.TestMethods, test.Before, test.After);
                        results = (results.passed + testResults.passed, results.total + testResults.total);
                        Console.WriteLine("######################################################");
                    }

                    Console.WriteLine($"Summary of running tests from {Path.GetFileNameWithoutExtension(arg)}");
                    Console.WriteLine("**************************");
                    Console.WriteLine($"* {"Tests passed:",-15} {results.passed,2}/{results.total,-3} *");
                    Console.WriteLine($"* {"Failed:",-15} {results.total - results.passed,2}     *");
                    Console.WriteLine("**************************");
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine($"{arg}: File not found");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{arg}: An error occured: {e.Message}");
                }

                context.Unload();
            }
        }

        private static List<TestData> LoadTests(string arg, AssemblyLoadContext context)
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

            List<TestData> tests = new List<TestData>();

            foreach (var testClass in testClasses)
            {
                var instance = Activator.CreateInstance(testClass);

                if (instance == null)
                    continue;

                var after = GetAfterEach(instance);
                var before = GetBeforeEach(instance);
                var testMethods = GetSortedTestMethods(instance);

                tests.Add(new TestData(instance, testMethods, before, after));
            }

            return tests;
        }

        private static (int passed, int total) RunTests(object instance, List<MethodInfo> testMethods, Delegate? before, Delegate? after)
        {
            int totalTestsRun = 0;
            int passed = 0;

            Console.WriteLine($"Running tests from class {instance.GetType().FullName}...");

            foreach (var testMethod in testMethods)
            {
                var dataRows = testMethod.GetCustomAttributes(typeof(MiniTest.DataRowAttribute)).ToList();

                var descriptionAttribute = (MiniTest.DescriptionAttribute?)testMethod.GetCustomAttribute(typeof(MiniTest.DescriptionAttribute));
                if (descriptionAttribute != null && !string.IsNullOrEmpty(descriptionAttribute.Description))
                {
                    Console.WriteLine(descriptionAttribute.Description);
                }

                if (dataRows.Count != 0)
                {
                    Console.WriteLine($"{testMethod.Name,-70}");

                    foreach (var dataRow in dataRows)
                    {
                        var description = (dataRow as DataRowAttribute)?.Description ?? "No description available";

                        if (RunTest(instance, testMethod, before, after, dataRow))
                        {
                            passed++;
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"- {description, -68} : PASSED");
                            Console.ResetColor();

                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine($"- {description, -68} : FAILED");
                            Console.ResetColor();
                        }
                        totalTestsRun++;
                    }
                }
                else
                {
                    if(RunTest(instance, testMethod, before, after))
                    {
                        passed++;
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine( $"{testMethod.Name, -70} : PASSED");
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{testMethod.Name, -70} : FAILED");
                        Console.ResetColor();
                    }
                    totalTestsRun++;
                }
            }

            Console.WriteLine("**************************");
            Console.WriteLine($"* {"Tests passed:", -15} {passed, 2}/{totalTestsRun, -3} *");
            Console.WriteLine($"* {"Failed:", -15} {totalTestsRun - passed, 2}     *");
            Console.WriteLine("**************************");

            return (passed, totalTestsRun);
        }

        private static bool RunTest(object instance, MethodInfo testMethod, Delegate? before, Delegate? after, Attribute? dataRow = null)
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
                {
                    Console.WriteLine($"{e.InnerException.Message}");
                    if (after != null)
                    {
                        after.DynamicInvoke(null);
                    }
                    return false;
                }
            }

            if (after != null)
            {
                after.DynamicInvoke(null);
            }

            return true;
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
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine($"Warning! {testClass.Name} has no parameterless constructor! Skipping class...");
                    Console.ResetColor();
                }

            }

            return testClasses
                .Where(type => type.GetConstructors().Any(ctor => ctor.GetParameters().Length == 0))
                .ToList();
        }
    }

    class TestData
    {
        object _instance;
        List<MethodInfo> testMethods;
        Delegate? _before;
        Delegate? _after;

        public object Instance => _instance;
        public List<MethodInfo> TestMethods => testMethods;
        public Delegate? Before => _before;
        public Delegate? After => _after;

        public TestData(object instance, List<MethodInfo> testMethods, Delegate? before, Delegate? after)
        {
            _instance = instance;
            this.testMethods = testMethods;
            _before = before;
            _after = after;
        }
    }
}
