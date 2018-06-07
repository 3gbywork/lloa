using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfficeAutomationClient.Helper
{
    class RandomHelper
    {
        private static Random random = new Random();

        public static string RandomString(int count, bool upperLetter = true, bool lowerLetter = true, bool number = true)
        {
            if (!upperLetter && !lowerLetter && !number) return string.Empty;

            var index = 0;
            var scopes = new List<Scope>();
            if (upperLetter)
            {
                var scope = new Scope { FirstIndex = index, FirstChar = 'A', Count = 26 };
                scopes.Add(scope);
                index += scope.Count;
            }
            if (lowerLetter)
            {
                var scope = new Scope { FirstIndex = index, FirstChar = 'a', Count = 26 };
                scopes.Add(scope);
                index += scope.Count;
            }
            if (number)
            {
                var scope = new Scope { FirstIndex = index, FirstChar = '0', Count = 10 };
                scopes.Add(scope);
                index += scope.Count;
            }

            var builder = new StringBuilder(count * 2);
            for (int i = 0; i < count; i++)
            {
                var value = random.Next(index);
                foreach (var scope in scopes)
                {
                    if (scope.FirstIndex <= value && (scope.FirstIndex + scope.Count > value))
                    {
                        var @char = (char)(value - scope.FirstIndex + scope.FirstChar);

                        builder.Append(@char);
                        break;
                    }
                }
            }

            return builder.ToString();
        }

        struct Scope
        {
            public int FirstIndex;
            public char FirstChar;
            public int Count;
        }
    }
}
