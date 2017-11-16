using System;
using System.Security.Principal;
using Microsoft.Deployment.WindowsInstaller;

namespace Htc.Vita.Wix.CustomAction
{
    internal class SidTranslatorExecutor : AbstractActionExecutor
    {
        public SidTranslatorExecutor(Session session) : base("SidTranslatorExecutor", session)
        {
        }

        protected override ActionResult OnExecute()
        {
            var database = Session.Database;
            var view = database.OpenView("SELECT `Sid`, `PropertyId` FROM `VitaSidTranslator`");
            view.Execute();

            foreach (var row in view)
            {
                var sid = row["Sid"].ToString();
                var propertyId = row["PropertyId"].ToString();
                var localizedName = new SecurityIdentifier(sid).Translate(typeof(NTAccount)).ToString();
                Session[propertyId] = localizedName;
            }
            return ActionResult.Success;
        }
    }
}
