using Microsoft.Deployment.WindowsInstaller;

namespace Htc.Vita.Wix.CustomAction
{
    public class CustomActions
    {
        [CustomAction]
        public static ActionResult ServiceManageDeferred(Session session)
        {
            session.Log("Begin ServiceManageDeferred");
            var result = new ServiceManagerExecutor.Deferred(session).Execute();
            session.Log("End ServiceManageDeferred");
            return result;
        }

        [CustomAction]
        public static ActionResult ServiceManageImmediate(Session session)
        {
            session.Log("Begin ServiceManageImmediate");
            var result = new ServiceManagerExecutor.Immediate(session).Execute();
            session.Log("End ServiceManageImmediate");
            return result;
        }

        [CustomAction]
        public static ActionResult SidTranslate(Session session)
        {
            session.Log("Begin SidTranslate");
            var result = new SidTranslatorExecutor(session).Execute();
            session.Log("End SidTranslate");
            return result;
        }
    }
}
