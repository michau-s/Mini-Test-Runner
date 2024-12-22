using MiniTest;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Loader;

namespace MiniTestRunner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            AssemblyLoadContext context = new AssemblyLoadContext("assembly", isCollectible: true);

            //TODO: write actuall functions like a normal person and put everything out of main

            foreach (var arg in args)
            {
                try
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

                    var testClass = assembly.GetTypes()
                        .Where(type => type.IsClass
                            && type.GetCustomAttribute(typeof(MiniTest.TestClassAttribute)) != null
                            && type.GetConstructors().Any(ctor => ctor.GetParameters().Length == 0)
                        );

                    foreach (var _class in testClass)
                    {
                        var instance = Activator.CreateInstance(_class);

                        if (instance == null)
                            continue;

                        var beforeEachMethod = instance.GetType().GetMethods()
                            .Where(method => method.GetCustomAttribute(typeof(MiniTest.BeforeEachAttribute)) != null).FirstOrDefault();

                        var afterEachMethod = instance.GetType().GetMethods()
                            .Where(method => method.GetCustomAttribute(typeof(MiniTest.AfterEachAttribute)) != null).FirstOrDefault();


                        Delegate? before = beforeEachMethod == null ? null : Delegate.CreateDelegate(typeof(Action), instance, beforeEachMethod);
                        Delegate? after = afterEachMethod == null ? null :Delegate.CreateDelegate(typeof(Action), instance, afterEachMethod);

                        var testMethods = instance.GetType().GetMethods()
                            .Where(method => method.GetCustomAttribute(typeof(MiniTest.TestMethodAttribute)) != null);

                        Console.WriteLine($"Class: {_class.Name}");

                        foreach (var method in testMethods)
                        {
                            Console.WriteLine($"Method: {method.Name}");
                        }

                        //TODO: Ignore test methods or attributes with incompatible configurations (e.g., parameter mismatch for DataRow).
                        //TODO: Write a warning message to the console in case of such configuration incompatibilities.

                        //I don't think I'll use this actually
                        var parametrizedTests = testMethods
                            .Where(method => method.GetCustomAttributes(typeof(MiniTest.DataRowAttribute)) != null);


                        //TODO: Fix methods without DataRow reporting parameter missmatch
                        foreach (var testMethod in testMethods)
                        {
                            var dataRows = testMethod.GetCustomAttributes(typeof(MiniTest.DataRowAttribute)).ToList();

                            if (dataRows.Count != 0)
                            {
                                foreach (var dataRow in dataRows)
                                {
                                    try
                                    {
                                        if (before != null)
                                        {
                                            before.DynamicInvoke(instance);
                                        }

                                        var testDataField = dataRow.GetType().GetField("testData");
                                        var parameters = testDataField?.GetValue(dataRow) as object[];

                                        testMethod.Invoke(instance, parameters);

                                        if (after != null)
                                        {
                                            after.DynamicInvoke(instance);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine($"test {testMethod.Name} failed: {e.Message}");
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    if (before != null)
                                    {
                                        before.DynamicInvoke(instance);
                                    }

                                    testMethod.Invoke(instance, null);

                                    if (after != null)
                                    {
                                        after.DynamicInvoke(instance);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine($"Test {testMethod.Name} failed: {e.Message}");
                                }
                            }
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    Console.WriteLine($"{arg}: File not found");
                }
                catch (Exception)
                {
                    Console.WriteLine($"{arg}: An error occured when loading the file");
                }
            }
        }
    }
}
