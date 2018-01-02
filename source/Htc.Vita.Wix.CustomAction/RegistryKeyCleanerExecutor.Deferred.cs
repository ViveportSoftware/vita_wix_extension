using System;
using System.Collections.Generic;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;

namespace Htc.Vita.Wix.CustomAction
{
    internal partial class RegistryKeyCleanerExecutor
    {
        internal class Deferred : AbstractActionExecutor
        {
            private const string ProfileListPath = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProfileList";

            private readonly Dictionary<string, string> _registryKeyPaths = new Dictionary<string, string>();
            private readonly Dictionary<string, string> _registryKeyScopes = new Dictionary<string, string>();

            public Deferred(Session session) : base("RegistryKeyCleanerExecutor.Deferred", session)
            {
            }

            protected override ActionResult OnExecute()
            {
                var customActionData = Session.CustomActionData;
                foreach (var item in customActionData)
                {
                    CleanRegistryKey(item.Key, item.Value);
                }
                return ActionResult.Success;
            }

            private void CleanRegistryKey(string key, string value)
            {
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                {
                    return;
                }

                if (key.StartsWith(PrefixScope))
                {
                    var id = key.Substring(PrefixScope.Length);
                    if (string.IsNullOrEmpty(id))
                    {
                        return;
                    }

                    if (_registryKeyPaths.ContainsKey(id))
                    {
                        DeleteRegistryKey(value, _registryKeyPaths[id]);
                    }
                    else if (!_registryKeyScopes.ContainsKey(id))
                    {
                        _registryKeyScopes.Add(id, value);
                    }
                    else
                    {
                        Log("Duplicate registry root id: " + id + ", value: " + value);
                    }
                }
                if (key.StartsWith(PrefixPath))
                {
                    var id = key.Substring(PrefixPath.Length);
                    if (string.IsNullOrEmpty(id))
                    {
                        return;
                    }

                    if (_registryKeyScopes.ContainsKey(id))
                    {
                        DeleteRegistryKey(_registryKeyScopes[id], value);
                    }
                    else if (!_registryKeyPaths.ContainsKey(id))
                    {
                        _registryKeyPaths.Add(id, value);
                    }
                    else
                    {
                        Log("Duplicate registry path id: " + id + ", value: " + value);
                    }
                }
            }

            private void DeleteRegistryKey(string scope, string path)
            {
                var registryScope = ConvertToScope(scope);
                if (registryScope == Scope.LocalMachine)
                {
                    DeleteRegistryKeyInLocalMachine(path);
                }
                else if (registryScope == Scope.CurrentUser)
                {
                    DeleteRegistryKeyInCurrentUser(path);
                }
                else if (registryScope == Scope.EachUser)
                {
                    DeleteRegistryKeyInEachUser(path);
                }
                else
                {
                    Log("Can not determine Windows registry scope: " + scope);
                }
            }

            private void DeleteRegistryKeyInCurrentUser(string path)
            {
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                try
                {
                    Registry.CurrentUser.DeleteSubKeyTree(path);
                    Log("Registry key \"HKCU\\" + path + "\" is deleted");
                }
                catch (ArgumentException)
                {
                    Log("Can not find registry key \"HKCU\\" + path + "\". Skipped");
                }
            }

            private void DeleteRegistryKeyInEachUser(string path)
            {
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                try
                {
                    using (var profileList = Registry.LocalMachine.OpenSubKey(ProfileListPath)) // the same in 32-bit and 64-bit mode
                    {
                        if (profileList == null)
                        {
                            return;
                        }
                        foreach (var userSid in profileList.GetSubKeyNames())
                        {
                            var target = userSid + "\\" + path;
                            try
                            {
                                Registry.Users.DeleteSubKeyTree(target);
                                Log("Registry key \"HKU\\" + target + "\" is deleted");
                            }
                            catch (ArgumentException)
                            {
                                Log("Can not find registry key \"HKU\\" + target + "\". Skipped");
                            }
                        }
                    }
                }
                catch (ArgumentException)
                {
                    Log("Can not find registry key under \"HKU\\");
                }
            }

            private void DeleteRegistryKeyInLocalMachine(string path)
            {
                if (string.IsNullOrEmpty(path))
                {
                    return;
                }

                try
                {
                    Registry.LocalMachine.DeleteSubKeyTree(path);
                    Log("Registry key \"HKLM\\" + path + "\" is deleted");
                }
                catch (ArgumentException)
                {
                    Log("Can not find registry key \"HKLM\\" + path + "\". Skipped");
                }
                try
                {
                    using (var hklm64Key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                    {
                        hklm64Key.DeleteSubKeyTree(path);
                        Log("Registry key \"HKLM\\" + path + "\" under 64-bit view is deleted");
                    }
                }
                catch (ArgumentException)
                {
                    Log("Can not find registry key \"HKLM\\" + path + "\" under 64-bit view. Skipped");
                }
                try
                {
                    using (var hklm32Key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                    {
                        hklm32Key.DeleteSubKeyTree(path);
                        Log("Registry key \"HKLM\\" + path + "\" under 32-bit view is deleted");
                    }
                }
                catch (ArgumentException)
                {
                    Log("Can not find registry key \"HKLM\\" + path + "\" under 32-bit view. Skipped");
                }
            }

            private Scope ConvertToScope(string scope)
            {
                if ("LocalMachine".Equals(scope))
                {
                    return Scope.LocalMachine;
                }
                if ("CurrentUser".Equals(scope))
                {
                    return Scope.CurrentUser;
                }
                if ("EachUser".Equals(scope))
                {
                    return Scope.EachUser;
                }
                Log("Can not convert Windows registry scope " + scope + ". Use LocalMachine as fallback scope");
                return Scope.LocalMachine;
            }
        }

        internal enum Scope
        {
            LocalMachine = 0,
            CurrentUser = 1,
            EachUser = 2
        }
    }
}
