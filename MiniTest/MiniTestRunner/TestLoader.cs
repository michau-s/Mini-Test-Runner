﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace MiniTestRunner
{
    /// <summary>
    /// A static class providing a simple interface to load tests to an <see cref="AssemblyLoadContext"/>
    /// </summary>
    static class TestLoader
    {
        /// <summary>
        /// The primary method used for tests discovery as provided in the task description
        /// </summary>
        /// <param name="arg"> A path to the .dll file</param>
        /// <param name="context"> A context to load to</param>
        /// <returns> A List of <see cref="TestData" /> structures for testing with <see cref="TestRunner.RunTests(object, List{MethodInfo}, Delegate?, Delegate?)"/></returns>
        public static List<TestData> LoadTests(string arg, AssemblyLoadContext context)
        {
            // Resolver
            arg = Path.GetFullPath(arg);

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

            // Extracting the tests
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

        /// <summary>
        /// A helper method used to recover the method that should be run before each test.
        /// <remarks>
        /// It assumes this method returns void and takes no parameters
        /// </remarks>
        /// </summary>
        /// <param name="instance">An instance of a class we want to search</param>
        /// <returns>A <see cref="Delegate?"/> bound to a method marked with <see cref="MiniTest.AfterEachAttribute"/></returns>
        private static Delegate? GetAfterEach(object instance)
        {
            var afterEachMethod = instance.GetType().GetMethods()
                            .Where(method => method.GetCustomAttribute(typeof(MiniTest.AfterEachAttribute)) != null).FirstOrDefault();

            return afterEachMethod == null ? null : Delegate.CreateDelegate(typeof(Action), instance, afterEachMethod);
        }

        /// <summary>
        /// A helper method used to recover the method that should be run before each test.
        /// <remarks>
        /// It assumes this method returns void and takes no parameters
        /// </remarks>
        /// </summary>
        /// <param name="instance">An instance of a class we want to search</param>
        /// <returns>A <see cref="Delegate?"/> bound to a method marked with <see cref="MiniTest.BeforeEachAttribute"/></returns>
        private static Delegate? GetBeforeEach(object instance)
        {
            var beforeEachMethod = instance.GetType().GetMethods()
                            .Where(method => method.GetCustomAttribute(typeof(MiniTest.BeforeEachAttribute)) != null).FirstOrDefault();

            return beforeEachMethod == null ? null : Delegate.CreateDelegate(typeof(Action), instance, beforeEachMethod);
        }

        /// <summary>
        /// A helper method used to recover all methods marked with <see cref="MiniTest.TestMethodAttribute"/> from a class instance
        /// </summary>
        /// <param name="instance"> An instance of a class we want to search</param>
        /// <returns>A <see cref="List{MethodInfo}"/> sorted by <see cref="MiniTest.PriorityAttribute"/> then by the method name alphabetically</returns>
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

        /// <summary>
        /// A helper method used to recover all classes marked with <see cref="MiniTest.TestClassAttribute"/>
        /// </summary>
        /// <param name="assembly"> An assembly to scan for test classes</param>
        /// <returns>A <see cref="List{Type}"/> of test classes found in the assembly</returns>
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
