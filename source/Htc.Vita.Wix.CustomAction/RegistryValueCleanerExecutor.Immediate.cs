using System;
using Microsoft.Deployment.WindowsInstaller;

namespace Htc.Vita.Wix.CustomAction
{
    internal partial class RegistryValueCleanerExecutor
    {
        private const string PrefixName = "Name_";
        private const string PrefixPath = "Path_";
        private const string PrefixScope = "Scope_";

        internal class Immediate : AbstractActionExecutor
        {
            public Immediate(Session session) : base("RegistryValueCleanerExecutor.Immediate", session)
            {
            }

            protected override ActionResult OnExecute()
            {
                var database = Session.Database;
                if (!database.Tables.Contains("VitaRegistryValueCleaner"))
                {
                    return ActionResult.Success;
                }

                try
                {
                    var view = database.OpenView("SELECT `Scope`, `Path`, `Name` FROM `VitaRegistryValueCleaner`");
                    view.Execute();

                    var customActionData = new CustomActionData();
                    foreach (var row in view)
                    {
                        var index = Math.Abs(("" + row["Scope"] + "_" + row["Path"] + "_" + row["Name"]).GetHashCode());
                        customActionData[PrefixScope + index] = row["Scope"].ToString();
                        customActionData[PrefixPath + index] = row["Path"].ToString();
                        customActionData[PrefixName + index] = row["Name"].ToString();
                    }

                    Session["Vita_RegistryValueCleanerDeferred"] = customActionData.ToString();
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
