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
            try
            {
                var view = database.OpenView("SELECT `Format`, `PropertyId` FROM `VitaCurrentTimestampFetcher`");
                view.Execute();

                foreach (var row in view)
                {
                    var format = row["Format"].ToString();
                    var propertyId = row["PropertyId"].ToString();
                    var timeSpan = DateTime.Now - new DateTime(1970, 1, 1);
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
