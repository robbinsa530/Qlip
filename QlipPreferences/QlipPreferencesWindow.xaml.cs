using System.ComponentModel;
using System.Windows;

using Qlip;
using System;

namespace QlipPreferences
{
    /// <summary>
    /// Interaction logic for QlipPreferencesWindow.xaml
    /// </summary>
    public partial class QlipPreferencesWindow : Window, INotifyPropertyChanged, IDataErrorInfo
    {
        private const double MAX_PASTE_TIMEOUT = 600;
        private const double MAX_SAVE_COUNT = 256;
        public const double DEFAULT_PASTE_TIMEOUT = 2.0;

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
                        _saveCount = temp;
                }
                OnPropertyChanged("SaveCount");
                return;
            }
        }
        private string _saveCountStr;
        private int _saveCount;

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
                        _pasteTimeout = temp;
                }
                _pasteTimeoutStr = value; // Always set string

                OnPropertyChanged("PasteTimeout");
                return;
            }
        }
        private string _pasteTimeoutStr;
        private double _pasteTimeout;

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

        public bool AutoPaste
        {
            get { return _autoPaste; }
            set
            {
                _autoPaste = value;
                if (!value) { _pasteTimeout = -1; }
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
            SaveCount = Qlip.Properties.Settings.Default.SaveCount.ToString();
            double pto = Qlip.Properties.Settings.Default.PasteTimeout;
            PasteTimeout = pto > 0 ? pto.ToString() : DEFAULT_PASTE_TIMEOUT.ToString();
            AutoPaste = pto > 0;
            ResetOnPaste = Qlip.Properties.Settings.Default.ResetOnPaste;
            ResetOnCancel = Qlip.Properties.Settings.Default.ResetOnCancel;
            MovePastedToFront = Qlip.Properties.Settings.Default.MovePastedToFront;
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
            if (Qlip.Properties.Settings.Default.SaveCount != _saveCount)
                Qlip.Properties.Settings.Default.SaveCount = _saveCount;

            if (Qlip.Properties.Settings.Default.ResetOnPaste != _resetOnPaste)
                Qlip.Properties.Settings.Default.ResetOnPaste = _resetOnPaste;

            if (Qlip.Properties.Settings.Default.ResetOnCancel != _resetOnCancel)
                Qlip.Properties.Settings.Default.ResetOnCancel = _resetOnCancel;

            if (Qlip.Properties.Settings.Default.PasteTimeout != _pasteTimeout)
                Qlip.Properties.Settings.Default.PasteTimeout = _pasteTimeout;

            if (Qlip.Properties.Settings.Default.MovePastedToFront != _movePastedToFront)
                Qlip.Properties.Settings.Default.MovePastedToFront = _movePastedToFront;

            Qlip.Properties.Settings.Default.Save();
            this.Close();
        }

        private void RestoreDefaultsButton_Click(object sender, RoutedEventArgs e)
        {
            Qlip.Properties.Settings.Default.Reset();
            Qlip.Properties.Settings.Default.Save();
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
