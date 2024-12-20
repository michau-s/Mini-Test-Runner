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
        private int _priority;

        public PriorityAttribute(int priority)
        {
            _priority = priority;
        }
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
        private string? description;

        public DescriptionAttribute(string? description)
        {
            this.description = description;
        }
    }

    public static class Assert
    {
        public static void ThrowsException<TException>(Action action, string message = "")
        {
            try
            {
                action();
                return;
            }
            catch (Exception e)
            {
                if (e is TException)
                    return;

                throw new AssertionException(message);
            }
        }

        public static void AreEqual<T>(T? expected, T? actual, string message = "")
        {
            if (expected is IEquatable<T>)
            {
                if (expected.Equals(actual))
                {
                    return;
                }
            }

            throw new AssertionException(message);
        }

        public static void AreNotEqual<T>(T? notExpected, T? actual, string message = "")
        {
            if (notExpected is IEquatable<T>)
            {
                if (!notExpected.Equals(actual))
                {
                    return;
                }
            }

            throw new AssertionException(message);
        }

        public static void IsTrue(bool condition, string message = "")
        {
            if (!condition)
            {
                throw new AssertionException(message);
            }
        }

        public static void IsFalse(bool condition, string message = "")
        {
            if (condition)
            {
                throw new AssertionException(message);
            }
        }

        public static void Fail(string message = "")
        {
            throw new AssertionException(message);
        }
    }

    [Serializable]
    internal class AssertionException : Exception
    {
        public AssertionException()
        {
        }

        public AssertionException(string? message) : base(message)
        {
        }

        public AssertionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
