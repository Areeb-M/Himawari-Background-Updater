using Himawari_Background_Updater.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Himawari_Background_Updater
{
    class Himawari : ApplicationContext
    {
        Container components;
        NotifyIcon notifyIcon;

        ToolStripMenuItem refresh;
        ToolStripMenuItem status;
        ToolStripMenuItem updateTime;
        ToolStripSeparator toolStripSeparator;
        ToolStripMenuItem exit;

        ThreadStart childref;
        Thread childThread;

        System.Windows.Forms.Timer timer;

        bool refreshing = false;

        public Himawari()
        {
            InitializeContext();
            InitializeTimer();
            Refresh_Background(null, null);
        }

        private void InitializeContext()
        {
            components = new Container();
            notifyIcon = new NotifyIcon(components)
                {
                    ContextMenuStrip = new ContextMenuStrip(),
                    Icon = Resources.AppIcon,
                    Text = "Himawari",
                    Visible = true
                };
            refresh = new ToolStripMenuItem("Refresh", null, Refresh_Background);
            status = new ToolStripMenuItem("Disable", null, Enable_Disable_Switch);
            updateTime = new ToolStripMenuItem("XX:XX XX");
            toolStripSeparator = new ToolStripSeparator();
            exit = new ToolStripMenuItem("Exit", null, exitItem_Click);

            notifyIcon.ContextMenuStrip.Items.Add(refresh);
            notifyIcon.ContextMenuStrip.Items.Add(status);
            notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            notifyIcon.ContextMenuStrip.Items.Add(new ToolStripMenuItem("Last Updated"));
            notifyIcon.ContextMenuStrip.Items.Add(updateTime);
            notifyIcon.ContextMenuStrip.Items.Add(toolStripSeparator);
            notifyIcon.ContextMenuStrip.Items.Add(exit);
        }

        private void InitializeTimer()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 10 * 60 * 1000;

            timer.Tick += Refresh_Background;
            timer.Enabled = true;
        }

        private void Refresh_Background(object sender, EventArgs e)
        {
            if (refreshing)
                return;
            childref = new ThreadStart(Refresh_Thread);
            childThread = new Thread(childref);
            childThread.Start();
        }

        private void Refresh_Thread()
        {
            try
            {
                refreshing = true;

                WebRequest request = WebRequest.Create("http://rammb.cira.colostate.edu/ramsdis/online/images/latest_hi_res/himawari-8/full_disk_ahi_true_color.jpg");
                WebResponse response = request.GetResponse();

                Image image = Image.FromStream(response.GetResponseStream());
                response.Close();

                image.Save("latest.jpg");
                image.Dispose();
                WallpaperUpdater.SetWallpaper(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\latest.jpg");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                refreshing = false;
            }

            updateTime.Text = DateTime.Now.ToString("h\\:mm tt");
            refreshing = false;
        }

        private void Enable_Disable_Switch (object sender, EventArgs e)
        {
            if (status.Text == "Disable")
            {
                timer.Enabled = false;
                status.Text = "Enable";
            } else
            {
                timer.Enabled = true;
                status.Text = "Disable";
            }
        }

        private void exitItem_Click(object sender, EventArgs e)
        {
            ExitThread();
        }

        protected override void ExitThreadCore()
        {
            notifyIcon.Visible = false;
            base.ExitThreadCore();
        }
    }
}
