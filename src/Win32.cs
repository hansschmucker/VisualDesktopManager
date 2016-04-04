using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
/*

        static readonly int GWL_STYLE = -16;

        static readonly int DWM_TNP_VISIBLE = 0x8;
        static readonly int DWM_TNP_OPACITY = 0x4;
        static readonly int DWM_TNP_RECTDESTINATION = 0x1;
        static readonly int SW_SHOWNOACTIVATE = 0x4;
        static readonly int HWND_TOPMOST = -1;
        static readonly int SWP_NOACTIVATE = 0x0010;

        static readonly ulong WS_VISIBLE = 0x10000000L;
        static readonly ulong WS_BORDER = 0x00800000L;
        static readonly ulong TARGETWINDOW = WS_BORDER | WS_VISIBLE;

        [DllImport("dwmapi.dll")]
        static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

        [DllImport("dwmapi.dll")]
        static extern int DwmUnregisterThumbnail(IntPtr thumb);

        [DllImport("dwmapi.dll")]
        static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out PSIZE size);

        [DllImport("dwmapi.dll")]
        static extern int DwmUpdateThumbnailProperties(IntPtr hThumb, ref DWM_THUMBNAIL_PROPERTIES props);

        [DllImport("user32.dll")]
        static extern ulong GetWindowLongA(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int EnumWindows(EnumWindowsCallback lpEnumFunc, int lParam);
        delegate bool EnumWindowsCallback(IntPtr hwnd, int lParam);

        [DllImport("user32.dll")]
        static extern void GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        static extern void GetWindowRect(IntPtr hWnd, ref Rect lpRect);

        [DllImport("user32.dll")]
        static extern void SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int w, int h, int flags);
*/
namespace VisualDesktopManager
{
    class Win32
    {
        public static readonly int GWL_STYLE = -16;

        public static readonly int DWM_TNP_VISIBLE = 0x8;
        public static readonly int DWM_TNP_OPACITY = 0x4;
        public static readonly int DWM_TNP_RECTDESTINATION = 0x1;

        public static readonly ulong WS_BORDER = 0x00800000L;
        public static readonly ulong WS_CAPTION = 0x00C00000L;
        public static readonly ulong WS_CHILD = 0x40000000L;
        public static readonly ulong WS_CHILDWINDOW = 0x40000000L;
        public static readonly ulong WS_CLIPCHILDREN = 0x02000000L;
        public static readonly ulong WS_CLIPSIBLINGS = 0x04000000L;
        public static readonly ulong WS_DISABLED = 0x08000000L;
        public static readonly ulong WS_DLGFRAME = 0x00400000L;
        public static readonly ulong WS_GROUP = 0x00020000L;
        public static readonly ulong WS_HSCROLL = 0x00100000L;
        public static readonly ulong WS_ICONIC = 0x20000000L;
        public static readonly ulong WS_MAXIMIZE = 0x01000000L;
        public static readonly ulong WS_MAXIMIZEBOX = 0x00010000L;
        public static readonly ulong WS_MINIMIZE = 0x20000000L;
        public static readonly ulong WS_MINIMIZEBOX = 0x00020000L;
        public static readonly ulong WS_OVERLAPPED = 0x00000000L;
        public static readonly ulong WS_OVERLAPPEDWINDOW = 0x00000000L | 0x00C00000L | 0x00080000L | 0x00040000L | 0x00020000L | 0x00010000L;
        public static readonly ulong WS_POPUP = 0x80000000L;
        public static readonly ulong WS_POPUPWINDOW = 0x80000000L | 0x00800000L | 0x00080000L;
        public static readonly ulong WS_SIZEBOX = 0x00040000L;
        public static readonly ulong WS_SYSMENU = 0x00080000L;
        public static readonly ulong WS_TABSTOP = 0x00010000L;
        public static readonly ulong WS_THICKFRAME = 0x00040000L;
        public static readonly ulong WS_TILED = 0x00000000L;
        public static readonly ulong WS_TILEDWINDOW = 0x00000000L | 0x00C00000L | 0x00080000L | 0x00040000L | 0x00020000L | 0x00010000L;
        public static readonly ulong WS_VISIBLE = 0x10000000L;
        public static readonly ulong WS_VSCROLL = 0x00200000L;


        public static readonly ulong TARGETWINDOW = WS_BORDER | WS_VISIBLE;
        public static readonly int SW_SHOWNOACTIVATE = 0x4;
        public static readonly IntPtr HWND_TOPMOST = (IntPtr)(-1);
        public static readonly int SWP_NOACTIVATE = 0x0010;

        [DllImport("dwmapi.dll")]
        public static extern int DwmRegisterThumbnail(IntPtr dest, IntPtr src, out IntPtr thumb);

        [DllImport("dwmapi.dll")]
        public static extern int DwmUnregisterThumbnail(IntPtr thumb);

        [DllImport("dwmapi.dll")]
        public static extern int DwmQueryThumbnailSourceSize(IntPtr thumb, out PSIZE size);

        [DllImport("dwmapi.dll")]
        public static extern int DwmUpdateThumbnailProperties(IntPtr hThumb, ref DWM_THUMBNAIL_PROPERTIES props);

        [DllImport("user32.dll")]
        public static extern ulong GetWindowLongA(IntPtr hWnd, int nIndex);
        
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int EnumWindows(EnumWindowsCallback lpEnumFunc, int lParam);
        public delegate bool EnumWindowsCallback(IntPtr hwnd, int lParam);
        
        [DllImport("user32.dll")]
        public static extern int GetWindowModuleFileName(IntPtr hWnd, StringBuilder title, int size);
        
        [DllImport("user32.dll")]
        public static extern void GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern void GetWindowRect(IntPtr hWnd, ref Rect lpRect);

        [DllImport("user32.dll")]
        public static extern void SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int w, int h, int flags);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        public static extern bool SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);


        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public System.Drawing.Point ptMinPosition;
            public System.Drawing.Point ptMaxPosition;
            public System.Drawing.Rectangle rcNormalPosition;
        }


        [StructLayout(LayoutKind.Sequential)]
        public struct DWM_THUMBNAIL_PROPERTIES
        {
            public int dwFlags;
            public Rect rcDestination;
            public Rect rcSource;
            public byte opacity;
            public bool fVisible;
            public bool fSourceClientAreaOnly;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            internal Rect(int left, int top, int right, int bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PSIZE
        {
            public int x;
            public int y;
        }

    }
}
