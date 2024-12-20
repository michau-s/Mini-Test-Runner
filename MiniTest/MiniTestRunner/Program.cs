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
                }
                catch(FileNotFoundException)
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
