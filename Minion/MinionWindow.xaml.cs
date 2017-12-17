using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Minion
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MinionWindow : Window
    {


        public MinionWindow(int _lifetime = 10)
        {
            InitializeComponent();
            this.lifetime = _lifetime * 30;
            StartRoutine(update, () => TimeSpan.FromMilliseconds(16.666));
            StartRoutine(ChangeDirection, () => TimeSpan.FromSeconds(rnd.Next(3, 6)));
            PortalSetup();

        }

        void PortalSetup()
        {
            portalLeft.Top = ScreenHeight - (portalLeft.Height + 20);
            portalLeft.Left = 0;
            portalRigth.Top = ScreenHeight - (portalRigth.Height + 20);
            portalRigth.Left = ScreenWidth - portalRigth.Width;
        }

        Portal portalLeft = new Portal("Portal2Blue.png");
        Portal portalRigth = new Portal("Portal2Red.png");
        static public double ScreenWidth = System.Windows.SystemParameters.PrimaryScreenWidth;
        static public double ScreenHeight = System.Windows.SystemParameters.PrimaryScreenHeight;

        static Random rnd = SpawnerPortal.rnd;
        public double posx = 20;
        public double posy = 20;
        public int lifetime;
        const float gravity = 0.6f;
        float accY = 0;
        float speed = 5f;
        int portalDist = 100;
        bool isStearing = false;
        bool isFalling = false;
        bool isDragged = false;
        bool isCloseToPortals = false;
        bool isDying = false;
        bool closed = false;
        int lastDirection = 1;
        int direction = 1;
        TranslateTransform translate = new TranslateTransform();

        enum AnimState
        {
            SPELLCATS_BACK = 0,
            SPELLCAST_LEFT = 1,
            SPELLCAST_FRONT = 2,
            SPELLCAST_RIGTH = 3,

            THRUST_BACK = 4,
            THRUST_LEFT = 5,
            THRUST_FRONT = 6,
            THRUST_RIGTH = 7,

            WALK_BACK = 8,
            WALK_LEFT = 9,
            WALK_FRONT = 10,
            WALK_RIGTH = 11,

            SLASH_BACK = 12,
            SLASH_LEFT = 13,
            SLASH_FRONT = 14,
            SLASH_RIGTH = 15,

            SHOOT_BACK = 16,
            SHOOT_LEFT = 17,
            SHOOT_FRONT = 18,
            SHOOT_RIGHT = 19,

            HURT = 20
        }

        enum State
        {
            SPELCASTING = 7,
            THRUSTING = 8,
            WALKING = 9,
            SLASHING = 6,
            SHOOTING = 13,
            HURT = 6
        }

        int frameSize = 64;
        State currentState = State.SPELCASTING;
        AnimState currentAnimState = AnimState.SPELLCAST_FRONT;
        int currentFrame = 0;


        void update()
        {
            if (isDying)
            {
                Dying();
                return;
            }
            this.lifetime--;

            CheckForDeath();

            if (isDragged) return;
            checkStearing();
            stearing();
            Move();
            WallCollision();
            GravityInfluence();
            WindowCollision();
            Dispatcher.Invoke(() =>
            {
                this.Left = posx;
                this.Top = posy;
            });
            SetAnimationState();
            Animation();
            CheckForPortals();
            SpawnPortals();
        }

        void CheckForPortals()
        {
            if ((this.Left < portalDist && this.Top > ScreenHeight - portalDist * 1.5)
                || (this.Left > ScreenWidth - (portalDist + this.Width)
                && this.Top > ScreenHeight - portalDist * 1.5))
            {
                isCloseToPortals = true;
            }
            else
            {
                isCloseToPortals = false;
            }
        }

        void CheckForDeath()
        {
            if (this.lifetime < 0)
            {
                currentFrame = 0;
                isDying = true;
                currentState = State.HURT;
                currentAnimState = AnimState.HURT;
            }
        }

        void SpawnPortals()
        {
            if (isCloseToPortals)
            {
                portalLeft.Visibility = Visibility.Visible;
                portalRigth.Visibility = Visibility.Visible;
            }
            else
            {
                portalRigth.Visibility = Visibility.Hidden;
                portalLeft.Visibility = Visibility.Hidden;
            }
        }

        void SetAnimationState()
        {
            if (isFalling)
            {
                currentState = State.SPELCASTING;
                currentAnimState = AnimState.SPELLCAST_FRONT;
                return;
            }
            if (!isFalling && currentState == State.SPELCASTING)
            {
                currentState = State.WALKING;
            }
            if (currentState == State.WALKING)
            {
                if (direction > 0)
                {
                    currentAnimState = AnimState.WALK_RIGTH;
                }
                else if (direction < 0)
                {
                    currentAnimState = AnimState.WALK_LEFT;
                }
                else if (direction == 0)
                {
                    currentAnimState = AnimState.WALK_FRONT;
                }
            }
        }


        void Animation()
        {
            translate.Y = -frameSize * (int)currentAnimState;
            translate.X = -frameSize * currentFrame;
            currentFrame++;
            currentFrame %= (int)currentState;

            charImage.RenderTransform = translate;

        }

        void Dying()
        {
            if (currentFrame == (int)currentState - 1)
            {
                this.Close();
                return;
            }
            Animation();
        }

        void checkStearing()
        {
            var mouse = GetMousePosition();
            var dis = Math.Sqrt(Math.Pow(mouse.X - this.Left, 2) + Math.Pow(mouse.Y - this.Top, 2));
            if (dis < ScreenHeight / 4)
            {
                isStearing = true;
            }
            else
            {
                isStearing = false;
            }

        }

        void stearing()
        {
            if (!isStearing)
                return;

            var mouse = GetMousePosition();


            var x = (posx + 0.5 * this.Width) - mouse.X;

            if (Math.Abs(x) < this.Width / 3) return;

            direction = Math.Sign(x);

        }

        void GravityInfluence()
        {
            accY += gravity;
            posy += accY;
            isFalling = true;
            if (posy > ScreenHeight - this.Height - 40)
            {
                posy = ScreenHeight - this.Height - 40;
                accY = 0;
                isFalling = false;
            }
        }



        void ChangeDirection()
        {
            if (!isStearing && currentState == State.WALKING)
            {

                var chance = rnd.NextDouble();

                if (chance < 0.6 || direction == 0)
                {
                    direction = lastDirection;
                    direction *= -1;
                    lastDirection = direction;
                }
                else
                {
                    direction = 0;
                }
            }
        }

        void WallCollision()
        {
            if (this.Left < 0)
            {
                posx = ScreenWidth - this.Width;
            }
            else if (this.Left > ScreenWidth - this.Width)
            {
                posx = 0;
            }
        }

        void WindowCollision()
        {
            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();

            foreach (SHDocVw.InternetExplorer ie in shellWindows)
            {
                var filename = Path.GetFileNameWithoutExtension(ie.FullName).ToLower();
                if (filename.Equals("explorer"))
                {
                    if (this.Top >= ie.Top - this.Height && this.Top <= ie.Top && this.posx > ie.Left - this.Width / 2 && this.posx < ie.Left + ie.Width - this.Width / 2)
                    {
                        posy = ie.Top - this.Height;
                        accY = 0;
                        isFalling = false;
                        return;
                    }
                }
            }
        }

        void Move()
        {
            if (!isFalling && currentState == State.WALKING)
                posx += speed * direction;
        }


        async void StartRoutine(Action action, Func<TimeSpan> func)
        {
            await Task.Delay(func());
            if (closed) return;
            action();
            StartRoutine(action, func);
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                isDragged = true;
                DragMove();
                posx = this.Left;
                posy = this.Top;

                isDragged = false;
            }
            if (e.ChangedButton == MouseButton.Middle)
                this.Close();
        }



        private void Window_Closed(object sender, EventArgs e)
        {
            portalLeft.Close();
            portalRigth.Close();
            this.closed = true;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }
    }
}
