﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Circuits
{
    internal static class Utils
    {
        public static string FormatValueWithPrefix(double value)
        {
            if (value == 0)
            {
                return "0.00 ";
            }
            int powOfTenTriple = (int)Math.Floor(Math.Log10(Math.Abs(value)) / 3);
            int boundedPowOfTenTriple = Math.Max(-10, Math.Min(10, powOfTenTriple)); //limit of current SI prefixes
            double formattedVal = Math.Round(value * Math.Pow(10, -boundedPowOfTenTriple * 3), 2);
            return $"{formattedVal} {GetPrefix(boundedPowOfTenTriple)}";
        }

        private static string GetPrefix(int powOfTenTriple)
        {
            int prefixNum = powOfTenTriple + 10;
            string[] prefixes = new string[]
            {
                "q", "r", "y", "z", "a", "f", "p", "n", "μ", "m", "", "k", "M", "G", "T", "P", "E", "Z", "Y", "R", "Q"
            };
            return prefixes[prefixNum];
        }

        public static string AddSpaceToCamelCaseName(string name)
        {
            if (IsAllUpperCase(name))
            {
                return name;
            }
            for (int i = name.Length - 1; i > 0; i--)
            {
                if (name[i] >= 'A' && name[i] <= 'Z')
                {
                    name = name.Substring(0, i) + " " + name.Substring(i);
                }
            }
            return name;
        }

        public static bool IsAllUpperCase(string str)
        {
            return str.All(c => c >= 'A' && c <= 'Z');
        }
    }
}
