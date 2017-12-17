using System;
using System.Diagnostics;
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
            StartRoutine(ChangeDirection, () => TimeSpan.FromSeconds(rnd.Next(1, 6)));
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
        const float gravity = 1.0f;
        float accY = 0;
        float speed = 5f;
        int portalDist = 100;
        bool isStearing = false;
        bool isFalling = false;
        bool isDragged = false;
        bool isCloseToPortals = false;
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
        State currentState = State.HURT;
        AnimState currentAnimState = AnimState.HURT;
        int currentFrame = 0;


        void update()
        {
            this.lifetime -= 1;
            if (this.lifetime < 0)
            {
                this.Close();
                return;
            }
            if (isDragged) return;
            checkStearing();
            stearing();
            Move();
            WallCollision();
            //ProcessesHandler();
            GravityInfluence();
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
            if ((this.Left < portalDist && this.Top > ScreenHeight - 150) || (this.Left > ScreenWidth - (portalDist + this.Width) && this.Top > ScreenHeight - 150))
            {
                isCloseToPortals = true;
            }
            else
            {
                isCloseToPortals = false;
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
            if (currentState == State.HURT)
            {
                currentAnimState = AnimState.HURT;
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
            //for (int i = 0; i < procRect.Length; i++)
            //{
            //    if (posy > procRect[i].Top - this.Height + 20 && posy < procRect[i].Top + 10 && posx > procRect[i].Left && posx < procRect[i].Right && procRect[i].Top > 100)
            //    {
            //        posy = procRect[i].Top - this.Height + 20;
            //        accY = 0;
            //        isFalling = false;
            //        break;
            //    } 
            //}

            else if (posy > NotepadRect.Top - this.Height + 20 && posx > NotepadRect.Left && posx < NotepadRect.Right)
            {
                posy = NotepadRect.Top - this.Height + 20;
                accY = 0;
                isFalling = false;
            }

            currentState = State.WALKING;



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

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr FindWindow(string strClassName, string strWindowName);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }
        //Process[] processes;
        //Process proc;
        Rect NotepadRect;
        //Rect[] procRect;
        void ProcessesHandler()
        {
            //processes = Process.GetProcessesByName("notepad");
            //proc = processes[0];
            //IntPtr ptr = proc.MainWindowHandle;
            //NotepadRect = new Rect();
            //GetWindowRect(ptr, ref NotepadRect);

            //processes = Process.GetProcesses();
            //procRect = new Rect[processes.Length];
            //for (int i = 0; i < processes.Length; i++)
            //{
            //    var ptr = processes[i].MainWindowHandle;
            //    procRect[i] = new Rect();
            //    GetWindowRect(ptr, ref procRect[i]);
            //}

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            portalLeft.Close();
            portalRigth.Close();
            this.closed = true;
        }
    }
}
