using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inedo.Extensions.Oracle.UnitTests
{
    [TestClass]
    public class TokenizerTests
    {
        private const string SimpleStatement = "select * from \"TestTable\"";
        private const string SimpleStatement2 = "select \\'hello','dears' from \"TestTable\"";
        private const string Strings = "'it''s a string' 'and another' q'(hello's ) test)' q'[hello's ] test]' q'Ahello's A testA'";
        private const string Comments = @"stuff --comment
/* block comment *
*/
REM remark
REMARK remark2
end";
        private const string Labels = "<<label1>>\n<<label2>>";
        private const string Operators = "IF a<=b OR a = b";

        [TestMethod]
        public void TestSimpleStatement()
        {
            CollectionAssert.AreEqual(
                new[] { "select", " *", " from", " \"TestTable\"" },
                Tokenizer.GetTokens(SimpleStatement).ToArray()
            );
        }

        [TestMethod]
        public void TestSimpleStatement2()
        {
            CollectionAssert.AreEqual(
                new[] { "select", " ", "\\","'hello'", ",", "'dears'", " from", " \"TestTable\"" },
                Tokenizer.GetTokens(SimpleStatement2).ToArray()
            );
        }

        [TestMethod]
        public void TestStrings()
        {
            CollectionAssert.AreEqual(
                new[] { "'it''s a string'", " 'and another'", " q'(hello's ) test)'", " q'[hello's ] test]'", " q'Ahello's A testA'" },
                Tokenizer.GetTokens(Strings).ToArray()
            );
        }
        [TestMethod]
        public void TestComments()
        {
            CollectionAssert.AreEqual(
                new[] { "stuff", " --comment", "\n/* block comment *\n*/", "\nREM remark", "\nREMARK remark2", "\nend" },
                Tokenizer.GetTokens(Comments.Replace("\r", "")).ToArray()
            );
        }
        [TestMethod]
        public void TestLabels()
        {
            CollectionAssert.AreEqual(
                new[] { "<<label1>>", "\n<<label2>>" },
                Tokenizer.GetTokens(Labels).ToArray()
            );
        }
        [TestMethod]
        public void TestOperators()
        {
            var rubbish = Tokenizer.GetTokens(Operators).ToArray();
            CollectionAssert.AreEqual(
                new[] { "IF", " a", "<=", "b", " OR", " a", " =", " b"  },
                Tokenizer.GetTokens(Operators).ToArray()
            );
        }
        [TestMethod]
        public void TestWhitespace()
        {
            CollectionAssert.AreEqual(
                new string[0],
                Tokenizer.GetTokens("           \n\t    ").ToArray()
            );
        }
    }
}
