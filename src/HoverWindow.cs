using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace VisualDesktopManager
{
    public partial class HoverWindow : Form
    {
        private NotifyIcon trayNotifyIcon;
        private ContextMenu trayContextMenu;
        private bool Sticky = false;

        public HoverWindow()
        {
            trayContextMenu = new ContextMenu();
            trayNotifyIcon = new NotifyIcon();
            IntPtr tryIconPtr = Properties.Resources.trayIcon.GetHicon();
            Icon trayIcon = Icon.FromHandle(tryIconPtr);

            trayContextMenu.MenuItems.Add("Exit", OnExit);
            trayContextMenu.MenuItems.Add("Sticky", OnSticky);
            trayNotifyIcon.Icon = trayIcon;
            trayNotifyIcon.Text = "VisualDesktopManager";

            trayNotifyIcon.ContextMenu = trayContextMenu;
            trayNotifyIcon.Visible = true;

            InitializeComponent();
        }

        private void OnExit(object sender, EventArgs e)
        {
            closingAllowed = true;
            Application.Exit();
        }

        private void OnSticky(object sender, EventArgs e) {
            Sticky = !Sticky;
        }

        protected override void OnLoad(EventArgs e)
        {
            ShowInTaskbar = false;
            Visible = false;

            base.OnLoad(e);
        }

        protected override void Dispose(bool isDisposing)
        {
            if ((components != null))
            {
                components.Dispose();
            }

            if (isDisposing)
            {
                trayNotifyIcon.Dispose();
            }

            base.Dispose(isDisposing);
        }

        private void BringToPassiveFront()
        {
            {
                Win32.ShowWindow(Handle, Win32.SW_SHOWNOACTIVATE);
                var r = new Win32.Rect();
                Win32.GetWindowRect(Handle, ref r);
                Win32.SetWindowPos(Handle, Win32.HWND_TOPMOST, r.Left, r.Top, r.Right - r.Left, r.Bottom - r.Top,Win32.SWP_NOACTIVATE);
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        internal struct DWM_THUMBNAIL_PROPERTIES
        {
            public int dwFlags;
            public Rect rcDestination;
            public Rect rcSource;
            public byte opacity;
            public bool fVisible;
            public bool fSourceClientAreaOnly;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Rect
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
        internal struct PSIZE
        {
            public int x;
            public int y;
        }

        private const int CP_NOCLOSE_BUTTON = 0x200;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams myCp = base.CreateParams;
                myCp.ClassStyle = myCp.ClassStyle | CP_NOCLOSE_BUTTON;
                return myCp;
            }
        }

        WindowThumbManager mgr;

        public Control thumbControl;
        private void Form1_Load(object sender, EventArgs e)
        {
            mgr = new WindowThumbManager(this);

            
            timer1.Interval = 1000/60;
            timer1.Start();
            exitMini();
        }

        private bool minimized = false;
        private bool mouseOver = true;
        private UInt64 age = 0;
        private int lastWidth = 1;
        private int lastHeight = 1;
        private int lastLeft = 1;
        private int lastTop = 1;

        private void enterMini()
        {
            if (minimized || Sticky)
                return;
            mgr.stop();
            {
                FormBorderStyle = FormBorderStyle.None;

                AllowTransparency = true;
                TransparencyKey = System.Drawing.Color.HotPink;
                BackColor = System.Drawing.Color.HotPink;
            }
            button1.BackColor = System.Drawing.Color.HotPink;


            lastWidth = ClientRectangle.Width;
            lastHeight = ClientRectangle.Height;
            lastLeft = Left;
            lastTop = Top;


            Width = 64;
            Height = 64;
            Left = Left + lastWidth - Width;
            Top = Top + lastHeight - Height;
            
            {
                Width = 64;
                Height = 64;
            }
            button1.Width = 64;
            button1.Height = 64;
            button1.Left = 0;
            button1.Top = 0;


            button1.Visible = true;
            minimized = true;
        }

        private void exitMini()
        {
            if (!minimized)
                return;

            Width = lastWidth;
            Height = lastHeight;
            Left = lastLeft;
            Top = lastTop;
            
            {
                FormBorderStyle = FormBorderStyle.SizableToolWindow;
                AllowTransparency = false;
                BackColor = System.Drawing.Color.Silver;
            }
            button1.BackColor = System.Drawing.Color.Silver;

            mgr.resume();
            button1.Visible = false;
            minimized = false;
        }

        private bool firstTick = true;
        private void OnTick(object sender, EventArgs e)
        {
            var r=new Win32.Rect();
            Win32.GetWindowRect(Handle, ref r);
            if (
                MousePosition.X > r.Left
                && MousePosition.X < r.Right
                && MousePosition.Y > r.Top
                && MousePosition.Y < r.Bottom
            )
                mouseOver = true;
            else{
                mouseOver = false;
                dragging = IntPtr.Zero;
            }

            if (!mouseOver)
                age++;
            else{
                age = 0;
                exitMini();
            }

            BringToPassiveFront();

            if (firstTick)
            {
                mgr.updateAspect();
                OnResize(null,null);
            }
            firstTick = false;
            if (age > 32)
                enterMini();
            else
                mgr.update();
        }

        private IntPtr dragging = IntPtr.Zero;

        private long lastClickTime = 0;
        private bool windowIsDragging;
        private bool closingAllowed = false;

        private void OnThumbMouseDown(object sender, MouseEventArgs e)
        {

            if (e.Button == MouseButtons.Right)
            {
                OnMouseUp(sender,e);

                windowIsDragging = true;
            }else if(e.Button == MouseButtons.Middle) {
                //FIXME add simulate click with middle button
                // https://stackoverflow.com/questions/10355286/programmatically-mouse-click-in-another-window
                var oldPos = Cursor.Position;
                Cursor.Position = new Point((int)(e.X / mgr.scale), (int)(e.Y / mgr.scale));

                /// return mouse 
                Cursor.Position = oldPos;

                return;
            }

            if (e.Button != MouseButtons.Left)
                return;
            
            if (dragging != IntPtr.Zero)
                return;
            var t = DateTime.Now.Ticks;
            var thumb = mgr.getUnder(e.X, e.Y);

            if (thumb == null)
                return;

            if ((t - lastClickTime) < 2500000)
            {
                thumb.maximize(e.X,e.Y);
            }
            else
            {
                dragging = thumb.window;
                lastClickTime = t;
            }
            BringToPassiveFront();
        }

        private void OnMouseUp(object sender, MouseEventArgs e)
        {
            windowIsDragging = false;
            dragging = IntPtr.Zero;
            BringToPassiveFront();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {

            if (windowIsDragging)
            {
                //Move by distance between click and center
                Left -= Width / 2 - e.X;
                Top -= Height / 2 - e.Y;
                return;
            }

            if (dragging == IntPtr.Zero)
                return;

            var thumb = mgr.findThumb(dragging);
            var now = DateTime.Now.Ticks;
            if (thumb != null)
            {
                if ((now - lastClickTime) > 2500000) {
                    var pc = new Win32.WINDOWPLACEMENT();
                    Win32.GetWindowPlacement(dragging, ref pc);

                    //use 1 if already maximized
                    if (pc.showCmd == 3) {
                        pc.showCmd = 1;
                        Win32.SetWindowPlacement(dragging, ref pc);
                        lastClickTime = now;
                        return;
                    }
                    thumb.centerOn(e.X, e.Y);
                }
            }
            BringToPassiveFront();
        }

        public void OnResize(object sender, EventArgs e)
        {
            var bh = (Height - ClientRectangle.Height);
            Height = (int)(ClientRectangle.Width / mgr.aspect)+bh;

            //FIXME draw screen borders and wallpaper
        }

        private void OnClose(object sender, FormClosingEventArgs e)
        {
            if(!closingAllowed)
                e.Cancel = true;
        }
    }
}
