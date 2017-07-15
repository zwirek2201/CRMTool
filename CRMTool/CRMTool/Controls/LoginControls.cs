using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Licencjat_new.CustomClasses;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Label = System.Windows.Controls.Label;
using TextBox = System.Windows.Controls.TextBox;

namespace Licencjat_new.Controls
{
    class RoundedTextBox:Border
    {
        private string _caption;
        private bool _selected;
        private bool _isPassword = false;

        private Label _leftLabel;
        private TextBox _textBox;
        private PasswordBox _passwordBox;
        private DockPanel _innerDock;

        public string Caption
        {
            get { return _caption; }
            set
            {
                _caption = value;
                _leftLabel.Content = Caption;
            }
        }

        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                if (Selected)
                {
                    BorderBrush = new SolidColorBrush(ColorScheme.GlobalBlue);
                    //_leftLabel.Visibility = Visibility.Collapsed;
                    //_textBox.Margin = new Thickness(15, 0, 15, 0);
                    //_passwordBox.Margin = new Thickness(15, 0, 15, 0);
                }
                else
                {
                    BorderBrush = new SolidColorBrush(ColorScheme.MenuDark);
                    //_leftLabel.Visibility = Visibility.Visible;
                    //_textBox.Margin = new Thickness(0, 0, 15, 0);
                    //_passwordBox.Margin = new Thickness(0, 0, 15, 0);
                }
            }
        }

        public bool IsPassword
        {
            get { return _isPassword; }
            set
            {
                _isPassword = value;
                if (IsPassword)
                {
                    _passwordBox.Visibility = Visibility.Visible;
                    _innerDock.Children.Remove(_textBox);
                }
                else
                {
                    _textBox.Visibility = Visibility.Visible;
                    _innerDock.Children.Remove(_passwordBox);
                }
            }
        }

        public bool Enabled
        {
            get { return IsPassword ? _passwordBox.IsEnabled : _textBox.IsEnabled; }
            set
            {
                if (IsPassword)
                    _passwordBox.IsEnabled = Enabled;
                else
                    _textBox.IsEnabled = Enabled;
            }
        }

        public RoundedTextBox()
        {
            Height = 40;
            CornerRadius = new CornerRadius(10);
            Background = new SolidColorBrush(ColorScheme.MenuLight);
            BorderThickness = new Thickness(1);
            BorderBrush = new SolidColorBrush(ColorScheme.MenuDark);
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Margin = new Thickness(7);

            _innerDock = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            _leftLabel = new Label()
            {
                Foreground = new SolidColorBrush(ColorScheme.GlobalBlue),
                FontSize = 16,
                Padding = new Thickness(15,3,5,3),
                HorizontalAlignment = HorizontalAlignment.Right,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            _textBox = new TextBox()
            {
                Foreground = new SolidColorBrush(Colors.Black),
                FontSize = 16,
                MinWidth = 20,
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Padding = new Thickness(0,3,0,3),
                Background = new SolidColorBrush(ColorScheme.MenuLight), 
                Margin = new Thickness(0,0,15,0),
                Visibility = Visibility.Visible
            };

            _passwordBox = new PasswordBox()
            {
                Foreground = new SolidColorBrush(Colors.Black),
                FontSize = 16,
                MinWidth = 20,
                BorderThickness = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                Padding = new Thickness(0, 3, 0, 3),
                Background = new SolidColorBrush(ColorScheme.MenuLight),
                Margin = new Thickness(0, 0, 15, 0),
                Visibility = Visibility.Collapsed,               
            };

            DockPanel.SetDock(_leftLabel, Dock.Left);
            _innerDock.Children.Add(_leftLabel);

            _innerDock.Children.Add(_passwordBox);
            _innerDock.Children.Add(_textBox);
            
            Child = _innerDock;

            _textBox.GotFocus += _textBox_GotFocus;
            _passwordBox.GotFocus += _textBox_GotFocus;;
            this.LostFocus += LoginTextBox_LostFocus;
        }

        public string Text
        {
            get { return IsPassword ? _passwordBox.Password : _textBox.Text; }
            set
            {
                if (IsPassword)
                    _passwordBox.Password = value;
                else
                    _textBox.Text = value;
            }
        }

        private void _textBox_GotFocus(object sender, RoutedEventArgs e)
        {
            Selected = true;
        }

        private void LoginTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Selected = false;
        }

        public void FocusOnMe()
        {
            if (IsPassword)
            {
                _passwordBox.Focusable = true;
                _passwordBox.Focus();
                Keyboard.Focus(_passwordBox);
            }
            else
            {
                _textBox.Focusable = true;
                _textBox.Focus();
                Keyboard.Focus(_textBox);
            }
        }
    }

    class RoundedButton : Border
    {
        public event EventHandler Clicked;
        private Label _innerLabel;

        private string _text;
        private Brush _backgroundColor = new SolidColorBrush(ColorScheme.GlobalBlue);
        private Brush _borderColor = new SolidColorBrush(ColorScheme.MenuLight);

        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                _innerLabel.Content = _text;
            }
        }

        public Brush BackgroundColor
        {
            get { return _backgroundColor; }
            set
            {
                _backgroundColor = value;
                Background = BackgroundColor;
            }
        }

        public Brush BorderColor
        {
            get { return _borderColor; }
            set
            {
                _borderColor = value;
                BorderBrush = BorderColor;
                _innerLabel.Foreground = BorderColor;
            }
        }

        public RoundedButton()
        {
            CornerRadius = new CornerRadius(10);
            Height = 40;
            Padding = new Thickness(15, 3, 15, 3);
            BorderThickness = new Thickness(1);
            Margin = new Thickness(10, 10, 10, 0);

            _innerLabel = new Label()
            {
                FontSize = 16,
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = BorderColor,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };

            Child = _innerLabel;

            MouseEnter += LoginButton_MouseEnter;
            MouseLeave += LoginButton_MouseLeave;
            PreviewMouseLeftButtonDown += LoginButton_PreviewMouseLeftButtonDown;
        }
        public RoundedButton(string text, Brush backgroundColor, Brush borderColor)
        {
            CornerRadius = new CornerRadius(10);
            Height = 40;
            Padding = new Thickness(15, 3, 15, 3);
            Background = backgroundColor ?? new SolidColorBrush(ColorScheme.GlobalBlue);
            BorderBrush = borderColor ?? new SolidColorBrush(ColorScheme.MenuLight);
            BorderThickness = new Thickness(1);
            Margin = new Thickness(10,10,10,0);

            _innerLabel = new Label()
            {
                FontSize = 16,
                FontWeight = FontWeights.Medium,
                Content = text,
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Foreground = BorderColor,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };

            Child = _innerLabel;

            MouseEnter += LoginButton_MouseEnter;
            MouseLeave += LoginButton_MouseLeave;
            MouseLeftButtonDown += LoginButton_PreviewMouseLeftButtonDown;
        }

        private void LoginButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MouseLeftButtonUp += LoginButton_PreviewMouseLeftButtonUp;
        }

        private void LoginButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Clicked?.Invoke(this, EventArgs.Empty);
            MouseLeftButtonUp -= LoginButton_PreviewMouseLeftButtonUp;
        }

        private void LoginButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Background = BackgroundColor;
            _innerLabel.Foreground = BorderColor;
            BorderBrush = BorderColor;
            Cursor = Cursors.Arrow;
        }

        private void LoginButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Background = BorderColor;
            _innerLabel.Foreground = BackgroundColor;
            BorderBrush = BackgroundColor;
            Cursor = Cursors.Hand;
        }

        public void PerformClick()
        {
            Clicked?.Invoke(this, EventArgs.Empty);
        }
    }
}
