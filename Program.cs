using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Diagnostics;
using System.Security.Principal;
using System.IO;

namespace alibijimexe
{
    class Program
    {
        [DllImport("user32.dll")] static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
        [DllImport("gdi32.dll")] static extern bool PatBlt(IntPtr hdc, int nXLeft, int nYLeft, int nWidth, int nHeight, uint dwRop);
        [DllImport("gdi32.dll")] static extern bool StretchBlt(IntPtr hdcDest, int nXOriginDest, int nYOriginDest, int nWidthDest, int nHeightDest, IntPtr hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc, uint dwRop);
        [DllImport("user32.dll")] static extern IntPtr LoadIcon(IntPtr hInstance, IntPtr lpIconName);
        [DllImport("user32.dll")] static extern bool DrawIcon(IntPtr hDC, int X, int Y, IntPtr hIcon);
        [DllImport("user32.dll")] static extern int GetSystemMetrics(int nIndex);
        [DllImport("user32.dll")] static extern bool BlockInput(bool fBlockIt);

        static void Main(string[] args)
        {
            WindowsPrincipal principal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
            {
                ProcessStartInfo i = new ProcessStartInfo(Process.GetCurrentProcess().MainModule.FileName);
                i.Verb = "runas";
                i.UseShellExecute = true;
                try { Process.Start(i); } catch { }
                return;
            }

            BlockInput(true);

            new Thread(() => {
                Thread.Sleep(60000);
                BlockInput(false);
                Environment.Exit(0);
            }).Start();

            IntPtr hdc = GetDC(IntPtr.Zero);
            int w = GetSystemMetrics(0);
            int h = GetSystemMetrics(1);
            Random rng = new Random();

            string b64 = "data:image/jpeg;base64,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";

            Image p_img = null;
            try
            {
                string cleanB64 = b64.Contains(",") ? b64.Split(',')[1] : b64;
                byte[] imgBytes = Convert.FromBase64String(cleanB64);
                using (MemoryStream ms = new MemoryStream(imgBytes))
                {
                    p_img = Image.FromStream(ms);
                }
            }
            catch { }

            IntPtr[] icons = {
                LoadIcon(IntPtr.Zero, (IntPtr)32513),
                LoadIcon(IntPtr.Zero, (IntPtr)32514),
                LoadIcon(IntPtr.Zero, (IntPtr)32515),
                LoadIcon(IntPtr.Zero, (IntPtr)32516)
            };

            new Thread(ShowCrazyMessages).Start();

            double angle = 0;
            long startTime = DateTimeOffset.Now.ToUnixTimeSeconds();

            while (DateTimeOffset.Now.ToUnixTimeSeconds() - startTime < 60)
            {
                PatBlt(hdc, 0, 0, w, h, 0x550009);
                int zoom = 15;
                int ox = rng.Next(-30, 31);
                int oy = rng.Next(-30, 31);
                StretchBlt(hdc, zoom + ox, zoom + oy, w - (zoom * 2), h - (zoom * 2), hdc, -3, -3, w + 6, h + 6, 0xCC0020);

                for (int i = 0; i < 5; i++)
                    DrawIcon(hdc, rng.Next(w), rng.Next(h), icons[rng.Next(icons.Length)]);

                if (p_img != null)
                {
                    using (Graphics g = Graphics.FromHdc(hdc))
                    {
                        int ix = (int)(w / 2 + Math.Cos(angle) * (w / 3) - 100);
                        int iy = (int)(h / 2 + Math.Sin(angle) * (h / 3) - 100);
                        g.DrawImage(p_img, ix, iy, 250, 250);
                    }
                }
                angle += 0.3;
                Thread.Sleep(10);
            }
            BlockInput(false);
            ReleaseDC(IntPtr.Zero, hdc);
        }

        static void ShowCrazyMessages()
        {
            Random r = new Random();
            string[,] msgs = {
                { "alibijim.exe", "noluyo la", "error" },
                { "alibijim.exe", "zıbab ıvj", "info" },
                { "Windows Defender", "alibijim.exe ile savaşıyor.", "warning" },
                { "alibijim.exe", "vuhuuuuu", "error" },
                { "alibijim.exe", "ıvj ıvj ıvj zıbaaaaaaaaaaappp", "error" },
                { "Windows Defender", "merhaba ben alibijim", "error" },
                { "alibijim.exe", "öpüyorum bal dudaklarımla:)", "error" }
            };

            while (true)
            {
                Thread.Sleep(3000);
                int i = r.Next(msgs.GetLength(0));
                MessageBoxIcon iconType = MessageBoxIcon.Error;
                if (msgs[i, 2] == "info") iconType = MessageBoxIcon.Information;
                if (msgs[i, 2] == "warning") iconType = MessageBoxIcon.Warning;
                new Thread(() => { MessageBox.Show(msgs[i, 1], msgs[i, 0], MessageBoxButtons.OK, iconType); }).Start();
            }
        }
    }
}