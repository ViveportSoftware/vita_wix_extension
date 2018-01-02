using Microsoft.Deployment.WindowsInstaller;

namespace Htc.Vita.Wix.CustomAction
{
    public static class CustomActions
    {
        [CustomAction]
        public static ActionResult CurrentTimestampFetch(Session session)
        {
            session.Log("Begin CurrentTimestampFetch");
            var result = new CurrentTimestampFetcherExecutor(session).Execute();
            session.Log("End CurrentTimestampFetch");
            return result;
        }

        [CustomAction]
        public static ActionResult RegistryKeyCleanDeferred(Session session)
        {
            session.Log("Begin RegistryKeyCleanDeferred");
            var result = new RegistryKeyCleanerExecutor.Deferred(session).Execute();
            session.Log("End RegistryKeyCleanDeferred");
            return result;
        }

        [CustomAction]
        public static ActionResult RegistryKeyCleanImmediate(Session session)
        {
            session.Log("Begin RegistryKeyCleanImmediate");
            var result = new RegistryKeyCleanerExecutor.Immediate(session).Execute();
            session.Log("End RegistryKeyCleanImmediate");
            return result;
        }

        [CustomAction]
        public static ActionResult RegistryValueCleanDeferred(Session session)
        {
            session.Log("Begin RegistryValueCleanDeferred");
            var result = new RegistryValueCleanerExecutor.Deferred(session).Execute();
            session.Log("End RegistryValueCleanDeferred");
            return result;
        }

        [CustomAction]
        public static ActionResult RegistryValueCleanImmediate(Session session)
        {
            session.Log("Begin RegistryValueCleanImmediate");
            var result = new RegistryValueCleanerExecutor.Immediate(session).Execute();
            session.Log("End RegistryValueCleanImmediate");
            return result;
        }

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
