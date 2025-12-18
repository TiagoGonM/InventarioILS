using System;
using System.Windows.Controls;

namespace InventarioILS.Model
{
    internal class BottomBarManager
    {
        private BottomBarManager() { }

        ContentControl _bottomBar;
        Action<bool> _toggleBottomBarCallback;
        

        public object ActiveControlContent 
        {
            get => _bottomBar.Content;
            set
            {
                if (value == null)
                {
                    _toggleBottomBarCallback.Invoke(false);
                    return;
                }

                _bottomBar.Content = value;
                _toggleBottomBarCallback?.Invoke(true);
            }
        }

        static BottomBarManager _instance = new();

        public static BottomBarManager Instance => _instance;

        public void Initialize(ContentControl controlRef, Action<bool> toggleBottomBar)
        {
            if (_bottomBar != null) return;

            _bottomBar = controlRef;
            _toggleBottomBarCallback = toggleBottomBar;
        }
    }
}
