﻿using System.Text;

namespace RoR2Randomizer.Extensions
{
    public static class StringExtensions
    {
        static readonly StringBuilder _stringBuilder = new StringBuilder();

        public static string Repeat(this string str, uint count)
        {
            _stringBuilder.Clear();

            for (uint i = 0; i < count; i++)
            {
                _stringBuilder.Append(str);
            }

            return _stringBuilder.ToString();
        }
    }
}
