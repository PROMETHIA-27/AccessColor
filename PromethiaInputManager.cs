using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AccessColor
{
    public static class PromethiaInputManager
    {
        private const Int32 WH_KEYBOARD_LL = 13;
        private const Int32 WM_KEYDOWN = 0x0100;
        private const Int32 WM_KEYUP = 0x0101;
        private const Int32 WM_SYSKEYDOWN = 0x0104;
        private const Int32 WM_SYSKEYUP = 0x0105;
        private static readonly LowLevelKeyboardProc _proc = HookCallback;
        private static readonly IntPtr _hookID = IntPtr.Zero;

        static PromethiaInputManager()
        {
            _hookID = SetHook(_proc); //Hook for global keypresses, because we want to see input even if app is not focused
            AppDomain.CurrentDomain.ProcessExit += (sender, args) => _ = UnhookWindowsHookEx(_hookID);

            //Prevent an annoying beep on alt keycombos (must be done to all windows)
            Keyboard.AddKeyDownHandler(MainWindow.Instance!, (sender, args) => args.Handled = true);
            Keyboard.AddKeyDownHandler(MainWindow.Instance!.SettingsWin, (sender, args) => args.Handled = true);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule!)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName!), 0);
            }
        }

        public static Boolean IsKeyPressed(Key keycode)
        {
            return pressedKeys.ContainsKey(keycode) && pressedKeys[keycode];
        }

        public delegate void KeyDownCallback(Key key);
        public static event KeyDownCallback? KeyDownEvent;

        public delegate void KeyUpCallback(Key key);
        public static event KeyUpCallback? KeyUpEvent;

        public delegate void KeyPressCallback(Key key);
        public static event KeyPressCallback? KeyPressEvent;

        private delegate IntPtr LowLevelKeyboardProc(Int32 nCode, IntPtr wParam, IntPtr lParam);

        private static readonly Dictionary<Key, Boolean> pressedKeys = new();
        private static IntPtr HookCallback(Int32 nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYDOWN || wParam == (IntPtr)WM_SYSKEYDOWN))
            {
                var vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                if (!pressedKeys.TryGetValue(key, out var keyVal) || keyVal == false)
                {
                    pressedKeys[key] = true;
                    KeyPressEvent?.Invoke(key);
                    KeyDownEvent?.Invoke(key);
                }
                else
                {
                    pressedKeys[key] = true;
                    KeyDownEvent?.Invoke(key);
                }
            }
            if (nCode >= 0 && (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP))
            {
                var vkCode = Marshal.ReadInt32(lParam);
                Key key = KeyInterop.KeyFromVirtualKey(vkCode);

                pressedKeys[key] = false;
                KeyUpEvent?.Invoke(key);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(Int32 idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, UInt32 dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, Int32 nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(String lpModuleName);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern Boolean GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        private struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        }

        public static (Int32 x, Int32 y) GetMousePos()
        {
            var w32Mouse = new Win32Point();
            _ = GetCursorPos(ref w32Mouse);
            return new(w32Mouse.X, w32Mouse.Y);
        }
    }
}
