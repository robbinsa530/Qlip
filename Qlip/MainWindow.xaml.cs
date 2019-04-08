using System;
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
        /// Used for capturing system clip events
        /// </summary>
        private IntPtr _nextClipboardViewer;

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
        public MainWindow()
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
            _model = new Model();

            _sponge = new SpongeWindow();
            _sponge.WndProcCalled += (s, e) => ProcessMessage(e);
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
                case KeyCodes.WM_DRAWCLIPBOARD:
                    if (!_pasting) { HandleCopy(); }
                    ClipboardListener.SendMessage(_nextClipboardViewer, message.Msg, message.WParam, message.LParam);
                    break;
                case KeyCodes.WM_CHANGECBCHAIN:
                    if (message.WParam == _nextClipboardViewer)
                        _nextClipboardViewer = message.LParam;
                    else
                        ClipboardListener.SendMessage(_nextClipboardViewer, message.Msg, message.WParam, message.LParam);
                    break;
            }
        }

        /// <summary>
        /// Handler for when copy hot key is pressed
        /// </summary>
        private static void HandleCopy(bool second=false)
        {
            try
            {
                if (Clipboard.ContainsText())
                {
                    string cur = _this._model.GetMostRecentClip();
                    string clipContents = Clipboard.GetText(TextDataFormat.UnicodeText);
                    if (!cur.Equals(clipContents))
                    {
                        _this._model.AddNewClip(Clipboard.GetText(TextDataFormat.UnicodeText));
                        _this._model.Reset();
                    }
                }
            }
            catch (Exception e)
            {
                if (!second)
                {
                    HandleCopy(true);
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

            _nextClipboardViewer = ClipboardListener.SetClipboardViewer(_sponge.Handle);

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
            if (_model.PasteTimeout() > 0)
                _timer.Change(_model.PasteTimeout() * 1000, Timeout.Infinite);
        }

        /// <summary>
        /// Cancel the timer used for auto pasting
        /// </summary>
        private void CancelTimer()
        {
            if (_model.PasteTimeout() > 0)
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
            ClipboardListener.ChangeClipboardChain(_sponge.Handle, _nextClipboardViewer);
        }

        ~MainWindow()
        {
            Dispose();
        }

        public void Dispose()
        {
            _hotKeyListenerPaste.Unregiser();
            ClipboardListener.ChangeClipboardChain(_sponge.Handle, _nextClipboardViewer);
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
