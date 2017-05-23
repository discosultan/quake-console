using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.InteropServices;

namespace QuakeConsole
{    
    internal static class Native
    {
        const uint CF_UNICODETEXT = 13;

        [DllImport("user32.dll")]
        private static extern ushort GetKeyState(int keyCode);
        [DllImport("user32.dll")]
        private static extern IntPtr GetClipboardData(uint uFormat);
        [DllImport("user32.dll")]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);
        [DllImport("user32.dll")]
        private static extern bool IsClipboardFormatAvailable(uint format);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool CloseClipboard();
        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalAlloc(uint uFlags, UIntPtr dwBytes);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalFree(IntPtr hMem);
        [DllImport("kernel32.dll")]
        private static extern IntPtr GlobalLock(IntPtr hMem);
        [DllImport("kernel32.dll")]
        private static extern bool GlobalUnlock(IntPtr hMem);
        [DllImport("kernel32.dll", EntryPoint = "CopyMemory", SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, uint count);                

        // Ref: http://users.cis.fiu.edu/~downeyt/cop4226/toggled.html
        public static bool IsKeyToggled(Keys key)
        {
            try
            {
                var nKey = (int) key;
                return (GetKeyState(nKey) & 0x01) == 1;
            }
            catch
            {
                return false;
            }
        }

        public static string GetClipboardText()
        {
            try
            {
                return GetClipboardTextImpl() ?? "";
            }
            catch
            {
                return "";
            }
        }

        public static void SetClipboardText(string value)
        {
            try
            {
                SetClipboardTextImpl(value);
            }
            catch
            {
            }
        }

        // Ref: http://stackoverflow.com/a/5945476/1466456
        private static string GetClipboardTextImpl()
        {
            if (!IsClipboardFormatAvailable(CF_UNICODETEXT))
                return null;
            if (!OpenClipboard(IntPtr.Zero))
                return null;

            string data = null;
            try
            {
                var hGlobal = GetClipboardData(CF_UNICODETEXT);
                if (hGlobal != IntPtr.Zero)
                {
                    var lpwcstr = GlobalLock(hGlobal);
                    if (lpwcstr != IntPtr.Zero)
                    {
                        data = Marshal.PtrToStringUni(lpwcstr);
                        GlobalUnlock(lpwcstr);
                    }
                }
            }
            finally
            {
                CloseClipboard();
            }

            return data;
        }

        // Ref: http://stackoverflow.com/a/24698804/1466456
        private static bool SetClipboardTextImpl(string message)
        {
            if (message == null)
                return false;

            if (!OpenClipboard(IntPtr.Zero))
                return false;

            try
            {
                uint sizeOfChar = 2;

                var characters = (uint)message.Length;
                uint bytes = (characters + 1) * sizeOfChar;

                const int GMEM_MOVABLE = 0x0002;
                const int GMEM_ZEROINIT = 0x0040;
                const int GHND = GMEM_MOVABLE | GMEM_ZEROINIT;

                // IMPORTANT: SetClipboardData requires memory that was acquired with GlobalAlloc using GMEM_MOVABLE.
                var hGlobal = GlobalAlloc(GHND, (UIntPtr)bytes);
                if (hGlobal == IntPtr.Zero)
                    return false;

                try
                {
                    // IMPORTANT: Marshal.StringToHGlobalUni allocates using LocalAlloc with LMEM_FIXED.
                    //            Note that LMEM_FIXED implies that LocalLock / LocalUnlock is not required.
                    IntPtr source = Marshal.StringToHGlobalUni(message);

                    try
                    {
                        var target = GlobalLock(hGlobal);
                        if (target == IntPtr.Zero)
                            return false;

                        try
                        {
                            CopyMemory(target, source, bytes);
                        }
                        finally
                        {
                            GlobalUnlock(target);
                        }

                        if (SetClipboardData(CF_UNICODETEXT, hGlobal).ToInt64() != 0)
                        {
                            // IMPORTANT: SetClipboardData takes ownership of hGlobal upon success.
                            hGlobal = IntPtr.Zero;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    finally
                    {
                        // Marshal.StringToHGlobalUni actually allocates with LocalAlloc, thus we should theorhetically use LocalFree to free the memory...
                        // ... but Marshal.FreeHGlobal actully uses a corresponding version of LocalFree internally, so this works, even though it doesn't
                        //  behave exactly as expected.
                        Marshal.FreeHGlobal(source);
                    }
                }
                catch (OutOfMemoryException)
                {
                    return false;
                }
                catch (ArgumentOutOfRangeException)
                {
                    return false;
                }
                finally
                {
                    if (hGlobal != IntPtr.Zero)
                        GlobalFree(hGlobal);
                }
            }
            finally
            {
                CloseClipboard();
            }
            return true;
        }
    }
}
