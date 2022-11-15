using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using WpfAppBar;


namespace AppBarExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Screen _screen;

        #region Win API

        /// Return Type: BOOL->int  
        ///X: int  
        ///Y: int  
        [DllImportAttribute("user32.dll", EntryPoint = "SetCursorPos")]
        [return: MarshalAsAttribute(UnmanagedType.Bool)]
        private static extern bool SetCursorPos(int X, int Y);

        #endregion Win API

        #region Delegates

        private delegate void NoArgDelegate();  // used to refresh the screen

        #endregion Delegates

        #region Fields

        private bool _isBeingDragged;
        private Rect _startPos;

        #endregion Fields

        #region Constructor

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion Constructor

        #region Form Events

        private void AppBar_OnClick(object sender, RoutedEventArgs e)
        {
            _startPos = new Rect(Left, Top, Width, Height);

            DockForm(ABEdge.Right);
        }

        private Screen CurrentScreen()
        {
            return Screen.FromPoint(new System.Drawing.Point((int)Left, (int)Top));
        }

        private void Normal_OnClick(object sender, RoutedEventArgs e)
        {
            UndockForm(_startPos);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _isBeingDragged = false;

            if (AppBar.IsEnabled)
            {
                UndockForm(_startPos);
            }            
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // check if docked
            if (AppBar.IsEnabled)
            {
                return;
            }

            var point = PointToScreen(e.GetPosition(this));

            if (point.Y < 100)
            {
                _isBeingDragged = true;

                try
                {
                    _screen = CurrentScreen();
                    DragMove();
                }
                catch
                {
                    _isBeingDragged = false;
                    DockForm(ABEdge.Right);
                }
                
            }
        }
                
        private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            var screen = CurrentScreen();

            if (Equals(screen, _screen))
            {
                _isBeingDragged = false;
                DockForm(ABEdge.Right);
            }
            else
            {
                MoveForm();
            }
        }

        #endregion Form Events

        #region Methods

        /// <summary>
        /// USed to refresh an UI element.
        /// </summary>
        /// <param name="obj">The UI element.</param>
        public static void Refresh(DependencyObject obj)
        {
            obj.Dispatcher.Invoke(DispatcherPriority.ApplicationIdle, (NoArgDelegate)delegate { });
        }

        /// <summary>
        /// Docks the form to the specified side.
        /// </summary>
        /// <param name="edge">The edge to dock to.</param>
        private void DockForm(ABEdge edge)
        {
            // set the width to 200 and update the buttons, lock screen to prevent jumping
            Dispatcher.Invoke(new Action(() =>
            {
                Width = 200;
                AppBar.IsEnabled = false;
                Normal.IsEnabled = true;
            }), DispatcherPriority.Render);

            // snap the window to the side like a task bar
            AppBarFunctions.SetAppBar(this, edge, grid);
        }

        /// <summary>
        /// Initiates moving the form after a drag drop.
        /// </summary>
        private void MoveForm()
        {
            if (!_isBeingDragged)
                return;

            _isBeingDragged = false;

            var info = AppBarFunctions.GetRegisterInfo(this);
            info.DockedSize = null;

            // snap the window to the side like a task bar
            DockForm(ABEdge.Right);
        }

        /// <summary>
        /// Undocks the form to the specified position.
        /// </summary>
        /// <param name="position">A structure that contains the location and size of the form.</param>
        private void UndockForm(Rect position, bool refresh = true)
        {
            // change the window back to normal window
            AppBarFunctions.SetAppBar(this, ABEdge.None, restorePosition: refresh);

            if (refresh)
            {
                Refresh(this);
            }

            // update the buttons and the width, lock screen to prevent jumping
            Dispatcher.Invoke(new Action(() =>
            {
                Normal.IsEnabled = false;
                AppBar.IsEnabled = true;
                Left = position.X;
                Top = position.Y;
                Width = position.Width;
                Height = position.Height;
            }), DispatcherPriority.Render);
        }


        #endregion Methods

    }
}
