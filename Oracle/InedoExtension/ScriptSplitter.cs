using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Inedo.Extensions.Oracle
{
    internal static class ScriptSplitter
    {
        private static readonly Regex Begin = new Regex(@"\G\s*BEGIN\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex End = new Regex(@"\G\s*END\b", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.Singleline);
        private static readonly Regex Semicolon = new Regex(@"\G(\s*;)", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture);
        private static readonly Regex StatementDelimiter = new Regex(@"\G(\s*;)|(\s*/\s*\n)", RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.ExplicitCapture);

        public static IEnumerable<string> Process(string scriptText)
        {
            var buffer = new StringBuilder();
            var tokens = Tokenizer.GetTokens(scriptText.Replace("\r", string.Empty));
            int nestingLevel = 0;
            bool lastTokenWasEnd = false;

            foreach (var token in tokens)
            {
                if (Begin.IsMatch(token))
                {
                    nestingLevel++;
                    buffer.Append(token);
                    lastTokenWasEnd = false;
                }
                else if (End.IsMatch(token))
                {
                    nestingLevel--;
                    buffer.Append(token);
                    lastTokenWasEnd = true;
                }
                else if (nestingLevel <= 0 && StatementDelimiter.IsMatch(token) && ((!lastTokenWasEnd) || (lastTokenWasEnd && !Semicolon.IsMatch(token))))
                {
                    var script = buffer.ToString().Trim();
                    if (script != string.Empty)
                        yield return script;

                    buffer.Length = 0;
                    lastTokenWasEnd = false;
                }
                else
                {
                    buffer.Append(token);
                    lastTokenWasEnd = false;
                }
            }

            var lastScript = buffer.ToString().Trim();
            if (lastScript != string.Empty)
                yield return lastScript;
        }
    }
}
