using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace FolderCompareTool
{
    public static class FileIconHelper
    {
        /// <summary>
        /// 获取文件的系统图标
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="iconSize">图标尺寸</param>
        /// <returns>文件的图标</returns>
        public static Icon? GetFileIcon(string filePath, IconSize iconSize = IconSize.Small)
        {
            var flags = SHGFI_ICON | (iconSize == IconSize.Small ? SHGFI_SMALLICON : SHGFI_LARGEICON);
            var shinfo = new SHFILEINFO();

            SHGetFileInfo(filePath, 0, out shinfo, (uint)Marshal.SizeOf(shinfo), flags);

            if (shinfo.hIcon == IntPtr.Zero) return null;

            var icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone(); //克隆图标以避免资源释放问题
            DestroyIcon(shinfo.hIcon); //立即释放原生资源
            return icon;
        }

        public static BitmapSource? Icon2Image(Icon? icon)
        {
            if (icon == null) return null;

            return Imaging.CreateBitmapSourceFromHBitmap(
                icon.ToBitmap().GetHbitmap(),
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());
        }

        #region Win32 Interop
        private const uint SHGFI_ICON = 0x100;
        private const uint SHGFI_LARGEICON = 0x0;
        private const uint SHGFI_SMALLICON = 0x1;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, out SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool DestroyIcon(IntPtr hIcon);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }
        #endregion
    }

    public enum IconSize
    {
        /// <summary>
        /// 32x32 大图标
        /// </summary>
        Large,

        /// <summary>
        /// 16x16 小图标
        /// </summary>
        Small
    }
}
