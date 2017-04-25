using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Minion
{
    /// <summary>
    /// Interaction logic for Portal.xaml
    /// </summary>
    public partial class Portal : Window
    {
        public Portal(string path)
        {

            InitializeComponent();
            PortalImage.Source = new BitmapImage(new Uri(path, UriKind.Relative));
        }
    }
}
