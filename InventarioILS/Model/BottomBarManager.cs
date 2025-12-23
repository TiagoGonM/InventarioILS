using System;
using System.Windows.Controls;

namespace InventarioILS.Model
{
    internal class BottomBarManager
    {
        private BottomBarManager() { }

        ContentControl _bottomBar;

        public object CurrentControlContent 
        {
            get => _bottomBar.Content;
            set => _bottomBar.Content = value;
        }

        static BottomBarManager _instance = new();

        public static BottomBarManager Instance => _instance;

        public void Initialize(ContentControl controlRef)
        {
            if (_bottomBar != null) return;

            _bottomBar = controlRef;
        }
    }
}
