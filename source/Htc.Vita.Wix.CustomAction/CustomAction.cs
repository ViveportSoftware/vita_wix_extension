using Microsoft.Deployment.WindowsInstaller;

namespace Htc.Vita.Wix.CustomAction
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult SidTranslate(Session session)
        {
            session.Log("Begin SidTranslate");
            var result = new SidTranslateExecutor(session).Execute();
            session.Log("End SidTranslate");
            return result;
        }
    }
}
