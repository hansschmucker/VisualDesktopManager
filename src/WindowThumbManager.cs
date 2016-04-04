using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VisualDesktopManager
{
    class WindowThumbManager
    {
        public UInt64 pass=0;
        public List<WindowThumb> thumbs = new List<WindowThumb>();
        public Control container;
        public WindowThumbManager(Control aContainer)
        {
            container = aContainer;
        }

        public List<IntPtr> lastEnumHwnds = new List<IntPtr>();

        unsafe bool recordWindowEnum(IntPtr hwnd, int lParam)
        {
            var place = new Win32.WINDOWPLACEMENT();
            Win32.GetWindowPlacement(hwnd, ref place);
            var style = Win32.GetWindowLongA(hwnd, Win32.GWL_STYLE);
            StringBuilder title = new StringBuilder(256);
            StringBuilder module = new StringBuilder(256);

            Win32.GetWindowModuleFileName(hwnd, module, 256);
            Win32.GetWindowText(hwnd, title, 256);

            if (
                   (style & Win32.WS_ICONIC) != Win32.WS_ICONIC
                && (style & Win32.WS_VISIBLE) == Win32.WS_VISIBLE
                && (title.Length>0 || module.Length>0)
                && place.showCmd!=2
                && hwnd!=container.Handle
            )
                lastEnumHwnds.Add(hwnd);
            return true;
        }

        unsafe void createThumbs()
        {
            
            for (var i = lastEnumHwnds.Count - 1; i >= 0; i--)
            {
                var hwnd = lastEnumHwnds[i];
                {
                    var thumb = new WindowThumb(this);
                    thumbs.Add(thumb);
                    thumb.window = hwnd;
                    thumb.showThumb();
                    thumb.update();
                }
            }
        }

        public void removeThumb(WindowThumb thumb)
        {
            for (var i = this.thumbs.Count - 1; i >= 0; i--)
            {
                if (thumb == thumbs[i])
                {
                    thumb.hideThumb();
                    thumb.manager = null;
                    thumb.window = IntPtr.Zero;
                    thumbs.RemoveAt(i);
                    return;
                }
            }
        }
        public WindowThumb getUnder(int x,int y){
            for (var i = this.thumbs.Count - 1; i >= 0; i--)
                if (x >= thumbs[i].clientX && y >= thumbs[i].clientY && x < thumbs[i].clientX2 && y < thumbs[i].clientY2)
                    return thumbs[i];
            
            return null;
        }

        public WindowThumb findThumb(IntPtr hwnd)
        {
            for (var i = this.thumbs.Count - 1; i >= 0; i--)
                if(this.thumbs[i].window == hwnd)
                    return this.thumbs[i];

            return null;
        }

        public double scale;
        public double aspect;
        public int viewport_x0;
        public int viewport_x1;
        public int viewport_y0;
        public int viewport_y1;
        public bool stopped = false;

        public void stop()
        {
            stopped = true;
            for (var i = thumbs.Count-1; i >= 0; i--)
             removeThumb(thumbs[i]);
        }

        public void resume()
        {
            stopped = false;
        }

        public void updateAspect()
        {
            viewport_x0 = int.MaxValue;
            viewport_y0 = int.MaxValue;
            viewport_x1 = int.MinValue;
            viewport_y1 = int.MinValue;

            for (var i = 0; i < Screen.AllScreens.Length; i++)
            {
                var scr = Screen.AllScreens[i];
                viewport_x0 = Math.Min(viewport_x0, scr.Bounds.Left);
                viewport_x1 = Math.Max(viewport_x1, scr.Bounds.Right);
                viewport_y0 = Math.Min(viewport_y0, scr.Bounds.Top);
                viewport_y1 = Math.Max(viewport_y1, scr.Bounds.Bottom);
            }
            var deltaX = viewport_x1 - viewport_x0;
            var deltaY = viewport_y1 - viewport_y0;
            var scaleX = (double)container.Bounds.Width / (double)deltaX;
            var scaleY = (double)container.Bounds.Height / (double)deltaY;


            aspect = (double)deltaX / (double)deltaY;
            scale = Math.Min(scaleX, scaleY);
        }

        public void update(){
            if (stopped)
                return;

            lastEnumHwnds = new List<IntPtr>();
            Win32.EnumWindows(new Win32.EnumWindowsCallback(recordWindowEnum), 0);
            var existingWindows = new List<IntPtr>();

            // Weed out all outdated thumbs
            for (var i = thumbs.Count-1; i >= 0; i--){
                if (!lastEnumHwnds.Contains(thumbs[i].window))
                {
                    removeThumb(thumbs[i]);
                }
                else
                    existingWindows.Add(thumbs[i].window);
            }

            
            
            // Add any new ones
            for (var i = lastEnumHwnds.Count-1; i >= 0; i--)
            {
                if(!existingWindows.Contains(lastEnumHwnds[i])){
                    {
                        var thumb = new WindowThumb(this);
                        thumbs.Add(thumb);
                        existingWindows.Add(lastEnumHwnds[i]);
                        thumb.window = lastEnumHwnds[i];
                        thumb.showThumb();
                        thumb.update();
                    }
                }
            }

            lastEnumHwnds = new List<IntPtr>();
            Win32.EnumWindows(new Win32.EnumWindowsCallback(recordWindowEnum), 0);

            var outOfOrderThumbs = new List<WindowThumb>();
            if (lastEnumHwnds.Count != thumbs.Count)
            {
                outOfOrderThumbs.Add(null);
            }
            else
            {
                for (var i = 0; i < lastEnumHwnds.Count; i++)
                {
                    if (lastEnumHwnds[i] != thumbs[thumbs.Count - 1 - i].window)
                        outOfOrderThumbs.Add(thumbs[i]);
                }
            }



            if (outOfOrderThumbs.Count > 0)
            {
                for (var i = thumbs.Count - 1; i >= 0; i--)
                {
                    removeThumb(thumbs[i]);
                }
                
            }

            /*
             * Go through all hwnds and flag the ones found valid.
             * Assign a Z depth to each valid one.
             * Create ones that don't have a thumb
             * Sort them by Z
             * Go through all thumbs. Weed out all invalid ones
             * Assign Z to thumbs
             * Sort them by Z
             * 
             */

            var oldScale = scale;
            var oldX = viewport_x0;
            var oldY = viewport_y0;

            updateAspect();

            var updateAll = scale != oldScale || oldX != viewport_x0 || oldY != viewport_y0;

            for (var i = 0; i < thumbs.Count; i++)
                thumbs[i].update(updateAll);
        }

    }


    
}
