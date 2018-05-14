using Inedo.BuildMaster.Extensibility.DatabaseConnections;
using Inedo.BuildMaster.Web.Controls.Extensions;
using Inedo.Web.Controls;

namespace Inedo.Extensions.Oracle
{
    internal sealed class OracleDatabaseProviderEditor : DatabaseConnectionEditorBase
    {
        private ValidatingTextBox txtConnectionString;

        public override void BindToForm(DatabaseConnection extension)
        {
            var p = (OracleDatabaseProvider)extension;
            txtConnectionString.Text = p.ConnectionString;
        }
        public override DatabaseConnection CreateFromForm()
        {
            return new OracleDatabaseProvider
            {
                ConnectionString = txtConnectionString.Text
            };
        }

        protected override void CreateChildControls()
        {
            this.txtConnectionString = new ValidatingTextBox { Required = true };

            this.Controls.Add(
                new SlimFormField("Connection string:", this.txtConnectionString)
                {
                    HelpText = "The connection string to the Oracle database. The standard format for this is:<br /><br />"
                    + "<em>DATA SOURCE=myServerAddress; USER ID=myUsername; PASSWORD=myPassword</em>"
                }
            );
        }
    }
}
