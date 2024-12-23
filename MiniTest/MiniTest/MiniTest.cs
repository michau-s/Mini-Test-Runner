namespace MiniTest
{
    /// <summary>
    /// An attribute used to mark a class as a test class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class TestClassAttribute : System.Attribute
    {

    }

    /// <summary>
    /// An attribute used to mark a method as a test method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class TestMethodAttribute : System.Attribute
    {

    }

    /// <summary>
    /// An attribute used to indicate a setup method to be ran after each test
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class BeforeEachAttribute : System.Attribute
    {

    }

    /// <summary>
    /// An attribute used to indicate a cleanup method to be ran after each test
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class AfterEachAttribute : System.Attribute
    {

    }

    /// <summary>
    /// An attribute used to set priorioty for a test method
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class PriorityAttribute : System.Attribute
    {
        public int Priority { get; }

        public PriorityAttribute(int priority)
        {
            Priority = priority;
        }
    }

    /// <summary>
    /// An attribute used to pass parameters to test methods that require them
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class DataRowAttribute : System.Attribute
    {
        public object?[] testData;
        public string? Description;

        public DataRowAttribute(object?[] testData, string? description = null)
        {
            this.Description = description;
            this.testData = testData;
        }

        public DataRowAttribute(object? testData, string? description = null)
        {
            this.Description = description;
            this.testData = new object?[] { testData };
        }
    }

    /// <summary>
    /// An attribute used to provide a description for a test class or method
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class DescriptionAttribute : System.Attribute
    {
        public string? Description { get; }

        public DescriptionAttribute(string? description)
        {
            this.Description = description;
        }
    }

    /// <summary>
    /// A static providing ways to assert the tests
    /// </summary>
    public static class Assert
    {
        public static void ThrowsException<TException>(Action action, string message = "")
        {
            try
            {
                action();
                throw new AssertionException($"Expected exception type:<{typeof(TException)}> but no exception was thrown. {message}");
            }
            catch (Exception e)
            {
                if (e is TException)
                    return;

                throw new AssertionException($"Expected exception type:<{typeof(TException)}>. Actual exception type:<{e.GetType()}>. {message}");
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

            throw new AssertionException($"Expected: {expected?.ToString() ?? "null"}. Actual: {actual?.ToString() ?? "null"}. {message}");
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

            throw new AssertionException($"Expected any value except: {notExpected?.ToString() ?? "null"}. Actual: {actual?.ToString() ?? "null"}. {message}");
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

    /// <summary>
    /// A class implementing the exception to be used in <see cref="Assert"/>
    /// </summary>
    [Serializable]
    public class AssertionException : Exception
    {
        /// <summary>
        /// Thrown when a method from <see cref="Assert"/> fails
        /// </summary>
        public AssertionException()
        {
        }
        /// <summary>
        /// Thrown when a method from <see cref="Assert"/> fails
        /// </summary>
        public AssertionException(string? message) : base(message)
        {
        }

        /// <summary>
        /// Thrown when a method from <see cref="Assert"/> fails
        /// </summary>
        public AssertionException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
