using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.IdentityModel.Selectors;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace VacationBudgetPlanner
{
    class Program
    {
        const double M_CONVERSION = 23.9536;
        const double J_CONVERSION = 145.441;
        const int HOURS = 24;
        const int MINUTES = 60;
        const int LOGON_ATTEMPTS = 3;
        static void Main(string[] args)
        {
            //string user = Environment.UserName;
            WindowsIdentity user = WindowsIdentity.GetCurrent();
            PrincipalContext pc = new PrincipalContext(ContextType.Machine, 
                Environment.MachineName);
            
            bool isValid = false;

            for (int i = 0; i < LOGON_ATTEMPTS; ++i)
            {
                Console.Write("Please enter password: ");
                SecureString passwd = GetPasswordInput();

                // Creates a pointer that points to a copy of the value in the SecureString obj
                // This also probably makes the secure string approach a moot point, 
                // the whole point of using a secure string is to put the data in secure(?) memory,
                // and by setting a copy of it to unsecure memory kind of defeats the purpose of this
                IntPtr pointer = Marshal.SecureStringToGlobalAllocUnicode(passwd);
                if (pc.ValidateCredentials(user.Name, Marshal.PtrToStringUni(pointer)))
                {
                    Console.WriteLine("Welcome User");
                    isValid = true;
                    break;
                }
                else
                {
                    Console.WriteLine("\nFailed validation");
                }

                Marshal.ZeroFreeGlobalAllocUnicode(pointer);
                passwd.Dispose();
            }

            if (!isValid)
            {
                Console.WriteLine("User failed validation, exiting.");
                return;
            }

            bool stopFlag;
            do
            {
                Console.WriteLine("Welcome to the Vacation Budget Planner App");
                Console.Write("What is your name? ");
                string name = GetUserInput();

                Console.Write($"Hello {name}! How many days are you staying (number please!)? ");
                int days = ConvertStringToInt(GetUserInput());

                Console.Write($"Nice! I bet those {days} will be great! How much money are you bringing? ");
                double money = ConvertStringToDouble(GetUserInput());
                string moneyPerDay = CalcMoneyPerDay(days, money);

                Console.Write($"Time to choose a destination {name}! Where would you like to travel?" +
                    $" (1) Mexico\n(2) Jamaica\n");
                string destination = GetUserInput().ToUpper();

                string localeMoney = "";
                double convertedMoneyTotal;
                switch (destination)
                {
                    case "MEXICO":
                    case "(1)":
                    case "(1":
                    case "1":
                        // convert money, calculate money per day
                        convertedMoneyTotal = money * M_CONVERSION;
                        localeMoney = CalcMoneyPerDay(days, convertedMoneyTotal, "en-MX");
                        break;
                    case "JAMAICA":
                    case "(2)":
                    case "(2":
                    case "2":
                        // convert money, calculate money per day
                        convertedMoneyTotal = money * J_CONVERSION;
                        localeMoney = CalcMoneyPerDay(days, convertedMoneyTotal, "en-JM");
                        break;
                    default:
                        Console.WriteLine($"{destination} is not a valid choice!");
                        Environment.Exit(1);
                        break;
                }

                double totalHours = days * HOURS;
                double totalMins = totalHours * MINUTES;

                Console.WriteLine($"{destination} sounds like a fun trip!");
                Console.WriteLine("****** =] || [= ******\n");

                Console.WriteLine($"Total Hours: {totalHours}");
                Console.WriteLine($"Total Minutes: {totalMins}");
                Console.WriteLine($"US dollars per day: {moneyPerDay}");
                Console.WriteLine($"Locale money per day: {localeMoney}");
                Console.WriteLine($"Enjoy your trip {name}!\n");

                Console.WriteLine("****** =] || [= ******\n");

                Console.Write("Would you like to rerun the program? (yes/no) ");
                string rerun = Console.ReadLine().Trim().ToLower();
                stopFlag = String.IsNullOrWhiteSpace(rerun) || rerun == "no";
            }
            while (!stopFlag);
            Console.WriteLine("\nHope you enjoyed the program!");
        }

        private static SecureString GetPasswordInput()
        {
            SecureString passwd = new SecureString();
            // ConsoleKeyInfo deals with the current key that was pressed
            // The true argument passed in intercepts the output so the character
            // will not be displayed on the console.
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);
            while (keyInfo.Key != ConsoleKey.Enter)
            {
                if (keyInfo.Key == ConsoleKey.Backspace)
                {
                    if (passwd.Length > 0)
                    {
                        passwd.RemoveAt(passwd.Length - 1);
                    }
                }
                else
                {
                    //char c = keyInfo.KeyChar;
                    //Console.WriteLine($"{c}");
                    passwd.AppendChar(keyInfo.KeyChar);
                }

                keyInfo = Console.ReadKey(true);
            }

            return passwd;
        }

        public static string GetUserInput()
        {
            string input = Console.ReadLine().Trim();
            if (String.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("Empty input detected, exiting.");
                Environment.Exit(1);
            }

            return input;
        }

        public static string CalcMoneyPerDay(int days, double money, string culture = "")
        {
            double moneyPerDay = money / days;
            var culObj = String.IsNullOrWhiteSpace(culture) ? CultureInfo.CurrentCulture
                : CultureInfo.GetCultureInfo(culture);
            return moneyPerDay.ToString("C", culObj);
        }

        public static int ConvertStringToInt(string v)
        {
            int number;
            bool parsed = Int32.TryParse(v, out number);
            if (parsed == false)
            {
                Console.WriteLine("Failed to convert string to int, exiting.");
                Environment.Exit(1);
            }

            return number;
        }

        public static double ConvertStringToDouble(string input)
        {
            double number;
            bool parsed = Double.TryParse(input, out number);
            if (parsed == false)
            {
                Console.WriteLine("Failed to convert string to double, exiting.");
                Environment.Exit(1);
            }

            return number;
        }
    }
}
