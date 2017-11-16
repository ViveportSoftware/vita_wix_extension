using Microsoft.Deployment.WindowsInstaller;

namespace Htc.Vita.Wix.CustomAction
{
    internal partial class ServiceManagerExecutor
    {
        internal class Immediate : AbstractActionExecutor
        {
            public Immediate(Session session) : base("ServiceManagerExecutor.Immediate", session)
            {
            }

            protected override ActionResult OnExecute()
            {
                var database = Session.Database;
                try
                {
                    var view = database.OpenView("SELECT `Name`, `StartType` FROM `VitaServiceManager`");
                    view.Execute();

                    var customActionData = new CustomActionData();
                    foreach (var row in view)
                    {
                        customActionData[row["Name"].ToString()] = row["StartType"].ToString();
                    }

                    Session["Vita_ServiceManagerDeferred"] = customActionData.ToString();
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
