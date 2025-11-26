using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public readonly DependencyProperty TextProperty = 
            DependencyProperty.Register(
                nameof(TextProperty),
                typeof(string),
                typeof(DigitTextBox),
                new PropertyMetadata(string.Empty, OnTextChanged)
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
