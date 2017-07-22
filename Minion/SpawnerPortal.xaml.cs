using System;
using System.Threading.Tasks;
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
        double scaleStep = 0.025;
        double scaleStart = 0.1;
        double scaleEnd = 1;
        public bool Animating = true;
        Action currentAnimation;
        public static Random rnd = new Random();
        static public double ScreenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
        static public double ScreenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

        public SpawnerPortal()
        {
            InitializeComponent();
            Scale = new ScaleTransform();
            Scale.ScaleX = scaleStart;
            Scale.ScaleY = scaleStart;
            Skew = new SkewTransform();
            StartRoutine(ShowSpawner, () => TimeSpan.FromSeconds(rnd.Next(10, 30)));
            StartRoutine(SpawnerUpdate, () => TimeSpan.FromMilliseconds(16.666));
            currentAnimation = IncreaseScaleAnimation;

        }
        void ShowSpawner()
        {
            this.Left = rnd.Next((int)(ScreenWidth - this.Width));
            this.Top = rnd.Next((int)(ScreenHeight - ScreenHeight / 2));
            this.Show();
            Animating = true;
        }

        void SpawnerUpdate()
        {
            if (Animating)
            {
                currentAnimation();
            }
        }

        void IncreaseScaleAnimation()
        {
            Scale.ScaleX += scaleStep;
            Scale.ScaleY += scaleStep;
            PortalImage.RenderTransform = Scale;
            if (Scale.ScaleX >= scaleEnd)
                currentAnimation = SpawningMinionAnimation;
        }

        void SpawningMinionAnimation()
        {
            ShakeAnimation();
            SkewIter++;
            if (SkewIter == 30)
                SpawnMinion();

            else if (SkewIter > 60)
                currentAnimation = DecreaseScaleAnimation;
        }

        void DecreaseScaleAnimation()
        {
            Scale.ScaleX -= scaleStep;
            Scale.ScaleY -= scaleStep;
            PortalImage.RenderTransform = Scale;
            if (Scale.ScaleX <= scaleStart)
            {
                Animating = false;
                SkewIter = 0;
                currentAnimation = IncreaseScaleAnimation;
                this.Hide();
            }
        }

        void ShakeAnimation()
        {
            Skew.AngleX = Math.Sin(SkewIter);
            Skew.AngleY = Math.Cos(SkewIter);
            PortalImage.RenderTransform = Skew;
        }

        void SpawnMinion()
        {
            var minion = new MinionWindow();
            minion.posx = (this.Left + this.Width / 2) - minion.Width / 2;
            minion.Left = (this.Left + this.Width / 2) - minion.Width / 2;
            minion.posy = (this.Top + this.Height / 2);
            minion.Top = (this.Top + this.Height / 2);
            minion.Show();
        }

        async void StartRoutine(Action action, Func<TimeSpan> func)
        {
            await Task.Delay(func());
            action();
            StartRoutine(action, func);
        }
    }
}
