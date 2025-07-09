using System.Windows;
using System.Windows.Controls;

namespace Siemens.Simatic.S7.Webserver.API.WebApplicationManager.CustomControls
{
    /// <summary>
    /// Interaction logic for ProgressBarWithText.xaml
    /// </summary>
    public partial class ProgressBarWithTextControl : UserControl
    {
        public static readonly DependencyProperty ProgressBarValueProperty =
            DependencyProperty.Register("ProgressBarValue",
                typeof(int),
                typeof(ProgressBarWithTextControl));

        public int ProgressBarValue
        {
            get
            {
                return (int)pbStatus.Value;
            }
            set
            {
                pbStatus.Value = (double)value;
            }
        }
        public ProgressBarWithTextControl()
        {
            InitializeComponent();
        }
    }
}
