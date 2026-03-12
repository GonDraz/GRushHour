using System;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace GonDraz.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        ///     Converts a string to a Unity Color. Returns white if parsing fails.
        ///     (Chuyển string sang Color. Nếu không hợp lệ trả về trắng)
        /// </summary>
        /// <param name="colorString"></param>
        /// <returns></returns>
        public static Color ToColor(this string colorString)
        {
            return ColorUtility.TryParseHtmlString(colorString, out var color) ? color : Color.white;
        }

        /// <summary>
        ///     Converts a string to int. Returns defaultValue if parsing fails.
        ///     (Chuyển string sang int, trả về giá trị mặc định nếu lỗi)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static int ToInt(this string s, int defaultValue = 0)
        {
            return int.TryParse(s, out var result) ? result : defaultValue;
        }

        /// <summary>
        ///     Converts a string to float. Returns defaultValue if parsing fails.
        ///     (Chuyển string sang float, trả về giá trị mặc định nếu lỗi)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static float ToFloat(this string s, float defaultValue = 0f)
        {
            return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
                ? result
                : defaultValue;
        }

        /// <summary>
        ///     Capitalizes the first character of the string.
        ///     (Viết hoa chữ cái đầu tiên của chuỗi)
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToTitleCase(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpper(s[0]) + s[1..];
        }

        /// <summary>
        ///     Returns a substring safely, never throws. Returns empty if out of range.
        ///     (Cắt chuỗi an toàn, trả về rỗng nếu vượt phạm vi)
        /// </summary>
        /// <param name="s"></param>
        /// <param name="start"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string SafeSubstring(this string s, int start, int length)
        {
            if (string.IsNullOrEmpty(s) || start >= s.Length) return string.Empty;
            if (start + length > s.Length) length = s.Length - start;
            return s.Substring(start, length);
        }

        /// <summary>
        ///     Checks if the string is null or empty.
        ///     (Kiểm tra chuỗi null hoặc rỗng)
        /// </summary>
        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        /// <summary>
        ///     Checks if the string is null, empty, or consists only of white-space characters.
        ///     (Kiểm tra chuỗi null, rỗng hoặc chỉ chứa khoảng trắng)
        /// </summary>
        public static bool IsNullOrWhiteSpace(this string s)
        {
            return string.IsNullOrWhiteSpace(s);
        }

        /// <summary>
        ///     Reverses the string.
        ///     (Đảo ngược chuỗi)
        /// </summary>
        public static string ReverseString(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var arr = s.ToCharArray();
            Array.Reverse(arr);
            return new string(arr);
        }

        /// <summary>
        ///     Removes all white-space characters from the string.
        ///     (Xoá tất cả ký tự khoảng trắng khỏi chuỗi)
        /// </summary>
        public static string RemoveWhiteSpaces(this string s)
        {
            return string.IsNullOrEmpty(s) ? s : string.Concat(s.Where(c => !char.IsWhiteSpace(c)));
        }

        /// <summary>
        ///     Trims the string and returns null if the result is empty.
        ///     (Trim chuỗi và trả về null nếu kết quả là rỗng)
        /// </summary>
        public static string TrimToNull(this string s)
        {
            if (s == null) return null;
            var trimmed = s.Trim();
            return trimmed.Length == 0 ? null : trimmed;
        }

        /// <summary>
        ///     Splits the string by a separator and removes empty entries.
        ///     (Tách chuỗi theo ký tự phân cách và loại bỏ phần tử rỗng)
        /// </summary>
        public static string[] SplitAndRemoveEmpty(this string s, params char[] separator)
        {
            return string.IsNullOrEmpty(s)
                ? Array.Empty<string>()
                : s.Split(separator, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        ///     Returns true if the string contains any of the specified substrings.
        ///     (Trả về true nếu chuỗi chứa bất kỳ chuỗi con nào trong danh sách)
        /// </summary>
        public static bool ContainsAny(this string s, params string[] values)
        {
            if (string.IsNullOrEmpty(s) || values == null) return false;
            return values.Any(v => !string.IsNullOrEmpty(v) && s.Contains(v));
        }

        /// <summary>
        ///     Returns true if the string equals any of the specified values (case sensitive).
        ///     (Trả về true nếu chuỗi bằng bất kỳ giá trị nào trong danh sách, phân biệt hoa thường)
        /// </summary>
        public static bool EqualsAny(this string s, params string[] values)
        {
            return values != null && values.Any(v => s == v);
        }

        /// <summary>
        ///     Returns true if the string equals any of the specified values (case insensitive).
        ///     (Trả về true nếu chuỗi bằng bất kỳ giá trị nào trong danh sách, không phân biệt hoa thường)
        /// </summary>
        public static bool EqualsAnyIgnoreCase(this string s, params string[] values)
        {
            return values != null && values.Any(v => string.Equals(s, v, StringComparison.OrdinalIgnoreCase));
        }

        public static string FormatCurrency(this int value)
        {
            return value switch
            {
                < 10000 => value.ToString("N0"),
                < 1000000 => $"{value / 1000f:0.#}K",
                _ => $"{value / 1000000f:0.#}m"
            };
        }
    }
}