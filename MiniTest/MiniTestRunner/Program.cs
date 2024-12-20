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

                    var beforeEachMethod = testClass.GetType().GetMethods()
                        .Where(method => method.GetCustomAttribute(typeof(MiniTest.BeforeEachAttribute)) != null);

                    var afterEachMethod = testClass.GetType().GetMethods()
                        .Where(method => method.GetCustomAttribute(typeof(MiniTest.BeforeEachAttribute)) != null);

                    //TODO: Late bind BeforeEach and AfterEach methods to a delegate

                    var testMethods = testClass.GetType().GetMethods()
                        .Where(method => method.GetCustomAttribute(typeof(MiniTest.TestMethodAttribute)) != null);

                    var parametrizedTests = testMethods
                        .Where(method => method.GetCustomAttributes(typeof(MiniTest.DataRowAttribute)) != null);

                    //TODO: Ignore test methods or attributes with incompatible configurations (e.g., parameter mismatch for DataRow).
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
