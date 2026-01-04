using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace WaterAssessment.Helpers.CustomControls
{
    public sealed partial class NumberBoxEx : NumberBox
    {
        public static readonly DependencyProperty DefaultValueProperty = DependencyProperty.Register(
            nameof(DefaultValue), typeof(double), typeof(NumberBoxEx), new PropertyMetadata(default));
        public NumberBoxEx() : base()
        {
            Loaded += NumberBoxEx_Loaded;
        }

        private void NumberBoxEx_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.FindDescendant<Button>(x => x.Name == nameof(DeleteButton)) is not Button deleteButton)
            {
                return;
            }
            DeleteButton = deleteButton;
            deleteButton.Click += OnDeleteButtonClick;
        }

        private void NumberBoxEx_Unloaded(object sender, RoutedEventArgs e)
        {
            if (DeleteButton is null)
            {
                return;
            }
            DeleteButton.Click -= OnDeleteButtonClick;
        }

        public Button? DeleteButton { get; private set; }

        public double DefaultValue
        {
            get => (double)GetValue(DefaultValueProperty);
            set => SetValue(DefaultValueProperty, value);
        }

        private void OnDeleteButtonClick(object sender, RoutedEventArgs e)
        {
            this.Value = DefaultValue;
        }
    }
}
