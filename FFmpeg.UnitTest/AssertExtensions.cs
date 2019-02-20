using System;
using Xunit;

namespace EmergenceGuardian.FFmpeg.UnitTests {
    public static class AssertExtensions {
        /// <summary>
        /// Evaluates whether or not the substring is contained within specified value, depending on whether contains is true (Contains) or false (DoesNotContain).
        /// </summary>
        /// <param name="expectedSubstring">The string to look for.</param>
        /// <param name="actualValue">The string to validate.</param>
        /// <param name="contains">Whether the expected substring should be present or not in the full string.</param>
        public static void ContainsOrNot(this string actualValue, string expectedSubstring, bool contains) {
            if (contains)
                Assert.Contains(expectedSubstring, actualValue);
            else
                Assert.DoesNotContain(expectedSubstring, actualValue);
        }
    }
}
