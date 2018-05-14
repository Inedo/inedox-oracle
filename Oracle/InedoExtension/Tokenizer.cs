using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Inedo.Extensions.Oracle
{
    internal static class Tokenizer
    {
        private static readonly Regex StringRegex = new Regex(@"\G\s*'(''|[^'])*'", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.Singleline);
        private static readonly Regex QuotedIdentifierRegex = new Regex(@"\G\s*""[^""]*""", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex QQuoteRegex1 = new Regex(@"\G\s*[qQ]'\(.*?\)'", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex QQuoteRegex2 = new Regex(@"\G\s*[qQ]'\[.*?\]'", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex QQuoteRegex3 = new Regex(@"\G\s*[qQ]'(.).*?\1'", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex BlockCommentRegex = new Regex(@"\G\s*/\*.*?\*/", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex LineCommentRegex = new Regex(@"\G\s*--[^\n]*", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex RemarkCommentRegex = new Regex(@"\G\s*REM(ARK)?\s+[^\n]*", RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex LabelRegex = new Regex(@"\G\s*<<.*?>>", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex ScriptSeparator = new Regex(@"\G\s*/\s*\n", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex DoubleOperators = new Regex(@"\G\s*<=|>=|=>|<>|!=", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex SingleOperators = new Regex(@"\G\s*[~!@#$%&*()_\-+=|\[\]{}:;<>,.?/]", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex EverythingElse = new Regex(@"\G\s*\w+\b", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex Whitespace = new Regex(@"\G\s+$", RegexOptions.Compiled | RegexOptions.Singleline);
        private static readonly Regex[] Regexes = new[]
            {
                StringRegex,
                QuotedIdentifierRegex,
                QQuoteRegex1,
                QQuoteRegex2,
                QQuoteRegex3,
                BlockCommentRegex,
                LineCommentRegex,
                RemarkCommentRegex,
                LabelRegex,
                ScriptSeparator,
                DoubleOperators,
                SingleOperators,
                EverythingElse
            };

        public static IEnumerable<string> GetTokens(string script)
        {
            int index = 0;

            while (index < script.Length)
            {
                if (Whitespace.IsMatch(script, index))
                    yield break;

                foreach (var regex in Regexes)
                {
                    var match = regex.Match(script, index);
                    if (match.Success)
                    {
                        yield return match.Value;
                        index += match.Length;
                        goto done;
                    }
                }

                if (index < script.Length)
                {
                    yield return script.Substring(index, 1);
                    index++;
                }

            done: ;
            }
        }
    }
}
