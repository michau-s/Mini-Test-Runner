using MiniTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniTestRunner
{
    /// <summary>
    /// A static class providing a simple interface to run tests from an instance of a class
    /// </summary>
    static class TestRunner
    {
        /// <summary>
        /// The primary method used for running tests
        /// </summary>
        /// <param name="instance"> An instance of the class we want to test</param>
        /// <param name="testMethods"> A List of test methods inside the class</param>
        /// <param name="before"> A delegate to be executed before running every test</param>
        /// <param name="after"> A delegate to be executed after running every test</param>
        /// <returns></returns>
        public static (int passed, int total) RunTests(object instance, List<MethodInfo> testMethods, Delegate? before, Delegate? after)
        {
            int totalTestsRun = 0;
            int passed = 0;

            Console.WriteLine($"Running tests from class {instance.GetType().FullName}...");

            foreach (var testMethod in testMethods)
            {
                var dataRows = testMethod.GetCustomAttributes(typeof(MiniTest.DataRowAttribute)).ToList();

                var descriptionAttribute = (MiniTest.DescriptionAttribute?)testMethod.GetCustomAttribute(typeof(MiniTest.DescriptionAttribute));
                if (descriptionAttribute != null && !string.IsNullOrEmpty(descriptionAttribute.Description))
                {
                    Console.WriteLine(descriptionAttribute.Description);
                }

                if (dataRows.Count != 0)
                {
                    Console.WriteLine($"{testMethod.Name,-70}");

                    foreach (var dataRow in dataRows)
                    {
                        var description = (dataRow as DataRowAttribute)?.Description ?? "No description available";

                        var methodParameters = testMethod.GetParameters();
                        var testData = (dataRow as DataRowAttribute)?.testData;

                        if (methodParameters.Length != testData?.Length)
                        {
                            OutputFormatter.OutPutWarning($"Warning: Skipping {description} due to parameter missmatch!");
                            continue;
                        }

                        for (int i = 0; i < methodParameters.Length; i++)
                        {
                            if (testData[i]?.GetType() != methodParameters[i].ParameterType)
                            {
                                OutputFormatter.OutPutWarning($"Warning: Skipping {description} due to parameter missmatch!");
                                break;
                            }
                        }

                        var result = RunTest(instance, testMethod, before, after);
                        if (result.status)
                        {
                            passed++;
                            OutputFormatter.OutPutParametrizedTestResult(true, description);
                        }
                        else
                        {
                            OutputFormatter.OutPutParametrizedTestResult(false, description);
                            Console.WriteLine($"{result.failMessege}");
                        }
                        totalTestsRun++;
                    }
                }
                else
                {
                    var result = RunTest(instance, testMethod, before, after);
                    if (result.status)
                    {
                        passed++;
                        OutputFormatter.OutPutTestResult(true, testMethod.Name);
                    }
                    else
                    {
                        OutputFormatter.OutPutTestResult(false, testMethod.Name);
                        Console.WriteLine($"{result.failMessege}");
                    }
                    totalTestsRun++;
                }
            }

            OutputFormatter.OutPutSummary(passed, totalTestsRun);

            return (passed, totalTestsRun);
        }

        /// <summary>
        /// A helper method for RunTests, runs a single test
        /// </summary>
        /// <param name="instance"> An instance of the class we want to test</param>
        /// <param name="testMethod"> A test method to run</param>
        /// <param name="before"> A delegate to be executed before running the test</param>
        /// <param name="after"> A delegate to be executed after running the test</param>
        /// <param name="dataRow"> An optional parameter, if the testMethod requires parameters, they should be passed through this</param>
        /// <returns></returns>
        private static (bool status, string failMessege) RunTest(object instance, MethodInfo testMethod, Delegate? before, Delegate? after, Attribute? dataRow = null)
        {
            object[]? parameters = null;

            if (dataRow != null)
            {
                var testDataField = dataRow.GetType().GetField("testData");
                parameters = testDataField?.GetValue(dataRow) as object[];
            }

            before?.DynamicInvoke(null);

            try
            {
                testMethod.Invoke(instance, parameters);
            }
            catch (Exception e)
            {
                if (e.InnerException is AssertionException)
                {
                    if (after != null)
                    {
                        after.DynamicInvoke(null);
                    }
                    return (false, e.InnerException.Message);
                }
            }

            after?.DynamicInvoke(null);

            return (true, "");
        }
    }
}
