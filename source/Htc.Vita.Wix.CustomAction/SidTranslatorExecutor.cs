using System.Security.Principal;
using Microsoft.Deployment.WindowsInstaller;

namespace Htc.Vita.Wix.CustomAction
{
    internal class SidTranslatorExecutor : AbstractActionExecutor
    {
        private const string KeyNamePropertyId = "PropertyId";
        private const string KeyNameSid = "Sid";
        private const string TableName = "VitaSidTranslator";

        public SidTranslatorExecutor(Session session) : base("SidTranslatorExecutor", session)
        {
        }

        protected override ActionResult OnExecute()
        {
            var database = Session.Database;
            if (!database.Tables.Contains(TableName))
            {
                return ActionResult.Success;
            }

            try
            {
                using (var view = database.OpenView($"SELECT `{KeyNameSid}`, `{KeyNamePropertyId}` FROM `{TableName}`"))
                {
                    view.Execute();

                    foreach (var row in view)
                    {
                        var sid = row[KeyNameSid].ToString();
                        var propertyId = row[KeyNamePropertyId].ToString();
                        var localizedName = new SecurityIdentifier(sid).Translate(typeof(NTAccount)).ToString();
                        Session[propertyId] = localizedName;
                    }
                }
            }
            finally
            {
                database.Close();
            }
            return ActionResult.Success;
        }
    }
}
