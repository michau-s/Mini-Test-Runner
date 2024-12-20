namespace MiniTest
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TestClassAttribute : System.Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class TestMethodAttribute : System.Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class BeforeEachAttribute : System.Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class AfterEachAttribute : System.Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PriorityAttribute : System.Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Method)]
    public class DataRowAttribute : System.Attribute
    {
        private object?[] testData;
        string? dataset;

        public DataRowAttribute(object?[] testData, string? dataset = null)
        {
            this.testData = testData;
            this.dataset = dataset;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DescriptionAttribute : System.Attribute
    {

    }
}
