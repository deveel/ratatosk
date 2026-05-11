//
// Copyright (c) Antonello Provenzano and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.
//

using System.Text;

namespace Deveel.Messaging
{
    internal static class ConnectionStringParser
    {
        public static IDictionary<string, object?> Parse(string connectionString)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(connectionString, nameof(connectionString));

            var result = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            var span = connectionString.AsSpan();

            while (!span.IsEmpty)
            {
                SkipWhitespace(ref span);
                if (span.IsEmpty)
                    break;

                var key = ReadKey(ref span);
                if (key == null)
                    break;

                SkipWhitespace(ref span);

                if (!span.IsEmpty && span[0] == '=')
                {
                    span = span.Slice(1);
                    SkipWhitespace(ref span);

                    var value = ReadValue(ref span);
                    result[key] = value;
                }
                else
                {
                    result[key] = "";
                }

                SkipWhitespace(ref span);

                if (!span.IsEmpty && span[0] == ';')
                    span = span.Slice(1);
            }

            return result;
        }

        private static void SkipWhitespace(ref ReadOnlySpan<char> span)
        {
            while (!span.IsEmpty && char.IsWhiteSpace(span[0]))
                span = span.Slice(1);
        }

        private static string? ReadKey(ref ReadOnlySpan<char> span)
        {
            int end = 0;
            while (end < span.Length && span[end] != '=' && span[end] != ';' && !char.IsWhiteSpace(span[end]))
                end++;

            if (end == 0)
                return null;

            var key = span.Slice(0, end).ToString();
            span = span.Slice(end);
            return key;
        }

        private static string? ReadValue(ref ReadOnlySpan<char> span)
        {
            if (span.IsEmpty)
                return null;

            if (span[0] == '\'' || span[0] == '"')
            {
                return ReadQuotedValue(ref span, span[0]);
            }

            int end = 0;
            while (end < span.Length && span[end] != ';')
                end++;

            var value = span.Slice(0, end).ToString();
            span = span.Slice(end);
            return value.TrimEnd();
        }

        private static string? ReadQuotedValue(ref ReadOnlySpan<char> span, char quote)
        {
            span = span.Slice(1);
            var sb = new StringBuilder();

            while (!span.IsEmpty)
            {
                if (span[0] == '\\' && span.Length > 1)
                {
                    sb.Append(span[1]);
                    span = span.Slice(2);
                }
                else if (span[0] == quote)
                {
                    span = span.Slice(1);
                    return sb.ToString();
                }
                else
                {
                    sb.Append(span[0]);
                    span = span.Slice(1);
                }
            }

            return sb.ToString();
        }
    }
}
