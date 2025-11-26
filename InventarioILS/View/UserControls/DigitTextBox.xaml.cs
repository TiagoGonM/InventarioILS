using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace InventarioILS.View.UserControls
{
    /// <summary>
    /// Lógica de interacción para DigitTextBox.xaml
    /// </summary>
    public partial class DigitTextBox : UserControl
    {
        public DigitTextBox()
        {
            InitializeComponent();
        }

        private void DigitInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        public static readonly DependencyProperty TextProperty = 
            DependencyProperty.Register(
                nameof(Text),
                typeof(string),
                typeof(DigitTextBox),
                new PropertyMetadata(String.Empty, OnTextChanged)
            );

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (DigitTextBox)d;
            control.DigitInput.Text = e.NewValue.ToString();
        }
    }
}
