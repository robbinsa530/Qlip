using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Qlip.Native;
using System.Windows.Interop;

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
        /// 
        /// </summary>
        private IntPtr _nextClipboardViewer;

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
                    if (!Clipboard.IsCurrent((new DataObject(_this._model.GetMostRecentClip()))))
                        _this._model.AddNewClip(Clipboard.GetText(TextDataFormat.Text));
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
        /// Capture key-downs when app is open
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    _pasting = true;
                    Clipboard.SetText(CurrentClip);
                    _model.Reset();
                    CurrentClip = null;
                    CurrentLabel = _model.GetCurrentForDisplay() + "/" + _model.GetCountForDisplay();
                    this.Hide();
                    System.Windows.Forms.SendKeys.SendWait("^{v}");
                    _pasting = false;
                    e.Handled = true;
                    break;
                case Key.V:
                case Key.Tab:
                case Key.Right:
                    _model.Next();
                    CurrentClip = _model.GetCurrentClip();
                    CurrentLabel = _model.GetCurrentForDisplay() + "/" + _model.GetCountForDisplay();
                    e.Handled = true;
                    break;
                case Key.Left:
                    _model.Prev();
                    CurrentClip = _model.GetCurrentClip();
                    CurrentLabel = _model.GetCurrentForDisplay() + "/" + _model.GetCountForDisplay();
                    e.Handled = true;
                    break;
                case Key.Escape:
                    _model.Reset();
                    CurrentLabel = _model.GetCurrentForDisplay() + "/" + _model.GetCountForDisplay();
                    this.Hide();
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
            _model.Reset();
            CurrentLabel = _model.GetCurrentForDisplay() + "/" + _model.GetCountForDisplay();
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
