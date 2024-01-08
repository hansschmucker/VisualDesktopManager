using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace VisualDesktopManager
{
    class WindowThumb
    {
        public IntPtr window = IntPtr.Zero;
        public int x = 0, y = 0, z = 0, x2 = 0, y2 = 0;
        public int clientX, clientY, clientX2, clientY2;
        public UInt64 lastUpdated = 0;
        public IntPtr thumbnail = IntPtr.Zero;
        public WindowThumbManager manager = null;

        public WindowThumb(WindowThumbManager aManager)
        {
            manager = aManager;
        }

        public void hideThumb()
        {
            if (thumbnail == IntPtr.Zero)
                return;

            Win32.DwmUnregisterThumbnail(thumbnail);
            thumbnail = IntPtr.Zero;
        }

        public void showThumb()
        {
            if (window == IntPtr.Zero || manager.container == null)
                return;
            
            hideThumb();

            Win32.DwmRegisterThumbnail(manager.container.Handle, window, out thumbnail);
            update(true);
        }

        public void removeThumb()
        {
            if (manager != null)
            {
                manager.removeThumb(this);
            }
        }

        public void centerOn(int inputx, int inputy)
        {
            if (manager == null)
                return;
            
            var client = new Win32.Rect();
            Win32.GetClientRect(window, ref client);

            var natW=client.Right;
            var natH=client.Bottom;
            var outerW = (x2 - x);
            var outerH = (y2 - y);
            var borderSide = (outerW - natW) / 2;

            Win32.ShowWindow(window, Win32.SW_SHOWNOACTIVATE);
            var dx = (int)(inputx / manager.scale + manager.viewport_x0 - natW / 2);
            var dy = (int)(inputy / manager.scale + manager.viewport_y0 - natH / 2);


            if (dx + natW > manager.viewport_x1)
                dx = manager.viewport_x1 - natW;
            if (dy + natH > manager.viewport_y1)
                dy = manager.viewport_y1 - natH;

            if (dx < manager.viewport_x0)
                dx = manager.viewport_x0;
            if (dy < manager.viewport_y0)
                dy = manager.viewport_y0;


            

            Win32.SetWindowPos(window, IntPtr.Zero,dx- borderSide, dy-1, outerW, outerH, Win32.SW_SHOWNOACTIVATE);
            update(true);
        }

        public void update()
        {
            update(false);
        }

        public void maximize(int thumbX, int thumbY) {
            List<int[]> screens = new List<int[]>();
            {
                for (var i = 0; i < Screen.AllScreens.Length; i++) {
                    screens.Add(new int[] { Screen.AllScreens[i].Bounds.Left, Screen.AllScreens[i].Bounds.Top, Screen.AllScreens[i].Bounds.Right, Screen.AllScreens[i].Bounds.Bottom, Screen.AllScreens[i].Bounds.Width, Screen.AllScreens[i].Bounds.Height, Screen.AllScreens[i].WorkingArea.Left, Screen.AllScreens[i].WorkingArea.Top, Screen.AllScreens[i].WorkingArea.Right, Screen.AllScreens[i].WorkingArea.Bottom,Screen.AllScreens[i].WorkingArea.Width, Screen.AllScreens[i].WorkingArea.Height });
                }
            }

            Win32.Rect bounds = new Win32.Rect();
            Win32.GetWindowRect(window, ref bounds);

            var pc = new Win32.WINDOWPLACEMENT();
            Win32.GetWindowPlacement(window, ref pc);

            //use 1 if already maximized
            if (pc.showCmd == 3) { 
                pc.showCmd = 1;
                Win32.SetWindowPlacement(window, ref pc);
                return;
            }

            var cx = (int)(thumbX / manager.scale);
            var cy = (int)(thumbY / manager.scale);

            for (var i = 0; i < screens.Count; i++)
            {
                if(
                    cx >= screens[i][0]
                    && cx < screens[i][2]
                    && cy >= screens[i][1]
                    && cy < screens[i][3]
                )
                {


                    var rcx = cx - screens[i][0];
                    var rcy = cy - screens[i][1];
                    var left = screens[i][4] / 4;
                    var right = left * 3;
                    var top = screens[i][5] / 4;
                    var bottom = top * 3;
                    var hFull = screens[i][10];
                    var vFull = screens[i][11];
                    var hHalf = hFull / 2;
                    var vHalf = vFull / 2;


                    if (rcx < left && rcy < top) {
                        Win32.SetWindowPos(window, IntPtr.Zero, screens[i][6], screens[i][7], hHalf, vHalf, Win32.SW_SHOWNOACTIVATE);
                    } else if (rcx > right && rcy < top) {
                        Win32.SetWindowPos(window, IntPtr.Zero, screens[i][6] + hHalf, screens[i][7], hHalf, vHalf, Win32.SW_SHOWNOACTIVATE);
                    } else if (rcx < left && rcy > bottom) {
                        Win32.SetWindowPos(window, IntPtr.Zero, screens[i][6], screens[i][7] + vHalf, hHalf, vHalf, Win32.SW_SHOWNOACTIVATE);
                    } else if (rcx > right && rcy > bottom) {
                        Win32.SetWindowPos(window, IntPtr.Zero, screens[i][6] + hHalf, screens[i][7] + vHalf, hHalf, vHalf, Win32.SW_SHOWNOACTIVATE);
                    } else if (rcx < left) {
                        Win32.SetWindowPos(window, IntPtr.Zero, screens[i][6], screens[i][7], hHalf, vFull, Win32.SW_SHOWNOACTIVATE);
                    } else if (rcx > right) {
                        Win32.SetWindowPos(window, IntPtr.Zero, screens[i][6] + hHalf, screens[i][7], hHalf, vFull, Win32.SW_SHOWNOACTIVATE);
                    } else if (rcy < top) {
                        Win32.SetWindowPos(window, IntPtr.Zero, screens[i][6], screens[i][7], hFull, vHalf, Win32.SW_SHOWNOACTIVATE);
                    } else if (rcy > bottom) {
                        Win32.SetWindowPos(window, IntPtr.Zero, screens[i][6], screens[i][7] + vHalf, hFull, vHalf, Win32.SW_SHOWNOACTIVATE);
                    } else {
                        Win32.ShowWindow(window, Win32.SW_SHOWNOACTIVATE);

                        pc.showCmd = 3;
                        Win32.SetWindowPlacement(window, ref pc);
                        update(true);
                        return;
                    }
                }
            }
        }

        public void update(bool force)
        {
            if (window == IntPtr.Zero){
                hideThumb();
                return;
            }

            Win32.Rect bounds=new Win32.Rect();
            Win32.GetWindowRect(window, ref bounds);

            if (!force && x == bounds.Left && y == bounds.Top && x2 == bounds.Right && y2 == bounds.Bottom)
                return;

            x = bounds.Left;
            y = bounds.Top;
            x2 = bounds.Right;
            y2 = bounds.Bottom;

            var props = new Win32.DWM_THUMBNAIL_PROPERTIES();
            props.rcDestination.Left = clientX = (int)((x-manager.viewport_x0)*manager.scale);
            props.rcDestination.Right = clientX2 = (int)((x2 - manager.viewport_x0) * manager.scale);
            props.rcDestination.Top = clientY = (int)((y - manager.viewport_y0) * manager.scale);
            props.rcDestination.Bottom = clientY2 = (int)((y2 - manager.viewport_y0) * manager.scale);
            
            props.rcSource.Left = 0;
            props.rcSource.Right = 0;
            props.rcSource.Top = 0;
            props.rcSource.Bottom = 0;
            props.fVisible = true;
            props.dwFlags = Win32.DWM_TNP_VISIBLE | Win32.DWM_TNP_RECTDESTINATION | Win32.DWM_TNP_OPACITY;
            props.opacity = (byte)255;
            Win32.DwmUpdateThumbnailProperties(thumbnail, ref props);

            /*lastUpdated = mgr.pass;*/
        }
    }

    
}
