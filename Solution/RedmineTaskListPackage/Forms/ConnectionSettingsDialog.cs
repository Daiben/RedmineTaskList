using System.Windows.Forms;

namespace RedmineTaskListPackage.Forms
{
    public partial class ConnectionSettingsDialog : Form
    {
        public ConnectionSettingsDialog()
        {
            InitializeComponent();
        }
        
        public ConnectionSettings ConnectionSettings
        {
            get { return propertyGrid.SelectedObject as ConnectionSettings; }
            set { propertyGrid.SelectedObject = value; }
        }
    }
}
