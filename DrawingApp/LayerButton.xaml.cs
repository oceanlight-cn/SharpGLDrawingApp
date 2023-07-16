using System;
using System.Collections.Generic;
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

namespace DrawingApp
{
    /// <summary>
    /// LayerButton.xaml 的交互逻辑
    /// </summary>
    public partial class LayerButton : UserControl
    {
        public LayerButton()
        {
            InitializeComponent();
        }

        private void ToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (toggleButton.Content.ToString() == "显示")
            {
                LayerManager.Instance.ShowLayer(Id);
                toggleButton.Content = "隐藏";
            }
            else
            {
                LayerManager.Instance.HideLayer(Id);
                toggleButton.Content = "显示";
            }
        }

        public int Id { get; set; }

        public void HideRemoveButton()
        { 
            removeButton.Visibility = Visibility.Collapsed; 
        }

        public string LabelContent
        {
            get { return contentLabel.Content.ToString(); }
            set { contentLabel.Content = value; }
        }

        private void removeButton_Click(object sender, RoutedEventArgs e)
        {
            LayerManager.Instance.RemoveLayer(Id);
            if (sender is Button button)
            {
                if (Parent is Panel parent)
                {
                    parent.Children.Remove(this);
                }
            }
        }
    }
}
