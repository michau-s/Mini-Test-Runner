using MiniTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MiniTestRunner
{
    static class TestRunner
    {
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
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"Warning: Skipping {description} due to parameter missmatch!");
                            Console.ResetColor();
                            continue;
                        }

                        for (int i = 0; i < methodParameters.Length; i++)
                        {
                            if (testData[i]?.GetType() != methodParameters[i].ParameterType)
                            {
                                Console.ForegroundColor = ConsoleColor.Yellow;
                                Console.WriteLine($"Warning: Skipping {description} due to parameter missmatch!");
                                Console.ResetColor();
                                break;
                            }
                        }

                        if (RunTest(instance, testMethod, before, after, dataRow))
                        {
                            passed++;
                            OutputFormatter.OutPutParametrizedTestResult(true, description);
                        }
                        else
                        {
                            OutputFormatter.OutPutParametrizedTestResult(false, description);
                        }
                        totalTestsRun++;
                    }
                }
                else
                {
                    if (RunTest(instance, testMethod, before, after))
                    {
                        passed++;
                        OutputFormatter.OutPutTestResult(true, testMethod.Name);
                    }
                    else
                    {
                        OutputFormatter.OutPutTestResult(false, testMethod.Name);
                    }
                    totalTestsRun++;
                }
            }

            OutputFormatter.OutPutSummary(passed, totalTestsRun);

            return (passed, totalTestsRun);
        }

        public static bool RunTest(object instance, MethodInfo testMethod, Delegate? before, Delegate? after, Attribute? dataRow = null)
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
                    Console.WriteLine($"{e.InnerException.Message}");
                    if (after != null)
                    {
                        after.DynamicInvoke(null);
                    }
                    return false;
                }
            }

            after?.DynamicInvoke(null);

            return true;
        }
    }
}
