using System;
using System.Windows;
using System.Windows.Controls;

namespace InventarioILS.Model
{
    public class BottomBarManager
    {
        private BottomBarManager() { }

        RowDefinition _bottomBarContainer;
        ContentControl _bottomBar;

        public static GridLength MAX_HEIGHT { get; private set; }
        public static GridLength DEFAULT_HEIGHT { get; private set; }

        public object CurrentControlContent {
            get => _bottomBar.Content;
            set
            {
                CleanControlContent();
                _bottomBar.Content = value;
            }
        }

        public void CleanControlContent()
        {
            if (_bottomBar.Content is IDisposable oldControl)
            {
                oldControl.Dispose();
            }

            if (_bottomBar.Content is FrameworkElement fe)
            {
                fe.DataContext = null;
            }
        }

        public GridLength BottomBarHeight {
            get => _bottomBarContainer.Height;
            set => _bottomBarContainer.Height = value;
        }

        readonly static BottomBarManager _instance = new();

        public static BottomBarManager Instance => _instance;

        public void Initialize(ContentControl controlRef, RowDefinition containerRef, double bottomBarMaxHeight = 500, double bottomBarDefaultHeight = 400)
        {
            if (_bottomBar != null || _bottomBarContainer != null) return;

            _bottomBar = controlRef;
            _bottomBarContainer = containerRef;
            MAX_HEIGHT = new GridLength(bottomBarMaxHeight);
            DEFAULT_HEIGHT = new GridLength(bottomBarDefaultHeight);
        }
    }
}
