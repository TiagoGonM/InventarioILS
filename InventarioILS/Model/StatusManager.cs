using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InventarioILS.Model
{
    public sealed class StatusManager
    {
        public enum MessageType
        {
            DEFAULT,
            SUCCESS,
            ERROR
        }

        private StatusManager() { }

        private Label _statusMessageLabel;
        private static StatusManager _instance = new();

        public static StatusManager Instance => _instance;

        public void Initialize(Label labelReference)
        {
            if (_statusMessageLabel != null) return;
            
            _statusMessageLabel = labelReference;
        }

        public Brush GetColor(MessageType type)
        {
            switch (type)
            {
                case MessageType.DEFAULT:
                    return Brushes.White;
                case MessageType.SUCCESS:
                    return Brushes.Green;
                case MessageType.ERROR:
                    return Brushes.PaleVioletRed;
                default:
                    break;
            }

            return Brushes.White;
        }

        public async Task UpdateMessageStatusAsync(string message, MessageType colorType = MessageType.DEFAULT)
        {
            await UpdateMessageStatusAsync(message, GetColor(colorType)).ConfigureAwait(false);
        }

        public async Task UpdateMessageStatusAsync(string message, Brush color = null)
        {
            if (_statusMessageLabel == null)
                return;

            color ??= Brushes.White;

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
