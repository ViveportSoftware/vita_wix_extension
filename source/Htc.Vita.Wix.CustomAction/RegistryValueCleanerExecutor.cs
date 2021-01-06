using System;
using System.Collections.Generic;
using Microsoft.Deployment.WindowsInstaller;
using Microsoft.Win32;

namespace Htc.Vita.Wix.CustomAction
{
    internal class RegistryValueCleanerExecutor
    {
        private const string KeyNameName = "Name";
        private const string KeyNamePath = "Path";
        private const string KeyNameScope = "Scope";
        private const string PrefixName = KeyNameName + "_";
        private const string PrefixPath = KeyNamePath + "_";
        private const string PrefixScope = KeyNameScope + "_";

        internal class Deferred : AbstractActionExecutor
        {
            private const string ProfileListPath = "SOFTWARE\\Microsoft\\Windows NT\\CurrentVersion\\ProfileList";

            private readonly Dictionary<string, string> _registryValueNames = new Dictionary<string, string>();
            private readonly Dictionary<string, string> _registryValuePaths = new Dictionary<string, string>();
            private readonly Dictionary<string, string> _registryValueScopes = new Dictionary<string, string>();

            public Deferred(Session session) : base("RegistryValueCleanerExecutor.Deferred", session)
            {
            }

            protected override ActionResult OnExecute()
            {
                var customActionData = Session.CustomActionData;
                foreach (var item in customActionData)
                {
                    try
                    {
                        CleanRegistryValue(item.Key, item.Value);
                    }
                    catch (Exception e)
                    {
                        Log($"CleanRegistryValue error: {e}");
                    }
                }
                return ActionResult.Success;
            }

            private void CleanRegistryValue(string key, string value)
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

                    if (_registryValuePaths.ContainsKey(id) && _registryValueNames.ContainsKey(id))
                    {
                        DeleteRegistryValue(value, _registryValuePaths[id], _registryValueNames[id]);
                    }
                    else if (!_registryValueScopes.ContainsKey(id))
                    {
                        _registryValueScopes.Add(id, value);
                    }
                    else
                    {
                        Log($"Duplicate registry root id: {id}, value: {value}");
                    }
                }
                if (key.StartsWith(PrefixPath))
                {
                    var id = key.Substring(PrefixPath.Length);
                    if (string.IsNullOrEmpty(id))
                    {
                        return;
                    }

                    if (_registryValueScopes.ContainsKey(id) && _registryValueNames.ContainsKey(id))
                    {
                        DeleteRegistryValue(_registryValueScopes[id], value, _registryValueNames[id]);
                    }
                    else if (!_registryValuePaths.ContainsKey(id))
                    {
                        _registryValuePaths.Add(id, value);
                    }
                    else
                    {
                        Log($"Duplicate registry path id: {id}, value: {value}");
                    }
                }
                if (key.StartsWith(PrefixName))
                {
                    var id = key.Substring(PrefixName.Length);
                    if (string.IsNullOrEmpty(id))
                    {
                        return;
                    }

                    if (_registryValueScopes.ContainsKey(id) && _registryValuePaths.ContainsKey(id))
                    {
                        DeleteRegistryValue(_registryValueScopes[id], _registryValuePaths[id], value);
                    }
                    else if (!_registryValueNames.ContainsKey(id))
                    {
                        _registryValueNames.Add(id, value);
                    }
                    else
                    {
                        Log($"Duplicate registry name id: {id}, value: {value}");
                    }
                }
            }

            private void DeleteRegistryValue(string scope, string path, string name)
            {
                var registryScope = ConvertToScope(scope);
                if (registryScope == Scope.LocalMachine)
                {
                    DeleteRegistryValueInLocalMachine(path, name);
                }
                else if (registryScope == Scope.CurrentUser)
                {
                    DeleteRegistryValueInCurrentUser(path, name);
                }
                else if (registryScope == Scope.EachUser)
                {
                    DeleteRegistryValueInEachUser(path, name);
                }
                else
                {
                    Log($"Can not determine Windows registry scope: {scope}");
                }
            }

            private void DeleteRegistryValueInCurrentUser(string path, string name)
            {
                if (string.IsNullOrEmpty(path) || name == null)
                {
                    return;
                }

                try
                {
                    using (var hkcuSubKey = Registry.CurrentUser.OpenSubKey(path, true))
                    {
                        if (hkcuSubKey != null)
                        {
                            hkcuSubKey.DeleteValue(name);
                            Log($"Registry value \"{name}\" under \"HKCU\\{path}\" is deleted");
                        }
                    }
                }
                catch (ArgumentException)
                {
                    Log($"Can not find registry value \"{name}\" under \"HKCU\\{path}\". Skipped");
                }
            }

