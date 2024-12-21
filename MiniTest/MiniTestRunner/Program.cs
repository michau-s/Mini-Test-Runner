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

            foreach (var arg in args)
            {
                try
                {
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
                            .Where(method => method.GetCustomAttribute(typeof(MiniTest.BeforeEachAttribute)) != null).Single();

                        var afterEachMethod = instance.GetType().GetMethods()
                            .Where(method => method.GetCustomAttribute(typeof(MiniTest.AfterEachAttribute)) != null).Single();

                        Delegate before = Delegate.CreateDelegate(beforeEachMethod.GetType(), beforeEachMethod);
                        Delegate after = Delegate.CreateDelegate(afterEachMethod.GetType(), afterEachMethod);

                        var testMethods = instance.GetType().GetMethods()
                            .Where(method => method.GetCustomAttribute(typeof(MiniTest.TestMethodAttribute)) != null);

                        //TODO: Ignore test methods or attributes with incompatible configurations (e.g., parameter mismatch for DataRow).

                        var parametrizedTests = testMethods
                            .Where(method => method.GetCustomAttributes(typeof(MiniTest.DataRowAttribute)) != null);
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
