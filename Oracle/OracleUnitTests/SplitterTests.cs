using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Inedo.Extensions.Oracle.UnitTests
{
    [TestClass]
    public class SplitterTests
    {
        private const string SimpleStatement = "select * from \"TestTable\"";
        private const string CreateDrop = @"create table t (number a, varchar2(10) );
drop table t;";
        private const string PlSqlBlock = @"begin
    select 1 from dual;
    select 1000 from dual;
end
";
        private const string NullPilsqlblock = "BEGIN\n  NULL;\nEND;";


        [TestMethod]
        public void TestPilsql_SingleStatement()
        {
            CollectionAssert.AreEqual(
                new[] { NullPilsqlblock },
                ScriptSplitter.Process(NullPilsqlblock).ToArray()
            );
        }

        [TestMethod]
        public void TestPilsql_MultipleStatement_NoDelim()
        {
            CollectionAssert.AreEqual(
                new[] { NullPilsqlblock + "\n\n" + NullPilsqlblock },
                ScriptSplitter.Process(NullPilsqlblock + "\n\n" + NullPilsqlblock).ToArray()
            );
        }

        [TestMethod]
        public void TestPilsql_MultipleStatement_SemicolonDelim()
        {
            CollectionAssert.AreEqual(
                new[] { NullPilsqlblock, NullPilsqlblock },
                ScriptSplitter.Process(NullPilsqlblock + "\n;\n" + NullPilsqlblock).ToArray()
            );
        }

        [TestMethod]
        public void TestPilsql_MultipleStatement_SlashDelim()
        {
            CollectionAssert.AreEqual(
                new[] { NullPilsqlblock, NullPilsqlblock },
                ScriptSplitter.Process(NullPilsqlblock + "\n/\n" + NullPilsqlblock).ToArray()
            );
        }

        [TestMethod]
        public void TestPilsql_MultipleStatement_ManyDelim()
        {
            CollectionAssert.AreEqual(
                new[] { NullPilsqlblock, NullPilsqlblock },
                ScriptSplitter.Process(NullPilsqlblock + "\n/\n" + "\n/\n" + "\n;\n" + "\n/\n" + NullPilsqlblock).ToArray()
            );
        }

        [TestMethod]
        public void TestSimpleStatement()
        {
            CollectionAssert.AreEqual(
                new[] { SimpleStatement },
                ScriptSplitter.Process(SimpleStatement).ToArray()
            );
        }
        [TestMethod]
        public void TestCreateDrop()
        {
            CollectionAssert.AreEqual(
                new[] { "create table t (number a, varchar2(10) )", "drop table t" },
                ScriptSplitter.Process(CreateDrop).ToArray()
            );
        }
        [TestMethod]
        public void TestPlSqlBlock()
        {
            CollectionAssert.AreEqual(
                new[] { PlSqlBlock.Replace("\r", "").Trim() },
                ScriptSplitter.Process(PlSqlBlock).ToArray()
            );
        }
        [TestMethod]
        public void TestCreateDropPlSqlBlock()
        {
            CollectionAssert.AreEqual(
                new[] { "create table t (number a, varchar2(10) )", "drop table t", PlSqlBlock.Replace("\r", "").Trim() },
                ScriptSplitter.Process(CreateDrop + "\n" + PlSqlBlock).ToArray()
            );
        }
    }
}
