using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InventarioILS.Model
{
    public sealed class StatusManager
    {
        private StatusManager() { }

        private Label _statusMessageLabel;
        private static StatusManager _instance = new();

        public static StatusManager Instance => _instance;

        public void Initialize(Label labelReference)
        {
            if (_statusMessageLabel != null) return;
            
            _statusMessageLabel = labelReference;
        }

        public async void UpdateMessageStatus(string message, Brush color)
        {
            if (_statusMessageLabel == null)
                return;

            await _statusMessageLabel.Dispatcher.InvokeAsync(() =>
            {
                _statusMessageLabel.Foreground = color;
                _statusMessageLabel.Content = message;
                _statusMessageLabel.Visibility = Visibility.Visible;
            });

            await Task.Delay(5000);

            await _statusMessageLabel.Dispatcher.InvokeAsync(() =>
            {
                _statusMessageLabel.Visibility = Visibility.Hidden;
            });
        }
    }
}
