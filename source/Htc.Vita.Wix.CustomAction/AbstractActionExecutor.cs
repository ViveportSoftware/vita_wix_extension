using System;
using Microsoft.Deployment.WindowsInstaller;

namespace Htc.Vita.Wix.CustomAction
{
    internal abstract class AbstractActionExecutor
    {
        private readonly string _name;
        protected readonly Session Session;

        protected AbstractActionExecutor(string name, Session session)
        {
            _name = name;
            Session = session;
        }

        public ActionResult Execute()
        {
            Log("Begin " + _name);
            var result = ActionResult.Failure;
            try
            {
                result = OnExecute();
            }
            catch (Exception e)
            {
                Log("Error on custom action " + _name + ": " + e.Message);
            }
            Log("End " + _name);
            return result;
        }

        protected void Log(string message)
        {
            Session.Log(_name + ": " + message);
        }

        protected abstract ActionResult OnExecute();
    }
}
