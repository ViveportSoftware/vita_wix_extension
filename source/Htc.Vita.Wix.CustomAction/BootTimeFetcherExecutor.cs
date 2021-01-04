using System;
using System.Globalization;
using Microsoft.Deployment.WindowsInstaller;

namespace Htc.Vita.Wix.CustomAction
{
    internal class BootTimeFetcherExecutor : AbstractActionExecutor
    {
        public BootTimeFetcherExecutor(Session session) : base("BootTimeFetcherExecutor", session)
        {
        }

        protected override ActionResult OnExecute()
        {
            var database = Session.Database;
            if (!database.Tables.Contains("VitaBootTimeFetcher"))
            {
                return ActionResult.Success;
            }

            try
            {
                var view = database.OpenView("SELECT `PropertyId`, `AsUtc` FROM `VitaBootTimeFetcher`");
                view.Execute();

                foreach (var row in view)
                {
                    var propertyId = row["PropertyId"].ToString();
                    int asUtc;
                    int.TryParse(row["AsUtc"].ToString(), out asUtc);
                    var bootTime = DateTime.Now.Subtract(TimeSpan.FromMilliseconds(Environment.TickCount));
                    if (asUtc == 1)
                    {
                        bootTime = DateTime.UtcNow.Subtract(TimeSpan.FromMilliseconds(Environment.TickCount));
                    }

                    Session[propertyId] = "" + bootTime.ToString(CultureInfo.InvariantCulture);
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
