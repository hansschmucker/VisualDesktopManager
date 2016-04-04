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

            var natW=(x2 - x);
            var natH=(y2 - y);

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

            Win32.SetWindowPos(window, IntPtr.Zero,dx, dy, natW, natH, Win32.SW_SHOWNOACTIVATE);
            update(true);
        }

        public void update()
        {
            update(false);
        }

        public void maximize()
        {
            List<int[]> screens=new List<int[]>();
            if (Screen.AllScreens.Length == 1 && Screen.AllScreens[0].Bounds.Width > 1920)
            {
                var scr = Screen.AllScreens[0];
                for (var i = 0; i < scr.Bounds.Width; i+=1920)
                {
                    screens.Add(new int[] { i, scr.Bounds.Top, i + 1920, scr.Bounds.Bottom, 1920, scr.Bounds.Height });
                }
            }
            else
            {
                for (var i = 0; i < Screen.AllScreens.Length; i++)
                {
                    screens.Add(new int[] { Screen.AllScreens[i].Bounds.Left,Screen.AllScreens[i].Bounds.Top, Screen.AllScreens[i].Bounds.Right, Screen.AllScreens[i].Bounds.Bottom, Screen.AllScreens[i].Bounds.Width, Screen.AllScreens[i].Bounds.Height });
                }
            }

            Win32.Rect bounds = new Win32.Rect();
            Win32.GetWindowRect(window, ref bounds);

            var cx = bounds.Left + (bounds.Right - bounds.Left) / 2;
            var cy = bounds.Top + (bounds.Bottom - bounds.Top) / 2;
            
            for (var i = 0; i < screens.Count; i++)
            {
                if(
                    cx >= screens[i][0]
                    && cx < screens[i][2]
                    && cy >= screens[i][1]
                    && cy < screens[i][3]
                )
                {
                    //manager.container.Text = "CX" + cx.ToString() + ",CY" + cy.ToString() + " " + screens[i][0].ToString() + " " + screens[i][1].ToString() + " " + screens[i][2].ToString() + " " + screens[i][3].ToString() + " " + screens[i][4].ToString() + " " + screens[i][5].ToString();

                    Win32.ShowWindow(window, Win32.SW_SHOWNOACTIVATE);
                    //Win32.SetWindowPos(window, IntPtr.Zero, screens[i][0], screens[i][1], screens[i][4], screens[i][5], Win32.SW_SHOWNOACTIVATE);
                    var pc = new Win32.WINDOWPLACEMENT();
                    Win32.GetWindowPlacement(window, ref pc);
                    pc.showCmd = 3;
                    Win32.SetWindowPlacement(window, ref pc);
                    update(true);
                    return;
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
