using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace MiniTestRunner
{
    static class TestLoader
    {

        public static List<TestData> LoadTests(string arg, AssemblyLoadContext context)
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
                    OutputFormatter.OutPutWarning($"Warning! {testClass.Name} has no parameterless constructor! Skipping class...");
                }
            }

            return testClasses
                .Where(type => type.GetConstructors().Any(ctor => ctor.GetParameters().Length == 0))
                .ToList();
        }
    }
}
