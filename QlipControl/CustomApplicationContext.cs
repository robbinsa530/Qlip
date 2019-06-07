using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Reflection;

using QlipPreferences;
using Qlip;

namespace QlipControl
{

    /// <summary>
    /// Framework for running application as a tray app.
    /// </summary>
    /// <remarks>
    /// Tray app code adapted from "Creating Applications with NotifyIcon in Windows Forms", Jessica Fosler,
    /// http://windowsclient.net/articles/notifyiconapplications.aspx
    /// </remarks>
    public class CustomApplicationContext : ApplicationContext
    {
        /// <summary>
        /// Icon
        /// </summary>
        private static readonly string IconFileName = "qlip.ico";

        /// <summary>
        /// Tooltip for notifyicon
        /// </summary>
        private static readonly string DefaultTooltip = "Qlip Clipboard Manager";

        /// <summary>
        /// To display whether a user can start or stop Qlip (depending on
        /// if it is currently started or stopped)
        /// </summary>
        private string StartStopText;

        /// <summary>
        /// The actual qlip main window form
        /// </summary>
        private System.Windows.Window qlipForm;

        /// <summary>
        /// The qlip preferences form
        /// </summary>
        private System.Windows.Window qlipConfigForm;

        /// <summary>
        /// A list of components to dispose when the context is disposed
        /// </summary>
        private System.ComponentModel.IContainer components;

        /// <summary>
        /// The icon that sits in the system tray
        /// </summary>
        private NotifyIcon notifyIcon;

        /// <summary>
        /// Data model
        /// </summary>
        private Model model;

        /// <summary>
		/// This class should be created and passed into Application.Run( ... )
		/// </summary>
		public CustomApplicationContext() 
		{
            StartStopText = "Start";
            model = new Model();
            InitializeContext();
            ShowQlipForm();
		}

        /// <summary>
        /// Create a context menu item with text and onclick event
        /// </summary>
        /// <param name="displayText">Text to display</param>
        /// <param name="tooltip">Tooltip to display on hover</param>
        /// <param name="eventHandler">OnClick handler</param>
        /// <returns>ToolStripMenuItem to be placed in a context menu</returns>
        private ToolStripMenuItem ToolStripMenuItemWithHandler(
            string displayText, EventHandler eventHandler)
        {
            var item = new ToolStripMenuItem(displayText);
            if (eventHandler != null) { item.Click += eventHandler; }

            item.Image = null;
            return item;
        }

        /// <summary>
        /// Build the context menu from scratch each time it is opened.
        /// Allows it to be dynamically built with different text/params
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = false;
            notifyIcon.ContextMenuStrip.Items.Clear();
            notifyIcon.ContextMenuStrip.Items.AddRange(
                new ToolStripItem[] {
                    ToolStripMenuItemWithHandler("&" + StartStopText + " Qlip", startStopQlip_Click),
                    ToolStripMenuItemWithHandler("&Open Preferences", openQlipConfig_Click)
                });
            notifyIcon.ContextMenuStrip.Items.Add(new ToolStripSeparator());
            notifyIcon.ContextMenuStrip.Items.Add(ToolStripMenuItemWithHandler("&Exit", exitItem_Click));
        }

        /// <summary>
        /// Open preferences on double click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowConfigForm(); 
        }

        /// <summary>
        /// Open context menu when notify icon is clicked
        /// From http://stackoverflow.com/questions/2208690/invoke-notifyicons-context-menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
                mi.Invoke(notifyIcon, null);
            }
        }

        /// <summary>
        /// Start Qlip
        /// </summary>
        private void ShowQlipForm()
        {
            if (qlipForm == null)
            {
                if (model != null)
                {
                    model.Reset();
                }
                qlipForm = new Qlip.MainWindow(model);

                qlipForm.Closed += qlipProcess_Closed; // avoid reshowing a disposed form
                ElementHost.EnableModelessKeyboardInterop(qlipForm);
                qlipForm.Show();
                StartStopText = "Stop";
            }
            else if (StartStopText.Equals("Start"))
            {
                qlipForm.Activate();
                StartStopText = "Stop";
            }
            else
            {
                qlipForm.Close();
                StartStopText = "Start";
            }
        }

        /// <summary>
        /// Start Qlip
        /// </summary>
        private void ShowConfigForm()
        {
            if (qlipConfigForm == null)
            {
                qlipConfigForm = new QlipPreferences.QlipPreferencesWindow();
                qlipConfigForm.Closed += qlipConfig_Closed; // avoid reshowing a disposed form
                ElementHost.EnableModelessKeyboardInterop(qlipConfigForm);
                qlipConfigForm.Show();
             }
        }

        /// <summary>
        /// Start or stop Qlip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startStopQlip_Click(object sender, EventArgs e)
        {
            ShowQlipForm();
        }

        /// <summary>
        /// Open the Qlip configuration panel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openQlipConfig_Click(object sender, EventArgs e)
        {
            ShowConfigForm();
        }

        // null out the forms so we know to create a new one.
        private void qlipProcess_Closed(object sender, EventArgs e)
        {
            qlipForm = null;
            StartStopText = "Start";
        }
        private void qlipConfig_Closed(object sender, EventArgs e)
        {
            qlipConfigForm = null;
        }

        private void InitializeContext()
        {
            components = new System.ComponentModel.Container();
            notifyIcon = new NotifyIcon(components)
                {
                    ContextMenuStrip = new ContextMenuStrip(),
                    Icon = new Icon(IconFileName),
                    Text = DefaultTooltip,
                    Visible = true
                };
            notifyIcon.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
            notifyIcon.DoubleClick += notifyIcon_DoubleClick;
            notifyIcon.MouseUp += notifyIcon_MouseUp;
        }

        /// <summary>
		/// When the application context is disposed, dispose things like the notify icon.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose( bool disposing )
		{
			if (disposing && components != null) { components.Dispose(); }
		}

		/// <summary>
		/// When the exit menu item is clicked, make a call to terminate the ApplicationContext.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        private void exitItem_Click(object sender, EventArgs e) 
		{
			ExitThread();
        }

        /// <summary>
        /// If we are presently showing a form, clean it up.
        /// </summary>
        protected override void ExitThreadCore()
        {
            // before we exit, let forms clean themselves up.
            if (qlipForm != null) { qlipForm.Close(); }
            if (qlipConfigForm != null) { qlipConfigForm.Close(); }

            notifyIcon.Visible = false; // should remove lingering tray icon
            base.ExitThreadCore();
        }
    }
}
