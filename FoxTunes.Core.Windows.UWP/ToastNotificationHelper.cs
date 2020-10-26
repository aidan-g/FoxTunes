using FoxTunes.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

namespace FoxTunes
{
    public static class ToastNotificationHelper
    {
        public static readonly string ID = string.Format(
            "{0}.{1}",
            Publication.Company,
            Publication.Product
        );

        public static string ProcessFileName
        {
            get
            {
                return Process.GetCurrentProcess().MainModule.FileName;
            }
        }

        public static string PluginFileName
        {
            get
            {
                return typeof(ToastNotificationHelper).Assembly.Location;
            }
        }

        private static string ShortcutFileName
        {
            get
            {
                return Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "Microsoft\\Windows\\Start Menu\\Programs\\Fox Tunes.lnk"
                );
            }
        }

        public static void Install()
        {
            if (Publication.IsPortable)
            {
                //We require a special start menu shortcut for toasts to work.
                //https://docs.microsoft.com/en-us/windows/win32/shell/enable-desktop-toast-with-appusermodelid
                InstallShortcut();
            }
            InstallServer();
            NotificationActivator.Enable();
        }

        public static void Uninstall(bool uninstallShortcut)
        {
            if (uninstallShortcut)
            {
                UninstallShortcut();
            }
            UninstallServer();
            NotificationActivator.Disable();
        }

        private static void InstallShortcut()
        {
            if (string.Equals(Shell.GetShortcut(ShortcutFileName), ProcessFileName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            Shell.CreateShortcut(
                ShortcutFileName,
                ProcessFileName,
                new Dictionary<Shell.PropertyKey, string>()
                {
                    { Shell.PropertyKey.AppUserModel_ID, ID },
                    { Shell.PropertyKey.AppUserModel_ToastActivatorCLSID, NotificationActivator.ID }
                }
            );
        }

        private static void InstallServer()
        {
            using (var key = Registry.CurrentUser.CreateSubKey(NotificationActivator.RegistryPath))
            {
                if (string.Equals(key.GetValue(null) as string, PluginFileName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                key.SetValue(null, PluginFileName);
            }
        }

        private static void UninstallShortcut()
        {
            if (!File.Exists(ShortcutFileName))
            {
                return;
            }
            File.Delete(ShortcutFileName);
        }

        private static void UninstallServer()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(NotificationActivator.RegistryPath))
            {
                if (key == null)
                {
                    return;
                }
            }
            Registry.CurrentUser.DeleteSubKeyTree(NotificationActivator.RegistryPath);
        }

        public static void Invoke(Delegate method, params object[] args)
        {
            if (Application.Current == null)
            {
                //Probably shutting down, nothing can be done.
                return;
            }
            Application.Current.Dispatcher.Invoke(method, args);
        }

        [ClassInterface(ClassInterfaceType.None)]
        [ComSourceInterfaces(typeof(INotificationActivationCallback))]
        [Guid("23A5B06E-20BB-4E7E-A0AC-6982ED6A6041"), ComVisible(true)]
        public class NotificationActivator : INotificationActivationCallback
        {
            const int COOKIE_NONE = -1;

            public static readonly Lazy<IUserInterface> UserInterface = new Lazy<IUserInterface>(
                () => ComponentRegistry.Instance.GetComponent<IUserInterface>()
            );

            public static readonly Lazy<IFileActionHandlerManager> FileActionHandlerManager = new Lazy<IFileActionHandlerManager>(
                () => ComponentRegistry.Instance.GetComponent<IFileActionHandlerManager>()
            );

            public static string ID
            {
                get
                {
                    return "{" + typeof(NotificationActivator).GUID.ToString().ToUpper() + "}";
                }
            }

            public static string RegistryPath
            {
                get
                {
                    return "SOFTWARE\\Classes\\CLSID\\" + ID + "\\InProcServer32";
                }
            }

            static NotificationActivator()
            {
                Cookie = COOKIE_NONE;
            }

            public static RegistrationServices RegistrationServices { get; private set; }

            public static int Cookie { get; private set; }

            public void Activate(string appUserModelId, string invokedArgs, NotificationUserInputData[] data, uint dataCount)
            {
                try
                {

                    if (!string.IsNullOrEmpty(invokedArgs))
                    {
                        var fileActionHandlerManager = FileActionHandlerManager.Value;
                        if (fileActionHandlerManager != null)
                        {
                            fileActionHandlerManager.RunCommand(invokedArgs);
                        }
                    }
                    else
                    {
                        var userInterface = UserInterface.Value;
                        if (userInterface != null)
                        {
                            userInterface.Activate();
                        }
                    }
                }
                catch
                {
                    //Nothing can be done, we don't even have a logger here in this COM instance.
                }
            }

            public static void Enable()
            {
                RegistrationServices = new RegistrationServices();
                Cookie = RegistrationServices.RegisterTypeForComClients(
                    typeof(NotificationActivator),
                    RegistrationClassContext.LocalServer,
                    RegistrationConnectionType.MultipleUse
                );
            }

            public static void Disable()
            {
                if (RegistrationServices == null || Cookie == COOKIE_NONE)
                {
                    return;
                }
                try
                {
                    RegistrationServices.UnregisterTypeForComClients(Cookie);
                }
                finally
                {
                    RegistrationServices = null;
                    Cookie = COOKIE_NONE;
                }
            }
        }

        [ComImport]
        [ComVisible(true)]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("53E31837-6600-4A81-9395-75CFFE746F94")]
        public interface INotificationActivationCallback
        {
            void Activate(
                [In, MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
                [In, MarshalAs(UnmanagedType.LPWStr)] string invokedArgs,
                [In, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)] NotificationUserInputData[] data,
                [In, MarshalAs(UnmanagedType.U4)] uint dataCount
            );
        }

        [StructLayout(LayoutKind.Sequential), Serializable]
        public struct NotificationUserInputData
        {
            [MarshalAs(UnmanagedType.LPWStr)]
            public string Key;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string Value;
        }
    }
}
