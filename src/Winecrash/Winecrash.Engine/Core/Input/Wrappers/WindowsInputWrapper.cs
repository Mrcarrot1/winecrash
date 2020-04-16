﻿using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Winecrash.Engine
{
    internal class WindowsInputWrapper : IInputWrapper
    {
        public OSPlatform CorrespondingOS { get; } = OSPlatform.Windows;

        public bool GetKey(Keys key)
        {
            return GetAsyncKeyState(key) != 0;
        }

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(Keys vKey);
    }
}