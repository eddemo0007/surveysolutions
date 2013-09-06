﻿
namespace WB.UI.Shared.Web
{
    using System;
    using System.IO;
    using System.Linq;

    public static class StringExtensions
    {
        public static string ToValidFileName(this string source)
        {
            return Path.GetInvalidFileNameChars()
                       .Aggregate(
                           source.Substring(0, Math.Min(source.Length, 255)), (current, c) => current.Replace(c, 'x'));
        }
    }
}
