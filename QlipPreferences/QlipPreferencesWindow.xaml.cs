using System.ComponentModel;
using System.Windows;

using Qlip;

namespace QlipPreferences
{
    /// <summary>
    /// Interaction logic for QlipPreferencesWindow.xaml
    /// </summary>
    public partial class QlipPreferencesWindow : Window, INotifyPropertyChanged, IDataErrorInfo
    {
        private const double MAX_PASTE_TIMEOUT = 600;
        private const double MAX_SAVE_COUNT = 256;

        public string SaveCount
        {
            get { return _saveCountStr; }
            set
            {
                _saveCountStr = value; // Always set string
                int temp;
                if (int.TryParse(value, out temp))
                {
                    if (temp > 0 && temp < MAX_SAVE_COUNT)
                        config.config.save_count = temp;
                }
                OnPropertyChanged("SaveCount");
                return;
            }
        }
        private string _saveCountStr;

        public string PasteTimeout
        {
            get { return _pasteTimeoutStr; }
            set
            {
                double temp;
                if (double.TryParse(value, out temp))
                {
                    string[] split = value.Split('.');
                    if (split.Length > 1 && split[1].Length > 3)
                        return;
                    if (temp > 0 && temp < MAX_PASTE_TIMEOUT)
                        config.config.paste_timeout = temp;
                }
                _pasteTimeoutStr = value; // Always set string

                OnPropertyChanged("PasteTimeout");
                return;
            }
        }
        private string _pasteTimeoutStr;

        public bool ResetOnPaste
        {
            get { return config.config.reset_on_paste; }
            set
            {
                config.config.reset_on_paste = value;
                OnPropertyChanged("ResetOnPaste");
                return;
            }
        }

        public bool ResetOnCancel
        {
            get { return config.config.reset_on_cancel; }
            set
            {
                config.config.reset_on_cancel = value;
                OnPropertyChanged("ResetOnCancel");
                return;
            }
        }

        public bool MovePastedToFront
        {
            get { return config.config.move_pasted_to_front; }
            set
            {
                config.config.move_pasted_to_front = value;
                OnPropertyChanged("MovePastedToFront");
                return;
            }
        }

        public bool AutoPaste
        {
            get { return _autoPaste; }
            set
            {
                _autoPaste = value;
                if (!value) { config.config.paste_timeout = -1; }
                OnPropertyChanged("AutoPaste");
                return;
            }
        }
        private bool _autoPaste;

        public bool FieldsAreValid
        {
            get { return _fieldsAreValid; }
            set
            {
                _fieldsAreValid = value;
                OnPropertyChanged("FieldsAreValid");
                return;
            }
        }
        private bool _fieldsAreValid;
        private bool _saveCountValid;
        private bool _pasteTimeoutValid;

        private ConfigHelper config;

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public QlipPreferencesWindow()
        {
            config = new ConfigHelper();
            SaveCount = config.config.save_count.ToString();
            double pto = config.config.paste_timeout;
            PasteTimeout = pto > 0 ? pto.ToString() : ConfigHelper.defaultPasteTimeout.ToString();
            AutoPaste = pto > 0;
            ResetOnPaste = config.config.reset_on_paste;
            ResetOnCancel = config.config.reset_on_cancel;
            MovePastedToFront = config.config.move_pasted_to_front;
            FieldsAreValid = _saveCountValid = _pasteTimeoutValid = true;

            DataContext = this;
            InitializeComponent();
        }

        private void UpdateFieldsAreValid()
        {
            FieldsAreValid = _pasteTimeoutValid && _saveCountValid;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            config.Save();
            this.Close();
        }

        private void RestoreDefaultsButton_Click(object sender, RoutedEventArgs e)
        {
            config.SetDefaults();
            config.Save();
            this.Close();
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
                string retStr = null;
                if (columnName == "SaveCount")
                {
                    if (!int.TryParse(SaveCount, out num))
                    {
                        _saveCountValid = false;
                        retStr = "Must Enter a Whole Number!";
                    }
                    else if (num < 0)
                    {
                        _saveCountValid = false;
                        retStr = "Save Count Must be Positive!";
                    }
                    else if (num > MAX_SAVE_COUNT)
                    {
                        _saveCountValid = false;
                        retStr = "Save Count Too High!";
                    }
                    else
                    {
                        _saveCountValid = true;
                    }
                }
                else if (columnName == "PasteTimeout")
                {
                    if (!double.TryParse(PasteTimeout, out dblNum))
                    {
                        _pasteTimeoutValid = false;
                        retStr = "Must Enter a Number!";
                    }
                    else if (dblNum < 0)
                    {
                        _pasteTimeoutValid = false;
                        retStr = "Paste Timeout Must be Positive!";
                    }
                    else if (dblNum > MAX_PASTE_TIMEOUT)
                    {
                        _pasteTimeoutValid = false;
                        retStr = "Paste Timeout Too High!";
                    }
                    else
                    {
                        _pasteTimeoutValid = true;
                    }
                }
                UpdateFieldsAreValid();
                return retStr;
            }
        }

        public string Error { get { return string.Empty; } }

    }
}
