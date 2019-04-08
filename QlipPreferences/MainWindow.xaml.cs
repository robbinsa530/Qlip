using System.ComponentModel;
using System.Windows;

using Qlip;

namespace QlipPreferences
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged, IDataErrorInfo
    {
        public string SaveCount
        {
            get { return _saveCountStr; }
            set
            {
                _saveCountStr = value; // Always set string
                int temp;
                if (int.TryParse(value, out temp))
                {
                    if (temp > 0)
                        _saveCount = temp;
                }
                OnPropertyChanged("SaveCount");
                return;
            }
        }
        private int _saveCount;
        private string _saveCountStr;

        public string PasteTimeout
        {
            get { return _pasteTimeoutStr; }
            set
            {
                _pasteTimeoutStr = value; // Always set string
                int temp;
                if (int.TryParse(value, out temp))
                {
                    if (temp > 0)
                        _pasteTimeout = temp;
                }
                OnPropertyChanged("PasteTimeout");
                return;
            }
        }
        private int _pasteTimeout;
        private string _pasteTimeoutStr;

        public bool ResetOnPaste
        {
            get { return _resetOnPaste; }
            set
            {
                _resetOnPaste = value;
                OnPropertyChanged("ResetOnPaste");
                return;
            }
        }
        private bool _resetOnPaste;

        public bool ResetOnCancel
        {
            get { return _resetOnCancel; }
            set
            {
                _resetOnCancel = value;
                OnPropertyChanged("ResetOnCancel");
                return;
            }
        }
        private bool _resetOnCancel;

        public bool MovePastedToFront
        {
            get { return _movePastedToFront; }
            set
            {
                _movePastedToFront = value;
                OnPropertyChanged("MovePastedToFront");
                return;
            }
        }
        private bool _movePastedToFront;

        private ConfigHelper config;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public MainWindow()
        {
            config = new ConfigHelper();
            SaveCount = config.config.save_count.ToString();
            PasteTimeout = config.config.paste_timeout.ToString();
            ResetOnPaste = config.config.reset_on_paste;
            ResetOnCancel = config.config.reset_on_cancel;
            MovePastedToFront = config.config.move_pasted_to_front;

            DataContext = this;
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RestoreDefaultsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        /// <summary>
        /// Used for binding text boxes to error checks
        /// </summary>
        /// <param name="columnName"></param>
        /// <returns></returns>
        public string this[string columnName]
        {
            get
            {
                int num;
                double dblNum;
                if (columnName == "SaveCount")
                {
                    if (!int.TryParse(SaveCount, out num))
                    {
                        return "Must Enter a Whole Number!";
                    }
                    else if (num < 0)
                    {
                        return "Save Count Must be Positive!";
                    }
                }
                if (columnName == "PasteTimeout")
                {
                    if (!double.TryParse(PasteTimeout, out dblNum))
                    {
                        return "Must Enter a Number!";
                    }
                    else if (dblNum < 0)
                    {
                        return "Paste Timeout Must be Positive!";
                    }
                }

                return null;
            }
        }

        public string Error { get { return string.Empty; } }

    }
}
