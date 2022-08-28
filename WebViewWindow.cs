﻿using System.Runtime.InteropServices;

namespace DownloadManager
{
    public partial class WebViewWindow : Form
    {
        #region DLL Import
        [DllImport("DwmApi")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, int[] attrValue, int attrSize);

        protected override void OnHandleCreated(EventArgs e)
        {
            if (DwmSetWindowAttribute(Handle, 19, new[] { 1 }, 4) != 0)
                DwmSetWindowAttribute(Handle, 20, new[] { 1 }, 4);
        }
        #endregion

        string url;
        bool isLoading = true;

        public WebViewWindow(string url, string windowTitle)
        {
            InitializeComponent();
            this.Text = windowTitle;
            this.url = url;
            textBox1.Text = url;

            // Initialize webView
            webView.NavigationStarting += webView_NavigationStarted;
            webView.NavigationCompleted += webView_NavigationCompleted;
        }

        async private void WebViewWindow_Load(object sender, EventArgs e)
        {
            // Navigate to url
            try
            {
                await webView.EnsureCoreWebView2Async();
                webView.CoreWebView2.Navigate(url);
            }
            catch (Exception ex)
            {
                DarkMessageBox msg = new DarkMessageBox(ex.ToString() + ": " + ex.Message + Environment.NewLine + ex.StackTrace, "WebView Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error, true);
                msg.ShowDialog();
                webView.BackgroundImage = Properties.Resources.error;
                webView.BackgroundImageLayout = ImageLayout.Stretch;
            }
        }

        void webView_NavigationStarted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs e)
        {
            // Update progress bar
            progressBar1.Style = ProgressBarStyle.Marquee;

            // Set loading to true
            isLoading = true;

            // Set stop/refresh button image
            button3.BackgroundImage = Properties.Resources.stop;
        }

        void webView_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            // Set loading to false
            isLoading = false;

            // Set stop/refresh button image
            button3.BackgroundImage = Properties.Resources.refresh;

            // Check if user can go back
            if (webView.CoreWebView2.CanGoBack == true)
            {
                // Enable back button
                button1.Enabled = true;
            }
            else
            {
                // Disable back button
                button1.Enabled = false;
            }

            // Check if the user can go forward
            if (webView.CoreWebView2.CanGoForward == true)
            {
                // Enable forward button
                button2.Enabled = true;
            }
            else
            {
                // Disable forward button
                button2.Enabled = false;
            }

            // Update url
            textBox1.Text = webView.CoreWebView2.Source;

            // Update progress bar
            progressBar1.Style = ProgressBarStyle.Blocks;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Back
            if (webView.CoreWebView2 == null)
            {
                DarkMessageBox msg = new DarkMessageBox("CoreWebView2 is null - WebView is not properly initialized.\nPlease wait a few seconds and if the problem persists, restart the application.", "WebView Error", MessageBoxButtons.OK, MessageBoxIcon.Error, true);
                msg.ShowDialog();
            }
            else
            {
                webView.CoreWebView2.GoBack();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Forward
            if (webView.CoreWebView2 == null)
            {
                DarkMessageBox msg = new DarkMessageBox("CoreWebView2 is null - WebView is not properly initialized.\nPlease wait a few seconds and if the problem persists, restart the application.", "WebView Error", MessageBoxButtons.OK, MessageBoxIcon.Error, true);
                msg.ShowDialog();
            }
            else
            {
                webView.CoreWebView2.GoForward();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Refresh
            if (webView.CoreWebView2 == null)
            {
                DarkMessageBox msg = new DarkMessageBox("CoreWebView2 is null - WebView is not properly initialized.\nPlease wait a few seconds and if the problem persists, restart the application.", "WebView Error", MessageBoxButtons.OK, MessageBoxIcon.Error, true);
                msg.ShowDialog();
            }
            else
            {
                // Check if page is loading
                if (isLoading == true)
                {
                    webView.CoreWebView2.Stop();
                }
                else
                {
                    webView.CoreWebView2.Reload();
                }
            }
        }
    }
}
