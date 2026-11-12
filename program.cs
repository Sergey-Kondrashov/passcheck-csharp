using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PassCheck
{
    internal static class Program
    {
        private static readonly string[] CommonPasswords =
        {
            "password", "123456", "123456789", "12345678", "qwerty", "qwerty123",
            "111111", "abc123", "password1", "iloveyou", "admin", "welcome",
            "letmein", "monkey", "dragon", "football", "1q2w3e4r", "sunshine",
            "master", "login"
        };

        private static readonly string[] Sequences =
        {
            "abcdefghijklmnopqrstuvwxyz", "0123456789", "qwertyuiop", "asdfghjkl", "zxcvbnm"
        };

        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintUsage();
                return 1;
            }

            switch (args[0].ToLowerInvariant())
            {
                case "check":
                    return RunCheck();
                case "generate":
                    return RunGenerate(args);
                default:
                    PrintUsage();
                    return 1;
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine("PassCheck - password strength checker and generator");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  dotnet run -- check             check a password (input is hidden)");
            Console.WriteLine("  dotnet run -- generate [len]    generate a random password (default 16)");
        }

        // ---------- check ----------

        private static int RunCheck()
        {
            Console.Write("Enter a password to check (input is hidden): ");
            string pw = ReadHidden();
            if (pw.Length == 0)
            {
                Console.Error.WriteLine("No password entered.");
                return 1;
            }

            bool hasLower = pw.Any(char.IsLower);
            bool hasUpper = pw.Any(char.IsUpper);
            bool hasDigit = pw.Any(char.IsDigit);
            bool hasSymbol = pw.Any(c => !char.IsLetterOrDigit(c));

            int pool = 0;
            if (hasLower) pool += 26;
            if (hasUpper) pool += 26;
            if (hasDigit) pool += 10;
            if (hasSymbol) pool += 33;

            // Rough estimate: length * log2(size of the character pool).
            double entropy = pw.Length * Math.Log2(pool);

            var warnings = new List<string>();

            if (pw.Length < 12)
                warnings.Add("Shorter than 12 characters.");

            if (CommonPasswords.Contains(pw.ToLowerInvariant()))
            {
                warnings.Add("This is one of the most commonly used passwords.");
                entropy = Math.Min(entropy, 10);
            }

            if (HasRepeats(pw))
            {
                warnings.Add("Contains the same character three times in a row.");
                entropy *= 0.8;
            }

            if (HasSequence(pw, 4))
            {
                warnings.Add("Contains an easy sequence (e.g. 1234, abcd, qwer).");
                entropy *= 0.8;
            }

            Console.WriteLine();
            Console.WriteLine("Length       : " + pw.Length);
            Console.WriteLine("Lowercase    : " + YesNo(hasLower));
            Console.WriteLine("Uppercase    : " + YesNo(hasUpper));
            Console.WriteLine("Digits       : " + YesNo(hasDigit));
            Console.WriteLine("Symbols      : " + YesNo(hasSymbol));
            Console.WriteLine("Entropy (est): " + entropy.ToString("F0") + " bits");
            Console.WriteLine("Rating       : " + Rate(entropy));

            if (warnings.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Warnings:");
                foreach (string w in warnings)
                    Console.WriteLine(" - " + w);
            }

            Console.WriteLine();
            Console.WriteLine("Note: this is only a rough estimate. Use a password manager and a unique password per site.");
            return 0;
        }

        private static string YesNo(bool value)
        {
            return value ? "yes" : "no";
        }

        private static string Rate(double entropy)
        {
            if (entropy < 28) return "very weak";
            if (entropy < 36) return "weak";
            if (entropy < 60) return "reasonable";
            if (entropy < 80) return "strong";
            return "very strong";
        }

        private static bool HasRepeats(string pw)
        {
            for (int i = 0; i + 2 < pw.Length; i++)
            {
                if (pw[i] == pw[i + 1] && pw[i] == pw[i + 2])
                    return true;
            }
            return false;
        }

        private static bool HasSequence(string pw, int length)
        {
            string lower = pw.ToLowerInvariant();
            foreach (string seq in Sequences)
            {
                string reversed = new string(seq.Reverse().ToArray());
                for (int i = 0; i + length <= lower.Length; i++)
                {
                    string part = lower.Substring(i, length);
                    if (seq.Contains(part) || reversed.Contains(part))
                        return true;
                }
            }
            return false;
        }

        private static string ReadHidden()
        {
            var sb = new StringBuilder();
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Enter)
                {
                    Console.WriteLine();
                    break;
                }
                if (key.Key == ConsoleKey.Backspace)
                {
                    if (sb.Length > 0) sb.Length--;
                }
                else if (!char.IsControl(key.KeyChar))
                {
                    sb.Append(key.KeyChar);
                }
            }
            return sb.ToString();
        }

        // ---------- generate ----------

        private static int RunGenerate(string[] args)
        {
            int length = 16;
            if (args.Length > 1 && !int.TryParse(args[1], out length))
            {
                Console.Error.WriteLine("Length must be a number.");
                return 1;
            }
            if (length < 8 || length > 128)
            {
                Console.Error.WriteLine("Length must be between 8 and 128.");
                return 1;
            }

            Console.WriteLine(Generate(length));
            return 0;
        }

        private static string Generate(int length)
        {
            string[] sets =
            {
                "abcdefghijklmnopqrstuvwxyz",
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                "0123456789",
                "!@#$%^&*()-_=+[]{};:,.?"
            };
            string all = string.Concat(sets);

            var chars = new List<char>();

            // At least one character from every group.
            foreach (string set in sets)
                chars.Add(set[RandomNumberGenerator.GetInt32(set.Length)]);

            while (chars.Count < length)
                chars.Add(all[RandomNumberGenerator.GetInt32(all.Length)]);

            // Fisher-Yates shuffle with a cryptographically secure random number generator.
            for (int i = chars.Count - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                char temp = chars[i];
                chars[i] = chars[j];
                chars[j] = temp;
            }

            return new string(chars.ToArray());
        }
    }
}
