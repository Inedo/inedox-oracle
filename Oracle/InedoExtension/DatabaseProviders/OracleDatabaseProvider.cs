using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Inedo.Data;
using Inedo.Diagnostics;
using Inedo.Extensibility.DatabaseConnections;
using Inedo.Extensions.Oracle.Properties;
using Inedo.Serialization;
using Oracle.ManagedDataAccess.Client;

namespace Inedo.Extensions.Oracle
{
    [DisplayName("Oracle")]
    [Description("Database provider for Oracle 9i and later.")]
    [PersistFrom("Inedo.BuildMasterExtensions.Oracle.OracleDatabaseProvider,Oracle")]
    public sealed class OracleDatabaseProvider : DatabaseConnection, IChangeScriptExecuter
    {
        private static readonly Task Complete = Task.FromResult<object>(null);

        public int MaxChangeScriptVersion => 1;

        public override Task ExecuteQueryAsync(string query, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(query))
                return Complete;

            var splitQueries = new List<string>(ScriptSplitter.Process(query)).ToArray();
            this.LogDebug($"Split into {splitQueries.Length} queries.");
            
            using (var conn = new OracleConnection(this.ConnectionString))
            {
                conn.Open();

                using (var cmd = new OracleCommand(string.Empty, conn))
                {
                    for (int i = 0; i < splitQueries.Length; i++)
                    {
                        var splitQuery = splitQueries[i];
                        this.LogDebug($"Executing Query {i + 1}...");
                        cmd.CommandText = splitQuery.Replace("\r", "");
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            return Complete;
        }

        public Task ExecuteChangeScriptAsync(ChangeScriptId scriptId, string scriptName, string scriptText, CancellationToken cancellationToken)
        {
            var state = this.GetStateAsync(cancellationToken).Result;
            if (!state.IsInitialized)
                throw new InvalidOperationException("Database is not initialized.");

            if (state.Scripts.Any(s => s.Id.ScriptId == scriptId.ScriptId))
            {
                this.LogInformation(scriptName + " already executed. Skipping...");
                return Complete;
            }

            Exception ex = null;
            try
            {
                this.ExecuteQueryAsync(scriptText, cancellationToken);
                this.LogInformation(scriptName + " executed successfully.");
            }
            catch (Exception _ex)
            {
                ex = _ex;
                this.LogError(scriptName + " failed: " + ex.Message);
            }

            this.ExecuteQueryAsync(
                string.Format(
                    "INSERT INTO \"__BuildMaster_DbSchemaChanges\" "
                    + " (\"Numeric_Release_Number\", \"Script_Id\", \"Script_Sequence\", \"Script_Name\", \"Executed_Date\", \"Success_Indicator\") "
                    + "VALUES "
                    + "({0}, {1}, \"__BuildMaster_DbSchema_Seq\".nextval, '{2}', CURRENT_DATE, '{3}')",
                    scriptId.LegacyReleaseSequence,
                    scriptId.ScriptId,
                    scriptName.Replace("'", "''"),
                    ex == null ? "Y" : "N"
                ),
                cancellationToken
            );

            return Complete;
        }
        public Task<ChangeScriptState> GetStateAsync(CancellationToken cancellationToken)
        {
            bool initialized = this.ExecuteScalar("SELECT COUNT(*) FROM tabs WHERE \"TABLE_NAME\" = '__BuildMaster_DbSchemaChanges'") > 0;
            if (!initialized)
                return Task.FromResult(new ChangeScriptState(false));

            var scripts = new List<ChangeScriptExecutionRecord>();
            var table = this.ExecuteDataTable("SELECT * FROM \"__BuildMaster_DbSchemaChanges\"");
            foreach (DataRow row in table.Rows)
            {
                scripts.Add(
                    new ChangeScriptExecutionRecord(
                        new ChangeScriptId(Convert.ToInt32(row["Script_Id"]), Convert.ToInt64(row["Numeric_Release_Number"])),
                        (string)row["Script_Name"],
                        (DateTime)row["Executed_Date"],
                        (YNIndicator)(string)row["Success_Indicator"]
                    )
                );
            }

            return Task.FromResult(new ChangeScriptState(1, scripts));
        }
        public Task InitializeDatabaseAsync(CancellationToken cancellationToken)
        {
            var state = this.GetStateAsync(cancellationToken).Result;
            if (state.IsInitialized)
                return Complete;

            using (var conn = new OracleConnection(this.ConnectionString))
            {
                conn.Open();

                using (var cmd = new OracleCommand(Resources.Initialize1, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new OracleCommand(Resources.Initialize2, conn))
                {
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = new OracleCommand(Resources.Initialize3, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            return Complete;
        }
        public Task UpgradeSchemaAsync(IReadOnlyDictionary<int, Guid> canoncialGuids, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        private DataTable ExecuteDataTable(string sqlCommand)
        {
            using (var conn = new OracleConnection(this.ConnectionString))
            {
                conn.Open();

                using (var cmd = new OracleCommand(sqlCommand, conn))
                {
                    var table = new DataTable();
                    table.Load(cmd.ExecuteReader());
                    return table;
                }
            }
        }
        private int ExecuteScalar(string sqlCommand)
        {
            using (var conn = new OracleConnection(this.ConnectionString))
            {
                conn.Open();

                using (var cmd = new OracleCommand(sqlCommand, conn))
                {
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }
    }
}
