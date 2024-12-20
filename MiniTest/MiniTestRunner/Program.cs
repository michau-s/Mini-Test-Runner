using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Loader;

namespace MiniTestRunner
{
    internal class Program
    {
        static void Main(string[] args)
        {
            foreach (var arg in args)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(arg);
                    if (AssemblyLoadContext.GetLoadContext(assembly) is null)
                    {
                        continue;
                    }
                    AssemblyLoadContext context = AssemblyLoadContext.GetLoadContext(assembly)!;
                }
                catch(FileNotFoundException)
                {
                    Console.WriteLine($"{arg}: File not found");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{arg}: An error occured when loading the file");
                }
            }
        }
    }
}
