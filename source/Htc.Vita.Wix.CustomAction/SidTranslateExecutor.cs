using System;
using System.Security.Principal;
using Microsoft.Deployment.WindowsInstaller;

namespace Htc.Vita.Wix.CustomAction
{
    internal class SidTranslateExecutor : AbstractActionExecutor
    {
        public SidTranslateExecutor(Session session) : base("SidTranslate", session)
        {
        }

        protected override ActionResult OnExecute()
        {
            var database = Session.Database;
            var view = database.OpenView("SELECT `Sid`, `PropertyId` FROM `VitaSidTranslate`");
            view.Execute();

            foreach (var row in view)
            {
                var sid = row["Sid"].ToString();
                var localizedName = new SecurityIdentifier(sid).Translate(typeof(NTAccount)).ToString();
                Session[sid] = localizedName;
            }
            return ActionResult.Success;
        }
    }
}
