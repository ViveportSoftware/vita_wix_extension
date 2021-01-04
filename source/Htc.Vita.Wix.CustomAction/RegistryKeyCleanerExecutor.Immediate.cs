using System;
using Microsoft.Deployment.WindowsInstaller;

namespace Htc.Vita.Wix.CustomAction
{
    internal partial class RegistryKeyCleanerExecutor
    {
        private const string PrefixPath = "Path_";
        private const string PrefixScope = "Scope_";

        internal class Immediate : AbstractActionExecutor
        {
            public Immediate(Session session) : base("RegistryKeyCleanerExecutor.Immediate", session)
            {
            }

            protected override ActionResult OnExecute()
            {
                var database = Session.Database;
                if (!database.Tables.Contains("VitaRegistryKeyCleaner"))
                {
                    return ActionResult.Success;
                }

                try
                {
                    var view = database.OpenView("SELECT `Scope`, `Path` FROM `VitaRegistryKeyCleaner`");
                    view.Execute();

                    var customActionData = new CustomActionData();
                    foreach (var row in view)
                    {
                        var index = Math.Abs(("" + row["Scope"] + "_" + row["Path"]).GetHashCode());
                        customActionData[PrefixScope + index] = row["Scope"].ToString();
                        customActionData[PrefixPath + index] = row["Path"].ToString();
                    }

                    Session["Vita_RegistryKeyCleanerDeferred"] = customActionData.ToString();
                }
                finally
                {
                    database.Close();
                }
                return ActionResult.Success;
            }
        }
    }
}
