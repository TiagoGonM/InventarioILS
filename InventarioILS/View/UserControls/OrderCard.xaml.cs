using System;
using System.Windows;
using System.Windows.Controls;

namespace InventarioILS.View.UserControls
{
    public partial class OrderCard : UserControl
    {
        public OrderCard()
        {
            InitializeComponent();
        }

        public OrderCard(string title, DateTime creationDate, string description) : this()
        {
            Title = title;
            CreationDate = creationDate;
            Description = description;
            Done = false;
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(OrderCard),
                new PropertyMetadata(string.Empty, OnTitleChanged));

        public static readonly DependencyProperty CreationDateProperty =
            DependencyProperty.Register("CreationDate", typeof(DateTime), typeof(OrderCard),
                new PropertyMetadata(DateTime.Now, OnCreationDateChanged));

        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(OrderCard),
                new PropertyMetadata(string.Empty, OnDescriptionChanged));

        public static readonly DependencyProperty DoneProperty =
            DependencyProperty.Register("Done", typeof(bool), typeof(OrderCard),
                new PropertyMetadata(false, OnDoneChanged));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public DateTime CreationDate
        {
            get => (DateTime)GetValue(CreationDateProperty);
            set => SetValue(CreationDateProperty, value);
        }

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public bool Done
        {
            get => (bool)GetValue(DoneProperty);
            set => SetValue(DoneProperty, value);
        }

        private static void OnTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (OrderCard)d;
            control.TitleLabel.Text = e.NewValue.ToString();
        }

        private static void OnCreationDateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (OrderCard)d;
            control.CreationDateLabel.Text = ((DateTime)e.NewValue).ToString("dd/MM/yyyy");
        }

        private static void OnDescriptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (OrderCard)d;
            control.DescriptionLabel.Text = e.NewValue.ToString();
        }

        private static void OnDoneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (OrderCard)d;
            bool isDone = (bool)e.NewValue;
            control.isDoneLabel.Visibility = isDone ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
