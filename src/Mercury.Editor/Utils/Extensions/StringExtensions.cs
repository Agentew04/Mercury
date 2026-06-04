using System.Linq;
using System.Text;

namespace Mercury.Editor.Extensions;

public static class StringExtensions {

    public static string Sanitize(this string str, char[] invalidChars) {
        StringBuilder sb = new();
        foreach (char c in str) {
            if (invalidChars.Contains(c)) {
                continue;
            }
            sb.Append(c);
        }

        return sb.ToString();
    }

    public static string Escape(this string s) {
        StringBuilder sb = new();
        foreach (char c in s) {
            sb.Append(c.Escape());
        }
        return sb.ToString();
    }

    public static string Escape(this char c) {
        if (char.IsControl(c)) {
            return c switch {
                '\0' => "\\0",
                '\b' => "\\b",
                '\t' => "\\t",
                '\n' => "\\n",
                '\r' => "\\r",
                _ => $"\\x{(int)c:X2}" // ex: \x1B
            };
        }
        return c.ToString();
    }
}