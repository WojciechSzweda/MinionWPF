using System;
using System.Windows;
using System.Windows.Media;

namespace Minion
{
    /// <summary>
    /// Interaction logic for SpawnerPortal.xaml
    /// </summary>
    public partial class SpawnerPortal : Window
    {
        ScaleTransform Scale;
        SkewTransform Skew;
        int SkewIter = 0;
        public bool Animating = true;
        public SpawnerPortal()
        {
            InitializeComponent();
            Scale = new ScaleTransform();
            Scale.ScaleX = 0.1;
            Scale.ScaleY = 0.1;
            Skew = new SkewTransform();
        }

        public void SpawnAnimation()
        {
            Scale.ScaleX += 0.025;
            Scale.ScaleY += 0.025;
            PortalImage.RenderTransform = Scale;

            if (Scale.ScaleY >= 1)
            {
                Skew.AngleX = Math.Sin(SkewIter);
                Skew.AngleY = Math.Cos(SkewIter);
                SkewIter++;
                PortalImage.RenderTransform = Skew;
                if (SkewIter == 30)
                {
                    var minion = new MainWindow(false);
                    minion.posx = (this.Left + this.Width / 2) - minion.Width / 2;
                    minion.Left = (this.Left + this.Width / 2) - minion.Width / 2;
                    minion.posy = (this.Top + this.Height / 2);
                    minion.Top = (this.Top + this.Height / 2);
                    minion.Show();
                }
                if (SkewIter > 60)
                {
                    Animating = false;
                    Close();
                }
            }
        }


    }
}
