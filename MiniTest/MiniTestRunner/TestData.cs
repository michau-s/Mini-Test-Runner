using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniTestRunner
{
    /// <summary>
    /// A helper class to store data returned by <see cref="TestLoader.LoadTests(string, System.Runtime.Loader.AssemblyLoadContext)"/>
    /// </summary>
    class TestData
    {
        /// <summary>
        /// The instance of the class containing the test methods.
        /// </summary>
        public object Instance { get; set; }
        /// <summary>
        /// The list of test methods discovered in the class.
        /// </summary>
        public List<MethodInfo> TestMethods { get; set; }
        /// <summary>
        /// A delegate to be executed before each test method.
        /// </summary>
        public Delegate? Before { get; set; }
        /// <summary>
        /// A delegate to be executed after each test method.
        /// </summary>
        public Delegate? After { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestData"/> class with the specified parameters.
        /// </summary>
        /// <param name="instance">The instance of the class containing the test methods.</param>
        /// <param name="testMethods">The list of test methods discovered in the class.</param>
        /// <param name="before">A delegate to be executed before each test method runs.</param>
        /// <param name="after">A delegate to be executed after each test method runs.</param>
        public TestData(object instance, List<MethodInfo> testMethods, Delegate? before, Delegate? after)
        {
            Instance = instance;
            this.TestMethods = testMethods;
            Before = before;
            After = after;
        }
    }
}
