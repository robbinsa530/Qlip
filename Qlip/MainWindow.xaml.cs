﻿using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Qlip.Native;
using System.Windows.Threading;

namespace Qlip
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Hot Key Listener which will listen for hot key
        /// </summary>
        private HotKey _hotKeyListenerPaste;

        /// <summary>
        /// Data model
        /// </summary>
        private Model _model;

        /// <summary>
        /// Static reference to 'this'
        /// </summary>
        private static MainWindow _this;

        /// <summary>
        /// id for paste listener
        /// </summary>
        private static int _pasteListenerId;

        /// <summary>
        /// Binds model to wndproc
        /// </summary>
        private readonly SpongeWindow _sponge;

        /// <summary>
        /// Whether or not this application is pasting from the clipboard
        /// </summary>
        private bool _pasting;

        /// <summary>
        /// Whether the program just pasted
        /// </summary>
        private bool _pasted = false;

        /// <summary>
        /// Whether the program just exited the clip view screen
        /// </summary>
        private bool _exited = false;

        /// <summary>
        /// Timer used for auto pasting when no action for some time
        /// </summary>
        private Timer _timer;

        /// <summary>
        /// Dispatcher to main thread (used for timer)
        /// </summary>
        private Dispatcher _thisDisp;

        /// <summary>
        /// Current clip to show
        /// </summary>
        public string CurrentClip
        {
            get { return _currentClip; }
            set
            {
                _currentClip = value;
                OnPropertyChanged("CurrentClip");
                return;
            }
        }
        private string _currentClip;

        /// <summary>
        /// Current clip to show
        /// </summary>
        public string CurrentLabel
        {
            get { return _currentLabel; }
            set
            {
                _currentLabel = value;
                OnPropertyChanged("CurrentLabel");
                return;
            }
        }
        private string _currentLabel;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Initializer
        /// </summary>
        public MainWindow(Model model)
        {
            CurrentClip = "";
            CurrentLabel = "1/1";
            _thisDisp = this.Dispatcher;
            _timer = new Timer(_ => 
            {
                _thisDisp.BeginInvoke((Action) (() => { Paste(); }));
            });

            InitializeComponent();
            DataContext = this;

            _pasting = false;
            _this = this;
            _model = model;

            _sponge = new SpongeWindow();
            _sponge.WndProcCalled += (s, e) => ProcessMessage(e);

            HandleCopy(); // Grab existing clip if one exists
        }

        /// <summary>
        /// Process the windows messages coming in to check for 
        /// only the ones we care about
        /// </summary>
        /// <param name="message"></param>
        private void ProcessMessage(System.Windows.Forms.Message message)
        {
            switch (message.Msg)
            {
                case KeyCodes.WM_HOTKEY_MSG_ID:
                    if (message.WParam.ToInt32() == _pasteListenerId)
                    {
                        if (!_pasting) { HandlePaste(); }
                    }
                    break;
                case KeyCodes.WM_CLIPBOARDUPDATE:
                    if (!_pasting) { HandleCopy(); }
                    break;
            }
        }

        /// <summary>
        /// Handler for when copy hot key is pressed
        /// </summary>
        private static void HandleCopy(bool second=false)
        {
            string errCheckPt = "0";
            string clipContents = null;
            try
            {
                errCheckPt = "1";
                if (Clipboard.ContainsText())
                {
                    errCheckPt = "2";
                    string cur = _this._model.GetMostRecentClip();
                    errCheckPt = "3";
                    clipContents = Clipboard.GetText(TextDataFormat.UnicodeText);
                    errCheckPt = "4";
                    if (clipContents != null && !cur.Equals(clipContents))
                    {
                        errCheckPt = "5";
                        _this._model.AddNewClip(clipContents);
                        errCheckPt = "6";
                        _this._model.Reset();
                        errCheckPt = "7";
                    }
                }
            }
            catch (Exception)
            {
                // Check to see if it got into qlip. If so, no problem
                string newCur = _this._model.GetMostRecentClip();
                if (clipContents != null && clipContents.Equals(newCur))
                {
                    return;
                }


                if (!second)
                {
                    HandleCopy(true);
                }
                else
                {
                    string msg = "Failed to capture copied text. ERRCODE=" + errCheckPt + "." + (second ? "1" : "2");
                    MessageBox.Show(msg, "Qlip Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        /// <summary>
        /// Handler for when paste hot key is pressed
        /// </summary>
        private static void HandlePaste()
        {
            if (_this.Visibility.Equals(Visibility.Hidden))
            {
                _this.CurrentClip = _this._model.GetCurrentClip();
                _this.CurrentLabel = _this._model.GetCurrentForDisplay() + "/" + _this._model.GetCountForDisplay();
                _this.Topmost = true;
                _this.Show();
                _this.Activate();

                _this.ResetTimer();
            }
            else
            {
                _this.ResetTimer();
                _this._model.Next();
                _this.CurrentClip = _this._model.GetCurrentClip();
                _this.CurrentLabel = _this._model.GetCurrentForDisplay() + "/" + _this._model.GetCountForDisplay();
            }
        }

        /// <summary>
        /// On load, register hot key listener
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _hotKeyListenerPaste = new HotKey(KeyCodes.CTRL + KeyCodes.SHIFT + KeyCodes.NO_REPEAT,
                                        KeyCodes.V,
                                        _sponge.Handle);
            _pasteListenerId = _hotKeyListenerPaste.getId();

            bool success = ClipboardListener.AddClipboardFormatListener(_sponge.Handle);
            if (!success)
            {
                MessageBox.Show("Error adding clipboard format listener!", "Qlip Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            RegisterPaste();
            this.Hide();
        }

        /// <summary>
        /// Private register function with error checking
        /// </summary>
        private void RegisterPaste()
        {
            if (!_hotKeyListenerPaste.Register())
            {
                if (MessageBox.Show("Failed to register hot key listener for Qlip.",
                                "Error!",
                                MessageBoxButton.YesNo,
                                MessageBoxImage.Error).Equals(MessageBoxResult.Yes))
                {
                    RegisterPaste();
                }
                else
                {
                    this.Close();
                }
            }
        }

        /// <summary>
        /// Perform paste event
        /// </summary>
        private void Paste()
        {
            _pasting = true;
            Clipboard.SetText(CurrentClip);
            if (_model.MovePastedToFront())
            {
                _model.RemoveCurrentClip();
                _model.AddNewClip(CurrentClip);
            }
            if (_model.ResetOnPaste()) { _model.Reset(); }
            CurrentClip = null;
            CurrentLabel = _model.GetCurrentForDisplay() + "/" + _model.GetCountForDisplay();
            _pasted = true;
            this.Hide();
            System.Windows.Forms.SendKeys.SendWait("^{v}");
            _pasting = false;
        }

        /// <summary>
        /// Reset timer used for auto pasting
        /// </summary>
        private void ResetTimer()
        {
            if (_model.PasteTimeoutMS() > 0)
                _timer.Change(_model.PasteTimeoutMS(), Timeout.Infinite);
        }

        /// <summary>
        /// Cancel the timer used for auto pasting
        /// </summary>
        private void CancelTimer()
        {
            if (_model.PasteTimeoutMS() > 0)
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Capture key-downs when app is open
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            ResetTimer();
            switch (e.Key)
            {
                case Key.Enter:
                    CancelTimer();
                    Paste();
                    e.Handled = true;
                    break;
                case Key.V:
                case Key.Tab:
                case Key.Up:
                case Key.Right:
                    _model.Next();
                    CurrentClip = _model.GetCurrentClip();
                    CurrentLabel = _model.GetCurrentForDisplay() + "/" + _model.GetCountForDisplay();
                    e.Handled = true;
                    break;
                case Key.Down:
                case Key.Left:
                    _model.Prev();
                    CurrentClip = _model.GetCurrentClip();
                    CurrentLabel = _model.GetCurrentForDisplay() + "/" + _model.GetCountForDisplay();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    CancelTimer();
                    if (_model.ResetOnCancel()) { _model.Reset(); }
                    CurrentLabel = _model.GetCurrentForDisplay() + "/" + _model.GetCountForDisplay();
                    _exited = true;
                    this.Hide();
                    e.Handled = true;
                    break;
                case Key.Home:
                    _model.Reset();
                    CurrentClip = _model.GetCurrentClip();
                    CurrentLabel = _model.GetCurrentForDisplay() + "/" + _model.GetCountForDisplay();
                    e.Handled = true;
                    break;
                case Key.End:
                    _model.GoToEnd();
                    CurrentClip = _model.GetCurrentClip();
                    CurrentLabel = _model.GetCurrentForDisplay() + "/" + _model.GetCountForDisplay();
                    e.Handled = true;
                    break;
                case Key.Delete:
                case Key.Back:
                case Key.X:
                    _model.RemoveCurrentClip();
                    CurrentClip = _model.GetCurrentClip();
                    CurrentLabel = _model.GetCurrentForDisplay() + "/" + _model.GetCountForDisplay();
                    e.Handled = true;
                    break;
            }
        }

        /// <summary>
        /// On closing, unregister hot key listener
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _hotKeyListenerPaste.Unregiser();
            bool success = ClipboardListener.RemoveClipboardFormatListener(_sponge.Handle);
            if (!success)
            {
                MessageBox.Show("Error removing clipboard format listener!", "Qlip Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //~MainWindow()
        //{
        //    Dispose();
        //}

        public void Dispose()
        {
            _hotKeyListenerPaste.Unregiser();
            bool success = ClipboardListener.RemoveClipboardFormatListener(_sponge.Handle);
            if (!success)
            {
                MessageBox.Show("Error removing clipboard format listener!", "Qlip Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// On lost focus, close and reset window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_model.ResetOnCancel() && !_exited && !_pasted) { _model.Reset(); }
            CurrentLabel = _model.GetCurrentForDisplay() + "/" + _model.GetCountForDisplay();
            _pasted = _exited = false;
            this.Hide();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            // Console.WriteLine("?");
        }

        private void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            // Console.WriteLine("?");
        }
    }
}
