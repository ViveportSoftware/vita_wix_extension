using System;
using Microsoft.Deployment.WindowsInstaller;

namespace Htc.Vita.Wix.CustomAction
{
    internal class CurrentTimestampFetcherExecutor : AbstractActionExecutor
    {
        public CurrentTimestampFetcherExecutor(Session session) : base("CurrentTimestampFetcherExecutor", session)
        {
        }

        protected override ActionResult OnExecute()
        {
            var database = Session.Database;
            if (!database.Tables.Contains("VitaCurrentTimestampFetcher"))
            {
                return ActionResult.Success;
            }

            try
            {
                var view = database.OpenView("SELECT `Format`, `PropertyId`, `AsUtc` FROM `VitaCurrentTimestampFetcher`");
                view.Execute();

                foreach (var row in view)
                {
                    var currentTime = DateTime.Now;
                    var unixStartTime = new DateTime(1970, 1, 1);
                    var format = row["Format"].ToString();
                    var propertyId = row["PropertyId"].ToString();
                    int asUtc;
                    int.TryParse(row["AsUtc"].ToString(), out asUtc);
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
            finally
            {
                database.Close();
            }
            return ActionResult.Success;
        }
    }
}
