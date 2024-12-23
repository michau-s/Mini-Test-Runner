using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniTestRunner
{
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