            private void DeleteRegistryValueInEachUser(string path, string name)
            {
                if (string.IsNullOrEmpty(path) || name == null)
                {
                    return;
                }

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
                            using (var hkuSubKey = Registry.Users.OpenSubKey(target, true))
                            {
                                if (hkuSubKey != null)
                                {
                                    hkuSubKey.DeleteValue(name);
                                    Log($"Registry value \"{name}\" under \"HKU\\{target}\" is deleted");
                                }
                            }
                        }
                        catch (ArgumentException)
                        {
                            Log($"Can not find registry value \"{name}\" under \"HKU\\{target}\". Skipped");
                        }
                    }
                }
            }

            private void DeleteRegistryValueInLocalMachine(string path, string name)
            {
                if (string.IsNullOrEmpty(path) || name == null)
                {
                    return;
                }

                try
                {
                    using (var hklmSubKey = Registry.LocalMachine.OpenSubKey(path, true))
                    {
                        if (hklmSubKey != null)
                        {
                            hklmSubKey.DeleteValue(name);
                            Log($"Registry value \"{name}\" under \"HKLM\\{path}\" is deleted");
                        }
                    }
                }
                catch (ArgumentException)
                {
                    Log($"Can not find registry value \"{name}\" under \"HKLM\\{path}\". Skipped");
                }
                catch (Exception e)
                {
                    Log($"Can not delete registry value \"{name}\" under \"HKLM\\{path}\". {e}");
                }
                try
                {
                    using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64))
                    {
                        using (var hklm64SubKey = baseKey.OpenSubKey(path, true))
                        {
                            if (hklm64SubKey != null)
                            {
                                hklm64SubKey.DeleteValue(name);
                                Log($"Registry value \"{name}\" under 64-bit view of \"HKLM\\{path}\" is deleted");
                            }
                        }
                    }
                }
                catch (ArgumentException)
                {
                    Log($"Can not find registry value \"{name}\" under 64-bit view of \"HKLM\\{path}\". Skipped");
                }
                catch (Exception e)
                {
                    Log($"Can not delete registry value \"{name}\" under 64-bit view of \"HKLM\\{path}\". {e}");
                }
                try
                {
                    using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32))
                    {
                        using (var hklm32SubKey = baseKey.OpenSubKey(path, true))
                        {
                            if (hklm32SubKey != null)
                            {
                                hklm32SubKey.DeleteValue(name);
                                Log($"Registry value \"{name}\" under 32-bit view of \"HKLM\\{path}\" is deleted");
                            }
                        }
                    }
                }
                catch (ArgumentException)
                {
                    Log($"Can not find registry value \"{name}\" under 32-bit view of \"HKLM\\{path}\". Skipped");
                }
                catch (Exception e)
                {
                    Log($"Can not delete registry value \"{name}\" under 32-bit view of \"HKLM\\{path}\". {e}");
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
                Log($"Can not convert Windows registry scope {scope}. Use LocalMachine as fallback scope");
                return Scope.LocalMachine;
            }
        }

        internal class Immediate : AbstractActionExecutor
        {
            private const string TableName = "VitaRegistryValueCleaner";

            public Immediate(Session session) : base("RegistryValueCleanerExecutor.Immediate", session)
            {
            }

            protected override ActionResult OnExecute()
            {
                var database = Session.Database;
                if (!database.Tables.Contains(TableName))
                {
                    return ActionResult.Success;
                }

                try
                {
                    using (var view = database.OpenView($"SELECT `{KeyNameScope}`, `{KeyNamePath}`, `{KeyNameName}` FROM `{TableName}`"))
                    {
                        view.Execute();

                        var customActionData = new CustomActionData();
                        foreach (var row in view)
                        {
                            var index = Math.Abs(($"{row[KeyNameScope]}_{row[KeyNamePath]}_{row[KeyNameName]}").GetHashCode());
                            customActionData[PrefixScope + index] = row[KeyNameScope].ToString();
                            customActionData[PrefixPath + index] = row[KeyNamePath].ToString();
                            customActionData[PrefixName + index] = row[KeyNameName].ToString();
                        }

                        Session["Vita_RegistryValueCleanerDeferred"] = customActionData.ToString();
                    }
                }
                finally
                {
                    database.Close();
                }
                return ActionResult.Success;
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
