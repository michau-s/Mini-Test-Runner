using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniTestRunner
{
    static class OutputFormatter
    {
        public static void OutPutSummary(int passed, int total)
        {
            Console.WriteLine("**************************");
            Console.WriteLine($"* {"Tests passed:",-15} {passed,2}/{total,-3} *");
            Console.WriteLine($"* {"Failed:",-15} {total - passed,2}     *");
            Console.WriteLine("**************************");
        }

        public static void OutPutTestResult(bool passed, string name)
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
            }
        }

        public static void OutPutParametrizedTestResult(bool passed, string description)
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
            }
        }

        public static void OutPutWarning(string warning)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(warning);
            Console.ResetColor();
        }
    }
}
