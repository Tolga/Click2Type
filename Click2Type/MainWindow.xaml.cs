using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Click2Type
{
    public partial class MainWindow
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;

        //Modifiers:
        private const uint MOD_NONE = 0x0000; //(none)
        private const uint MOD_ALT = 0x0001; //ALT
        private const uint MOD_CONTROL = 0x0002; //CTRL
        private const uint MOD_SHIFT = 0x0004; //SHIFT
        private const uint MOD_WIN = 0x0008; //WINDOWS
        private const uint VK_T = 0x54; // T
        private const uint VK_ESCAPE = 0x1B; // ESCAPE

        private Point _firstPosition;
        private Control _movingObject;

        public MainWindow()
        {
            InitializeComponent();
            PreviewMouseLeftButtonDown += Canvas_OnClick;
        }

        private IntPtr _windowHandle;
        private HwndSource _source;
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            _windowHandle = new WindowInteropHelper(this).Handle;
            _source = HwndSource.FromHwnd(_windowHandle);
            _source.AddHook(HwndHook);

            RegisterHotKey(_windowHandle, HOTKEY_ID, MOD_CONTROL, VK_T); //CTRL + T
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            switch (msg)
            {
                case WM_HOTKEY:
                    switch (wParam.ToInt32())
                    {
                        case HOTKEY_ID:
                            int vkey = ((int)lParam >> 16) & 0xFFFF;
                            if (vkey == VK_T)
                            {
                                WindowState = WindowState == WindowState.Minimized ? WindowState.Maximized : WindowState.Minimized;
                            }
                            break;
                    }
                    break;
            }
            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            _source.RemoveHook(HwndHook);
            UnregisterHotKey(_windowHandle, HOTKEY_ID);
            base.OnClosed(e);
        }

        private void Control_OnRelease(object sender, MouseButtonEventArgs e)
        {
            _movingObject = null;
        }

        private void Control_OnClick(object sender, MouseButtonEventArgs e)
        {
            _movingObject = sender as Control;
            _firstPosition = e.GetPosition(_movingObject);
        }

        private void Control_OnMove(object sender, MouseEventArgs e)
        {
            if (_movingObject == null) return;
            if (e.LeftButton != MouseButtonState.Pressed) return;

            _movingObject.SetValue(LeftProperty, e.GetPosition(_movingObject).X - _firstPosition.X);
            _movingObject.SetValue(TopProperty, e.GetPosition(_movingObject).Y - _firstPosition.Y);
        }

        private void Canvas_OnClick(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource.ToString() != "System.Windows.Controls.Canvas") return;

            var mousePosition = e.GetPosition(_movingObject);
            var txtb = new TextBox
            {
                Height = 28,
                Width = 200,
                Padding = new Thickness(4),
                RenderTransform = new TranslateTransform(mousePosition.X, mousePosition.Y),
                Text = "Click 2 Type",
                Background = new SolidColorBrush(Colors.Transparent),
                Foreground = new SolidColorBrush(Colors.Black),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };

            txtb.PreviewMouseLeftButtonDown += Control_OnClick;
            txtb.PreviewMouseLeftButtonUp += Control_OnRelease;
            txtb.PreviewMouseMove += Control_OnMove;

            Canvas2Type.Children.Add(txtb);
        }
    }
}
