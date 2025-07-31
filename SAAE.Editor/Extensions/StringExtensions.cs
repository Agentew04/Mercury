using System.Linq;
using System.Text;

namespace SAAE.Editor.Extensions;

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
}