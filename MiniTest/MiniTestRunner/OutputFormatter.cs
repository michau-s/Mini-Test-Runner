﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTestRunner
{
    /// <summary>
    /// A helper class used to format information about the results of the tests
    /// </summary>
    static class OutputFormatter
    {
        /// <summary>
        /// Outputs a formatted table with a summary of test results
        /// </summary>
        /// <param name="passed"> Number of the tests that passed</param>
        /// <param name="total"> Number of total tests performed</param>
        public static void OutPutSummary(int passed, int total)
        {
            Console.WriteLine("**************************");
            Console.WriteLine($"* {"Tests passed:",-15} {passed,2}/{total,-3} *");
            Console.WriteLine($"* {"Failed:",-15} {total - passed,2}     *");
            Console.WriteLine("**************************");
        }

        /// <summary>
        /// Outputs a result of a single parameterless test
        /// </summary>
        /// <param name="passed">Number of the tests that passed</param>
        /// <param name="name">Number of total tests performed</param>
        /// <param name="eMessage">An optional parameter with an error messege in case of a failed test</param>
        public static void OutPutTestResult(bool passed, string name, string? eMessage = null)
        {
            if (passed)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{name,-70} : PASSED");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{name,-70} : FAILED");
                Console.ResetColor();
                if (eMessage != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(eMessage);
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Outputs a result of a single parameterized test
        /// </summary>
        /// <param name="passed">Number of the tests that passed</param>
        /// <param name="description">A description taken from <see cref="[DataRowAttribute]"/></param>
        /// <param name="eMessage">An optional parameter with an error messege in case of a failed test</param>
        public static void OutPutParametrizedTestResult(bool passed, string description, string? eMessage = null)
        {
            if (passed)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"- {description,-68} : PASSED");
                Console.ResetColor();
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"- {description,-68} : FAILED");
                Console.ResetColor();
                if (eMessage != null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(eMessage);
                    Console.ResetColor();
                }
            }
        }

        /// <summary>
        /// Outputs a warning to the console, in yellow
        /// </summary>
        /// <param name="warning"> a messege to be used as warning</param>
        public static void OutPutWarning(string warning)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(warning);
            Console.ResetColor();
        }
    }
}
