using System;
using Microsoft.Deployment.WindowsInstaller;

namespace Htc.Vita.Wix.CustomAction
{
    internal class CurrentTimestampFetcherExecutor : AbstractActionExecutor
    {
        private const string KeyNameAsUtc = "AsUtc";
        private const string KeyNameFormat = "Format";
        private const string KeyNamePropertyId = "PropertyId";
        private const string TableName = "VitaCurrentTimestampFetcher";

        public CurrentTimestampFetcherExecutor(Session session) : base("CurrentTimestampFetcherExecutor", session)
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
                using (var view = database.OpenView($"SELECT `{KeyNameFormat}`, `{KeyNamePropertyId}`, `{KeyNameAsUtc}` FROM `{TableName}`"))
                {
                    view.Execute();

                    foreach (var row in view)
                    {
                        var currentTime = DateTime.Now;
                        var unixStartTime = new DateTime(1970, 1, 1);
                        var format = row[KeyNameFormat].ToString();
                        var propertyId = row[KeyNamePropertyId].ToString();
                        int asUtc;
                        int.TryParse(row[KeyNameAsUtc].ToString(), out asUtc);
                        var timeSpan = currentTime - unixStartTime;
                        if (asUtc == 1)
                        {
                            timeSpan = currentTime.ToUniversalTime() - unixStartTime;
                        }

                        Session[propertyId] = "" + (long)timeSpan.TotalSeconds;
                        if ("InMilliSec".Equals(format))
                        {
                            Session[propertyId] = "" + (long)timeSpan.TotalMilliseconds;
                        }
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
