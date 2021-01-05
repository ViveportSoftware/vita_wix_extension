using System;
using System.Globalization;
using Microsoft.Deployment.WindowsInstaller;

namespace Htc.Vita.Wix.CustomAction
{
    internal class BootTimeFetcherExecutor : AbstractActionExecutor
    {
        private const string KeyNameAsUtc = "AsUtc";
        private const string KeyNamePropertyId = "PropertyId";
        private const string TableName = "VitaBootTimeFetcher";

        public BootTimeFetcherExecutor(Session session) : base("BootTimeFetcherExecutor", session)
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
                using (var view = database.OpenView($"SELECT `{KeyNamePropertyId}`, `{KeyNameAsUtc}` FROM `{TableName}`"))
                {
                    view.Execute();

                    foreach (var row in view)
                    {
                        var propertyId = row[KeyNamePropertyId].ToString();
                        int asUtc;
                        int.TryParse(row[KeyNameAsUtc].ToString(), out asUtc);
                        var bootTime = DateTime.Now.Subtract(TimeSpan.FromMilliseconds(Environment.TickCount));
                        if (asUtc == 1)
                        {
                            bootTime = DateTime.UtcNow.Subtract(TimeSpan.FromMilliseconds(Environment.TickCount));
                        }

                        Session[propertyId] = "" + bootTime.ToString(CultureInfo.InvariantCulture);
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
