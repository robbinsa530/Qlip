using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace WpfFormLibrary
{
    /// <summary>
    /// Interaction logic for WpfForm1.xaml
    /// </summary>
    public partial class IntroForm : Window
    {
        public IntroForm()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {

            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));

            e.Handled = true;

        }
    }
}
