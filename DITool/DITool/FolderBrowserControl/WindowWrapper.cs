using System;

namespace DITool.FolderBrowserControl
{
    class WindowWrapper : System.Windows.Forms.IWin32Window
    {
        private readonly IntPtr _hwnd;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="handle">Handle to wrap</param>
        public WindowWrapper(IntPtr handle)
        {
            _hwnd = handle;
        }

        /// <summary>
        /// Original ptr
        /// </summary>
        public IntPtr Handle
        {
            get { return _hwnd; }
        }
    }
}
