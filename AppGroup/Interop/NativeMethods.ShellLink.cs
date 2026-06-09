using System;
using System.Runtime.InteropServices;
using System.Text;

namespace AppGroup.Interop
{
    /// <summary>
    /// Contains P/Invoke declarations for IShellLink and related COM interfaces.
    /// Used as a replacement for IWshRuntimeLibrary to avoid COM interop dependencies.
    /// </summary>
    public static partial class NativeMethods
    {
        #region ShellLink COM Interfaces

        [ComImport]
        [Guid("00021401-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IShellLinkW
        {
            // IUnknown methods
            void GetTypeInfoCount(out uint pcTInfo);
            void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo);
            void GetIDsOfNames(ref Guid riid, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgszNames, uint cNames, uint lcid, IntPtr rgDispId);
            void Invoke(uint dispIdMember, ref Guid riid, uint lcid, uint dwFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

            // IShellLink methods
            void GetPath([MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile, int cch, ref _WIN32_FIND_DATAW pfd, uint fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cch);
            void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
            void GetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cch);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
            void GetArguments([MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cch);
            void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
            void GetHotkey(out ushort pwHotkey);
            void SetHotkey(ushort wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath, int cch, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
            void Resolve(IntPtr hwnd, uint fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
        }

        [ComImport]
        [Guid("000214EE-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IShellLinkA
        {
            // IUnknown methods
            void GetTypeInfoCount(out uint pcTInfo);
            void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo);
            void GetIDsOfNames(ref Guid riid, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] rgszNames, uint cNames, uint lcid, IntPtr rgDispId);
            void Invoke(uint dispIdMember, ref Guid riid, uint lcid, uint dwFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

            // IShellLink methods
            void GetPath([MarshalAs(UnmanagedType.LPStr)] StringBuilder pszFile, int cch, ref _WIN32_FIND_DATAA pfd, uint fFlags);
            void GetIDList(out IntPtr ppidl);
            void SetIDList(IntPtr pidl);
            void GetDescription([MarshalAs(UnmanagedType.LPStr)] StringBuilder pszName, int cch);
            void SetDescription([MarshalAs(UnmanagedType.LPStr)] string pszName);
            void GetWorkingDirectory([MarshalAs(UnmanagedType.LPStr)] StringBuilder pszDir, int cch);
            void SetWorkingDirectory([MarshalAs(UnmanagedType.LPStr)] string pszDir);
            void GetArguments([MarshalAs(UnmanagedType.LPStr)] StringBuilder pszArgs, int cch);
            void SetArguments([MarshalAs(UnmanagedType.LPStr)] string pszArgs);
            void GetHotkey(out ushort pwHotkey);
            void SetHotkey(ushort wHotkey);
            void GetShowCmd(out int piShowCmd);
            void SetShowCmd(int iShowCmd);
            void GetIconLocation([MarshalAs(UnmanagedType.LPStr)] StringBuilder pszIconPath, int cch, out int piIcon);
            void SetIconLocation([MarshalAs(UnmanagedType.LPStr)] string pszIconPath, int iIcon);
            void SetRelativePath([MarshalAs(UnmanagedType.LPStr)] string pszPathRel, uint dwReserved);
            void Resolve(IntPtr hwnd, uint fFlags);
            void SetPath([MarshalAs(UnmanagedType.LPStr)] string pszFile);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct _WIN32_FIND_DATAW
        {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
        public struct _WIN32_FIND_DATAA
        {
            public uint dwFileAttributes;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftCreationTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastAccessTime;
            public System.Runtime.InteropServices.ComTypes.FILETIME ftLastWriteTime;
            public uint nFileSizeHigh;
            public uint nFileSizeLow;
            public uint dwReserved0;
            public uint dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string cFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            public string cAlternateFileName;
        }

        #endregion

        #region CLSID and IID Constants

        public static readonly Guid CLSID_ShellLink = new Guid("00021401-0000-0000-C000-000000000046");
        public static readonly Guid IID_IShellLinkW = new Guid("00021401-0000-0000-C000-000000000046");
        public static readonly Guid IID_IShellLinkA = new Guid("000214EE-0000-0000-C000-000000000046");
        public static readonly Guid IID_IPersistFile = new Guid("0000010B-0000-0000-C000-000000000046");

        #endregion

        #region IPersistFile Interface

        [ComImport]
        [Guid("0000010B-0000-0000-C000-000000000046")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        public interface IPersistFile
        {
            // IUnknown methods
            void GetTypeInfoCount(out uint pcTInfo);
            void GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo);
            void GetIDsOfNames(ref Guid riid, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPWStr)] string[] rgszNames, uint cNames, uint lcid, IntPtr rgDispId);
            void Invoke(uint dispIdMember, ref Guid riid, uint lcid, uint dwFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr);

            // IPersist methods
            void GetClassID(out Guid pClassID);

            // IPersistFile methods
            void IsDirty();
            void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
            void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, [MarshalAs(UnmanagedType.Bool)] bool fRemember);
            void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
            void GetCurFile([MarshalAs(UnmanagedType.LPWStr)] out string ppszFileName);
        }

        #endregion

        #region Helper Methods for ShellLink

        /// <summary>
        /// Creates a new IShellLink instance.
        /// </summary>
        public static IShellLinkW CreateShellLink()
        {
            Type? shellLinkType = Type.GetTypeFromCLSID(CLSID_ShellLink);
            if (shellLinkType == null)
                throw new COMException("Cannot create IShellLink instance", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

            object? instance = Activator.CreateInstance(shellLinkType);
            if (instance == null)
                throw new COMException("Cannot create IShellLink instance", Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error()));

            return (IShellLinkW)instance;
        }

        /// <summary>
        /// Saves a shell link to a file.
        /// </summary>
        public static void SaveShellLink(IShellLinkW shellLink, string filePath)
        {
            if (shellLink is IPersistFile persistFile)
            {
                persistFile.Save(filePath, true);
            }
            else
            {
                // Fallback using COM
                dynamic comShellLink = shellLink;
                comShellLink.Save(filePath);
            }
        }

        /// <summary>
        /// Loads a shell link from a file.
        /// </summary>
        public static IShellLinkW LoadShellLink(string filePath)
        {
            IShellLinkW shellLink = CreateShellLink();
            if (shellLink is IPersistFile persistFile)
            {
                persistFile.Load(filePath, 0);
            }
            else
            {
                // Fallback using COM
                dynamic comShellLink = shellLink;
                comShellLink.Load(filePath);
            }
            return shellLink;
        }

        #endregion
    }
}
