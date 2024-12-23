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
        /// <returns>
        /// A tuple containing the following:
        /// <list type="number">
        /// <item>- Number of tests passed </item>
        /// <item>- Number of total tests run </item>
        /// </list>
        /// </returns>
        public static (int passed, int total) RunTests(object instance, List<MethodInfo> testMethods, Delegate? before, Delegate? after)
        {
            int totalTestsRun = 0;
            int passed = 0;

            Console.WriteLine($"Running tests from class {instance.GetType().FullName}...");

            foreach (var testMethod in testMethods)
            {
                // Discover DataRows
                var dataRows = testMethod.GetCustomAttributes(typeof(MiniTest.DataRowAttribute)).ToList();

                // If the test has a description, print it to the screen
                var descriptionAttribute = (MiniTest.DescriptionAttribute?)testMethod.GetCustomAttribute(typeof(MiniTest.DescriptionAttribute));
                if (descriptionAttribute != null && !string.IsNullOrEmpty(descriptionAttribute.Description))
                {
                    Console.WriteLine(descriptionAttribute.Description);
                }

                // If this is a parametrized test we handle it differently (I could probably handle it in one block of code but couldn't figure out how)
                if (dataRows.Count != 0)
                {
                    Console.WriteLine($"{testMethod.Name,-70}");

                    foreach (var dataRow in dataRows)
                    {
                        // Extracting the description
                        var description = (dataRow as DataRowAttribute)?.Description ?? "No description available";

                        // Checking if the parameters provided match with the methods signature
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
                        
                        // If the parameters match, we run the tests with them
                        var result = RunTest(instance, testMethod, before, after, dataRow);
                        if (result.status)
                        {
                            passed++;
                            OutputFormatter.OutPutParametrizedTestResult(true, description);
                        }
                        else
                        {
                            OutputFormatter.OutPutParametrizedTestResult(false, description, result.failMessege);
                        }
                        totalTestsRun++;
                    }
                }
                // If there was no DataRowAttribute present
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
                        OutputFormatter.OutPutTestResult(false, testMethod.Name, result.failMessege);
                    }
                    totalTestsRun++;
                }
            }

            // Summarize the tests from this class
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
        /// <returns>
        /// A tuple containing the following:
        /// <list type="number">
        /// <item>- <c>true</c> and an empty string (<c>""</c>) if the test passes. </item>
        /// <item>- <c>false</c> and the exception message if the test fails. </item>
        /// </list>
        /// </returns>
        private static (bool status, string failMessege) RunTest(object instance, MethodInfo testMethod, Delegate? before, Delegate? after, Attribute? dataRow = null)
        {
            object[]? parameters = null;

            // If dataRow is not null, we assign parameters from it to our array, otherwise the array stays null and Invoke passes no parameters later
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
