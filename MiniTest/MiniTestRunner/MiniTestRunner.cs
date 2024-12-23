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
    internal class MiniTestRunner
    {
        static void Main(string[] args)
        {
            // It needs to be an ABSOLUTE path, for some reason relative paths don't work
            foreach (var arg in args)
            {
                AssemblyLoadContext context = new AssemblyLoadContext("assembly", isCollectible: true);
                try
                {
                    List<TestData> data = TestLoader.LoadTests(arg, context);

                    (int passed, int total) results = (0, 0);

                    foreach (var test in data)
                    {
                        //apparently I cannot += on tuples so this works
                        var testResults = TestRunner.RunTests(test.Instance, test.TestMethods, test.Before, test.After);
                        results = (results.passed + testResults.passed, results.total + testResults.total);
                        Console.WriteLine("######################################################");
                    }

                    Console.WriteLine($"Summary of running tests from {Path.GetFileNameWithoutExtension(arg)}");
                    OutputFormatter.OutPutSummary(results.passed, results.total);
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

    }
}
