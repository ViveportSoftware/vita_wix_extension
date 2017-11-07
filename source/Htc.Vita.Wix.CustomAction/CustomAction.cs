using Microsoft.Deployment.WindowsInstaller;

namespace Htc.Vita.Wix.CustomAction
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult SidTranslate(Session session)
        {
            session.Log("Begin SidTranslate");



            session.Log("End SidTranslate");
            return ActionResult.Success;
        }
    }
}
