using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Licencjat_new.CustomClasses;
using Licencjat_new.Windows;
using WpfAnimatedGif;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace Licencjat_new.Controls
{
    #region UpperMenuStrip
    class UpperMenuStrip : Grid
    {
        #region Variables
        private MainWindow  _parent;
        private StackPanel _userPanel;
        private Popup _userPopup;
        private UpperMenuMode _menuMode;
        private List<UpperMenuModeButton> _menuButtons = new List<UpperMenuModeButton>();
        private Label _userLabel;
        private Label _hiddenLabel;
        private LoginStatusChangedEventArgs _loginStatus;
        public event EventHandler UpperMenuModeChanged;
        #endregion

        #region Properties
        public NotificationButton NotificationButton { get; private set; }

        public UpperMenuMode MenuMode
        {
            get { return _menuMode; }
            set
            {
                _menuMode = value;

                foreach (UpperMenuModeButton button in _menuButtons)
                {
                    if (button.Active)
                        button.Active = false;
                }

                _menuButtons.Find(obj => obj.Mode == MenuMode).Active = true;

                UpperMenuModeChanged(this, EventArgs.Empty);
            }
        }

        public new MainWindow Parent
        {
            get { return _parent; }
            set
            {
                if (_parent == null && value != null)
                    value.LoginStatusChanged += _parent_LoginStatusChanged;

                _parent = value;               
            }
        }
        #endregion

        #region Constructors
        public UpperMenuStrip()
        {
            Width = double.NaN;
            Height = 40;
            Margin = new Thickness(0);
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            Background = new SolidColorBrush(ColorScheme.GlobalBlue);
            DockPanel.SetDock(this, Dock.Top);
            _loginStatus = null;

            Redraw();
        }
        #endregion

        #region Events
        private void _parent_LoginStatusChanged(object sender, LoginStatusChangedEventArgs e)
        {
            _loginStatus = e;
            Redraw();
        }

        private void _userPanel_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_userPopup.IsOpen)
            {
                _userLabel.FontWeight = FontWeights.Normal;
                _userPopup.IsOpen = false;
            }
        }

        private void UpperMenuButtonClick(object sender, EventArgs e)
        {
            UpperMenuModeButton sendingButton = (UpperMenuModeButton)sender;
            MenuMode = sendingButton.Mode;
        }

        private void _userPanel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _userPopup.IsOpen = true;
            _userLabel.FontWeight = FontWeights.Medium;
        }

        private void _userPanel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_userPopup.IsMouseOver)
            {
                _userPopup.IsOpen = false;
                _userLabel.FontWeight = FontWeights.Normal;
            }
        }
        #endregion

        #region Methods
        private void Redraw()
        {
            //Left panel
            StackPanel leftPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Height = 40,
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            StackPanel logoPanel = new StackPanel()
            {
                Width = 40,
                Height = 40,
                Margin = new Thickness(0),
                Background = new SolidColorBrush(ColorScheme.GlobalWhite),
                HorizontalAlignment = HorizontalAlignment.Left
            };

            Image logo = new Image()
            {
                Width = 24,
                Height = 24,
                Margin = new Thickness(8),
                Source = ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/logo.png")),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            RenderOptions.SetBitmapScalingMode(logo, BitmapScalingMode.HighQuality);

            logoPanel.Children.Add(logo);
            leftPanel.Children.Add(logoPanel);

            Label versionLabel = new Label()
            {
                Height = 40,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Foreground = new SolidColorBrush(ColorScheme.GlobalWhite),
                Margin = new Thickness(2, 0, 0, 0),
                FontSize = 13,
                FontWeight = FontWeights.Medium,
                Content = "CRMTool ("+ Assembly.GetExecutingAssembly().GetName().Version +")"
            };

            leftPanel.Children.Add(versionLabel);
            this.Children.Add(leftPanel);

            //Right panel
            StackPanel rightPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Height = 40,
                Margin = new Thickness(0),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            this.Children.Add(rightPanel);

            _userPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                Height = 40,
                Margin = new Thickness(0, 0, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Left,
                Background = new SolidColorBrush(ColorScheme.GlobalWhite)
            };

            Image userImage = new Image()
            {
                Source = ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/user.png")),
                Width = 20,
                Height = 20,
                Margin = new Thickness(8, 5, 0, 5),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            RenderOptions.SetBitmapScalingMode(userImage, BitmapScalingMode.HighQuality);
            _userPanel.Children.Add(userImage);

            _userLabel = new Label()
            {
                Height = 40,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(2, 0, 5, 0),
                Foreground = new SolidColorBrush(ColorScheme.GlobalBlue),
                FontSize = 13,
            };

            Label _hiddenLabel = new Label()
            {
                Height = 0,
                FontSize = 13,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Margin = new Thickness(2, 0, 5, 0),
                FontWeight = FontWeights.Bold,
                Visibility = Visibility.Hidden,
            };

            _userPanel.Children.Add(new StackPanel() { Orientation = Orientation.Vertical, Children = { _userLabel, _hiddenLabel } });

            //User logged in
            if (_loginStatus != null && _loginStatus.LoggedIn)
            {
                UpperMenuModeButton mainButton = new UpperMenuModeButton("Strona główna", UpperMenuMode.HomePage);
                mainButton.Click += UpperMenuButtonClick;
                rightPanel.Children.Add(mainButton);
                _menuButtons.Add(mainButton);

                UpperMenuModeButton conversationButton = new UpperMenuModeButton("Konwersacje",
                    UpperMenuMode.Conversation);
                conversationButton.Click += UpperMenuButtonClick;
                rightPanel.Children.Add(conversationButton);
                _menuButtons.Add(conversationButton);

                UpperMenuModeButton documentButton = new UpperMenuModeButton("Dokumenty", UpperMenuMode.Document);
                documentButton.Click += UpperMenuButtonClick;
                rightPanel.Children.Add(documentButton);
                _menuButtons.Add(documentButton);

                UpperMenuModeButton emailButton = new UpperMenuModeButton("E-mail", UpperMenuMode.Email);
                emailButton.Click += UpperMenuButtonClick;
                rightPanel.Children.Add(emailButton);
                _menuButtons.Add(emailButton);

                UpperMenuModeButton contactButton = new UpperMenuModeButton("Kontakty", UpperMenuMode.Contact);
                contactButton.Click += UpperMenuButtonClick;
                rightPanel.Children.Add(contactButton);
                _menuButtons.Add(contactButton);

                _userLabel.Content = _loginStatus.FirstName + " " + _loginStatus.LastName;
                _hiddenLabel.Content = _loginStatus.FirstName + " " + _loginStatus.LastName;

                _userPanel.PreviewMouseLeftButtonUp += _userPanel_PreviewMouseLeftButtonUp;

                _userPopup = new Popup()
                {
                    Width = 200,
                    Height = 150,
                    PlacementTarget = _userPanel,
                    Placement = PlacementMode.Bottom,
                    AllowsTransparency = true,
                    VerticalOffset = 0,
                    PopupAnimation = PopupAnimation.Slide
                };

                Canvas popupStack = new Canvas()
                {
                    Background = new SolidColorBrush(Colors.White),
                    Margin = new Thickness(5, 0, 5, 5),
                    Effect = new DropShadowEffect
                    {
                        Color = Colors.Black,
                        Direction = 320,
                        ShadowDepth = 0,
                        Opacity = 0.4,
                        BlurRadius = 8
                    }
                };

                _userPopup.Child = popupStack;

                _userPanel.MouseEnter += _userPanel_MouseEnter;
                _userPanel.MouseLeave += _userPanel_MouseLeave;
                _userPopup.MouseLeave += _userPanel_MouseLeave;
            }
            else
            {
                _userLabel.Content = "Zaloguj";
                _hiddenLabel.Content = "Zaloguj";
            }

            rightPanel.Children.Add(_userPanel);

            if (_loginStatus != null && _loginStatus.LoggedIn)
            {
                NotificationButton = new NotificationButton();
                rightPanel.Children.Add(NotificationButton);
            }
        }
        #endregion
    }

    internal class UpperMenuModeButton : DockPanel
    {
        #region Variables

        private readonly Label _buttonLabel;
        private bool _active;

        public event EventHandler Click;
        #endregion

        #region Properties
        public bool Active
        {
            get { return _active; }
            set
            {
                _active = value;
                if (value)
                {
                    this.Background = new SolidColorBrush(ColorScheme.MenuDarker);
                    _buttonLabel.FontWeight = FontWeights.Medium;
                    _buttonLabel.Foreground = new SolidColorBrush(ColorScheme.GlobalBlue);
                }
                else
                {
                    this.Background = new SolidColorBrush(ColorScheme.GlobalBlue);
                    _buttonLabel.FontWeight = FontWeights.Normal;
                    _buttonLabel.Foreground = new SolidColorBrush(Colors.White);
                }
            }
        }

        public string Name { get; }

        public UpperMenuMode Mode { get; }
        #endregion

        #region Constructors
        public UpperMenuModeButton(String name, UpperMenuMode mode)
        {
            Name = name;
            Mode = mode;
            Height = 40;
            Background = new SolidColorBrush(ColorScheme.GlobalBlue);
            HorizontalAlignment = HorizontalAlignment.Left;
            Tag = mode.ToString();

            _buttonLabel = new Label()
            {
                FontSize = 13,
                Margin = new Thickness(5, 0, 5, 0),
                Content = name,
                VerticalContentAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(ColorScheme.GlobalWhite),
                Height = 40
            };

            Label hiddenLabel = new Label()
            {
                Height = 0,
                FontSize = 13,
                Margin = new Thickness(5, 0, 5, 0),
                FontWeight = FontWeights.Medium,
                VerticalContentAlignment = VerticalAlignment.Center,
                Content = name,
                Visibility = Visibility.Hidden,
        };

            this.Children.Add(new StackPanel { Orientation = Orientation.Vertical, Children = {_buttonLabel, hiddenLabel}});
            MouseEnter += UpperMenuModeButton_MouseEnter; 
            MouseLeave += UpperMenuModeButton_MouseLeave;
            PreviewMouseLeftButtonDown += UpperMenuModeButton_PreviewMouseLeftButtonDown;
        }

        private void UpperMenuModeButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PreviewMouseLeftButtonUp += UpperMenuModeButton_PreviewMouseLeftButtonUp;
        }

        private void UpperMenuModeButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Click?.Invoke(this, new EventArgs());
            PreviewMouseLeftButtonUp -= UpperMenuModeButton_PreviewMouseLeftButtonUp;
        }
        #endregion

        #region Events
        private void UpperMenuModeButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!Active)
            {
                this.Background = new SolidColorBrush(ColorScheme.MenuDarker);
                _buttonLabel.FontWeight = FontWeights.Medium;
                _buttonLabel.Foreground = new SolidColorBrush(ColorScheme.GlobalBlue);
            }
        }

        private void UpperMenuModeButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!Active)
            {
                this.Background = new SolidColorBrush(ColorScheme.GlobalBlue);
                _buttonLabel.FontWeight = FontWeights.Normal;
                _buttonLabel.Foreground = new SolidColorBrush(Colors.White);
            }
        }
        #endregion
    }
    #endregion

    #region StatusBar
    class StatusBar : DockPanel
    {
        private Label _leftLabel;
        private string _statusText;

        public string StatusText
        {
            get
            {
                return _statusText;
            }

            set
            {
                _statusText = value;

                _leftLabel.Content = StatusText;
            }
        }

        #region Constructors
        public StatusBar()
        {
            Width = double.NaN;
            Height = 20;
            VerticalAlignment = VerticalAlignment.Bottom;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Background = new SolidColorBrush(ColorScheme.GlobalBlue);

            StackPanel leftStack = new StackPanel()
            {
                Width = double.NaN,
                Height = 20,
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0,0,0,0),
                //Background = new SolidColorBrush(Colors.Red)
            };
            DockPanel.SetDock(leftStack, Dock.Left);

            Image leftImage = new Image()
            {
                Name = "leftImage",
                Width = 20,
                Height = 14,
                Margin = new Thickness(5,0,5,0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                Stretch = Stretch.Uniform,
        };

            ImageBehavior.SetRepeatBehavior(leftImage, RepeatBehavior.Forever);

            RenderOptions.SetBitmapScalingMode(leftImage, BitmapScalingMode.HighQuality);

            leftStack.Children.Add(leftImage);

            _leftLabel = new Label()
            {
                Margin = new Thickness(0,0,0,0),
                Name = "leftLabel",
                Width = double.NaN,
                Padding = new Thickness(0,0,0,0),
                Height = 20,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                FontSize = 11,
                Foreground = new SolidColorBrush(ColorScheme.GlobalWhite),
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };

            leftStack.Children.Add(_leftLabel);
            Children.Add(leftStack);

            StackPanel rightStack = new StackPanel()
            {
                Width = double.NaN,
                Height = 20,
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0, 0, 0, 0),
            };
            DockPanel.SetDock(rightStack, Dock.Right);
        }
        #endregion
    }
    #endregion

    #region ToolBarMenuStrip

    class ToolBarMainMenuStrip : DockPanel
    {
        #region Variables
        private StackPanel _leftPanel;
        private StackPanel _rightPanel;
        #endregion

        #region Properties
        public List<UIElement> Buttons { get; private set; } = new List<UIElement>();
        #endregion

        #region Constructors
        public ToolBarMainMenuStrip()
        {
            Width = double.NaN;
            Height = 40;
            Background = new SolidColorBrush(ColorScheme.MenuDarker);
            VerticalAlignment = VerticalAlignment.Top;
            LastChildFill = false;

            _leftPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            DockPanel.SetDock(_leftPanel, Dock.Left);
            Children.Add(_leftPanel);

            _rightPanel = new StackPanel()
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0,0,15,0)
            };   

            DockPanel.SetDock(_rightPanel, Dock.Right);
            Children.Add(_rightPanel);

            DockPanel.SetDock(this, Dock.Top);
            Panel.SetZIndex(this,200);

            Effect = new DropShadowEffect
            {
                Direction = 315,
                ShadowDepth = 1.5,
                Opacity = 0.3,
                BlurRadius = 5
            };
        }
        #endregion

        #region Methods
        public void AddButton(UIElement button, Dock dock)
        {
            if (dock == Dock.Left)
                _leftPanel.Children.Add(button);
            else if (dock == Dock.Right)
                _rightPanel.Children.Add(button);

            Buttons.Add(button);
        }
        #endregion
    }

    class ToolBarSmallMenuStrip : DockPanel
    {
        #region Constructors
        public ToolBarSmallMenuStrip()
        {
            Width = double.NaN;
            Height = 30;
            Background = new SolidColorBrush(ColorScheme.GlobalBlue);
            VerticalAlignment = VerticalAlignment.Top;
            DockPanel.SetDock(this, Dock.Top);
            LastChildFill = false;
        }
        #endregion
    }

    class ToolBarButton : Grid
    {
        #region Variables
        private string _name;
        private Uri _icon;
        private Popup _popup;

        public event EventHandler Click;
        #endregion

        #region Constructors
        public ToolBarButton(string name, Uri icon)
        {
            _name = name;
            _icon = icon;
            Height = 40;
            Background = new SolidColorBrush(Colors.Transparent);

            StackPanel innerStack = new StackPanel()
            {
                Height = 40,
            };

            Image img = new Image()
            {
                Height = 25,
                Margin = new Thickness(7.5),
                Source = ImageHelper.UriToImageSource(_icon)
            };

            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
            innerStack.Children.Add(img);
            Children.Add(innerStack);

            MouseEnter += ToolBarButton_MouseEnter;
            MouseLeave += ToolBarButton_MouseLeave;

            PreviewMouseLeftButtonDown += ToolBarButton_PreviewMouseLeftButtonDown;

            _popup = new Popup()
            {
                MinWidth = 40,
                Placement = PlacementMode.RelativePoint,
                AllowsTransparency = true,
                VerticalOffset = 40,
                HorizontalOffset = 0,
                PopupAnimation = PopupAnimation.Fade
            };

            _popup.MouseLeave += ToolBarButton_MouseLeave;

            Border border = new Border()
            {
                BorderBrush = new SolidColorBrush(ColorScheme.MenuDark),
                BorderThickness = new Thickness(0,0,0,1)
            };

            StackPanel popupStack = new StackPanel()
            {
                Background = new SolidColorBrush(Colors.White),
                Margin = new Thickness(0),
                Opacity = 0.8,
                MinHeight = 30
            };

            border.Child = popupStack;

            TextBlock buttonName = new TextBlock()
            {
                Margin = new Thickness(10, 5, 10, 5),
                FontSize = 13,
                MaxWidth = 150,
                FontWeight = FontWeights.Medium,
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Colors.Black),
                TextAlignment = TextAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                Text = _name
            };

            popupStack.Children.Add(buttonName);
            _popup.Child = border;
        }

        #endregion

        #region Events
        private void ToolBarButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_popup.IsOpen)
            {
                _popup.IsOpen = false;
            }
        }

        private void ToolBarButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (Parent != null)
            {
                FrameworkElement parent = (FrameworkElement) this.Parent;
                _popup.PlacementTarget = parent;
                _popup.Width = parent.ActualWidth;
            }
            _popup.IsOpen = true;
        }

        private void ToolBarButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PreviewMouseLeftButtonUp += ToolBarButton_PreviewMouseLeftButtonUp;
        }

        private void ToolBarButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Click?.Invoke(this, EventArgs.Empty);
            PreviewMouseLeftButtonUp -= ToolBarButton_PreviewMouseLeftButtonUp;
        }
        #endregion
    }

    class ToolBarWideButton : Grid
    {
        #region Variables
        private string _name;
        private Uri _icon;
        private Popup _popup;
        private bool _toggled;
        private Label _nameLabel;

        public event EventHandler Clicked;
        public event EventHandler ToggledChanged;
        #endregion

        #region Properties

        public bool Toggled
        {
            get { return _toggled; }
            set
            {
                _toggled = value;
                if(Toggleable)
                    _nameLabel.Foreground = new SolidColorBrush(Toggled ? ColorScheme.GlobalBlue : Colors.Black);

                if(Toggled)
                    ToggledChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool Toggleable { get; private set; }

        public object HelperChild { get; set; }
        #endregion

        #region Constructors
        public ToolBarWideButton(string name, Uri icon = null, bool toggleable = false)
        {
            _name = name;
            _icon = icon;
            Width = double.NaN;
            Height = double.NaN;
            VerticalAlignment = VerticalAlignment.Stretch;
            Background = new SolidColorBrush(Colors.Transparent);
            Toggleable = toggleable;

            PreviewMouseLeftButtonDown += ToolBarWideButton_PreviewMouseLeftButtonDown;
            MouseEnter += ToolBarWideButton_MouseEnter;
            MouseLeave += ToolBarWideButton_MouseLeave;

            StackPanel innerStack = new StackPanel()
            {
                Width = double.NaN,
                VerticalAlignment = VerticalAlignment.Stretch,
                Orientation = Orientation.Horizontal
            };

            if (icon != null)
            {
                Image img = new Image()
                {
                    Width = 15,
                    Height = 15,
                    Margin = new Thickness(5, 10, 5, 10),
                    VerticalAlignment = VerticalAlignment.Center,
                    Source = ImageHelper.UriToImageSource(_icon)
                };
                RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
                innerStack.Children.Add(img);
            }

            _nameLabel = new Label()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                HorizontalAlignment = HorizontalAlignment.Left,
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.Black),
                Background = new SolidColorBrush(Colors.Transparent),
                Content = _name
            };
            innerStack.Children.Add(_nameLabel);

            Children.Add(innerStack);
        }

        private void ToolBarWideButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if(!Toggled)
            _nameLabel.Foreground = new SolidColorBrush(Colors.Black);
        }

        private void ToolBarWideButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if(!Toggled)
            _nameLabel.Foreground = new SolidColorBrush(ColorScheme.GlobalBlue);
        }

        private void ToolBarWideButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PreviewMouseLeftButtonUp += ToolBarWideButton_PreviewMouseLeftButtonUp;
        }

        private void ToolBarWideButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (Toggleable)
            {
                if (!Toggled)
                {
                    Toggled = true;
                }

            }
            else
            {
                Clicked?.Invoke(this, EventArgs.Empty);
            }
            PreviewMouseLeftButtonUp -= ToolBarWideButton_PreviewMouseLeftButtonUp;
        }
        #endregion
    }

    class ToolBarSpacer : Border
    {
        public ToolBarSpacer(double width, Thickness margin)
        {
            Width = width;
            Margin = margin;
            Background = new SolidColorBrush(ColorScheme.GlobalBlue);
            CornerRadius = new CornerRadius(width/2);
        }
    }

    class SmallToolBarWideButton : DockPanel
    {
        #region Variables
        private string _name;
        private Uri _icon;
        private Popup _popup;

        public event EventHandler Click;
        #endregion

        #region Constructors
        public SmallToolBarWideButton(string name, Uri icon = null)
        {
            _name = name;
            _icon = icon;
            Width = double.NaN;
            Height = double.NaN;
            VerticalAlignment = VerticalAlignment.Stretch;
            Background = new SolidColorBrush(Colors.Transparent);

            StackPanel innerStack = new StackPanel()
            {
                Width = double.NaN,
                VerticalAlignment = VerticalAlignment.Stretch,
                Orientation = Orientation.Horizontal
            };

            if (icon != null)
            {
                Image img = new Image()
                {
                    Width = 15,
                    Height = 15,
                    Margin = new Thickness(5, 10, 5, 10),
                    VerticalAlignment = VerticalAlignment.Center,
                    Source = ImageHelper.UriToImageSource(_icon)
                };
                RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);
                innerStack.Children.Add(img);
            }

            Label label = new Label()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                HorizontalAlignment = HorizontalAlignment.Left,
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.White),
                Background = new SolidColorBrush(Colors.Transparent),
                Content = _name,
                Padding = new Thickness(15,0,15,0)
            };
            innerStack.Children.Add(label);

            Children.Add(innerStack);

            PreviewMouseLeftButtonDown += SmallToolBarWideButton_PreviewMouseLeftButtonDown;
            MouseEnter += (s, ea) =>
            {
                Background = new SolidColorBrush(ColorScheme.MenuLight);
                label.Foreground = new SolidColorBrush(ColorScheme.GlobalBlue);
            };

            MouseLeave += (s, ea) =>
            {
                Background = new SolidColorBrush(ColorScheme.GlobalBlue);
                label.Foreground = new SolidColorBrush(ColorScheme.GlobalWhite);
            };
        }

        private void SmallToolBarWideButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PreviewMouseLeftButtonUp += SmallToolBarWideButton_PreviewMouseLeftButtonUp;
        }

        private void SmallToolBarWideButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Click?.Invoke(this, EventArgs.Empty);
            PreviewMouseLeftButtonUp -= SmallToolBarWideButton_PreviewMouseLeftButtonUp;
        }
        #endregion
    }

    public class ToolBarToggleButton : DockPanel
    {
        #region Variables

        private string _imageSource;
        private Image _image;

        private Canvas _mainCanvas;
        private Border _togglePanel;
        private bool _toggled = false;

        public string ImageSource
        {
            get { return _imageSource; }
            set
            {
                _imageSource = value;
                _image.Source = ImageHelper.UriToImageSource(new Uri(_imageSource));
            }
        }

        public event EventHandler ToggleChanged;
        #endregion

        #region Properties

        public bool Toggled
        {
            get { return _toggled; }
            set
            {
                _toggled = value;

                TranslateTransform transform = new TranslateTransform();
                _togglePanel.RenderTransform = transform;
                DoubleAnimation anim1 = new DoubleAnimation();
                anim1.From = _toggled ? 0 : 20;
                anim1.To = _toggled ? 20 : 0;
                anim1.Duration = new Duration(TimeSpan.FromMilliseconds(100));

                var animation = new ColorAnimation();
                animation.From = _toggled ? ColorScheme.MenuDark : ColorScheme.GlobalBlue;
                animation.To = _toggled ? ColorScheme.GlobalBlue : ColorScheme.MenuDark;
                animation.Duration = new Duration(TimeSpan.FromMilliseconds(200));

                transform.BeginAnimation(TranslateTransform.XProperty, anim1);
                _togglePanel.Background.BeginAnimation(SolidColorBrush.ColorProperty, animation);

                ToggleChanged?.Invoke(this, null);
            }
        }
        #endregion

        #region Constructors

        public ToolBarToggleButton()
        {
            VerticalAlignment = VerticalAlignment.Stretch;
            LastChildFill = false;

            _image = new Image()
            {
                Margin = new Thickness(5, 10, 0, 10),
                VerticalAlignment = VerticalAlignment.Center
            };
            RenderOptions.SetBitmapScalingMode(_image, BitmapScalingMode.HighQuality);

            DockPanel.SetDock(_image, Dock.Left);
            Children.Add(_image);

            _mainCanvas = new Canvas()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(ColorScheme.GlobalWhite)
            };

            Border lowerPanel = new Border()
            {
                Width = 35,
                Height = 15,
                Margin = new Thickness(10, 12.5, 10, 12.5),
                Background = new SolidColorBrush(ColorScheme.GlobalWhite),
                CornerRadius = new CornerRadius(7.5)
            };

            _togglePanel = new Border()
            {
                Height = 20,
                Width = 20,
                Margin = new Thickness(7.5, 10, 7.5, 10),
                Background = new SolidColorBrush(ColorScheme.MenuDark),
                CornerRadius = new CornerRadius(10)
            };

            Canvas.SetTop(lowerPanel, 0);
            Canvas.SetTop(_togglePanel, 0);
            Canvas.SetLeft(lowerPanel, 0);
            Canvas.SetLeft(_togglePanel, 0);

            Width = 80;

            _mainCanvas.Children.Add(lowerPanel);
            _mainCanvas.Children.Add(_togglePanel);

            Children.Add(_mainCanvas);

            _mainCanvas.MouseLeftButtonDown += ToolBarToggleButton_MouseLeftButtonDown;
        }
        public ToolBarToggleButton(Uri image = null)
        {
            VerticalAlignment = VerticalAlignment.Stretch;
            LastChildFill = false;

            if (image != null)
            {
                Image img = new Image()
                {
                    Margin = new Thickness(5,10,0,10),
                    VerticalAlignment = VerticalAlignment.Center,
                    Source = ImageHelper.UriToImageSource(image)
                };
                RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.HighQuality);

                DockPanel.SetDock(img, Dock.Left);
                Children.Add(img);
            }

            _mainCanvas = new Canvas()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(ColorScheme.MenuLight)
            };

            Border lowerPanel = new Border()
            {
                Width = 35,
                Height = 15,
                Margin = new Thickness(10, 12.5, 10, 12.5),
                Background = new SolidColorBrush(ColorScheme.MenuLight),
                CornerRadius = new CornerRadius(7.5)
            };

            _togglePanel = new Border()
            {
                Height = 20,
                Width = 20,
                Margin = new Thickness(7.5, 10, 7.5, 10),
                Background = new SolidColorBrush(ColorScheme.MenuDark),
                CornerRadius = new CornerRadius(10)
            };

            Canvas.SetTop(lowerPanel, 0);
            Canvas.SetTop(_togglePanel, 0);
            Canvas.SetLeft(lowerPanel, 0);
            Canvas.SetLeft(_togglePanel, 0);

            Width = 80;

            _mainCanvas.Children.Add(lowerPanel);
            _mainCanvas.Children.Add(_togglePanel);

            Children.Add(_mainCanvas);

            _mainCanvas.MouseLeftButtonDown += ToolBarToggleButton_MouseLeftButtonDown;
        }
        #endregion

        #region Events
        private void ToolBarToggleButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _mainCanvas.MouseLeftButtonUp += ToolBarToggleButton_MouseLeftButtonUp;
        }

        private void ToolBarToggleButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            MouseLeftButtonUp -= ToolBarToggleButton_MouseLeftButtonUp;
            Toggled = !Toggled;
            e.Handled = true;
        }
        #endregion
    }
    #endregion

    #region TitleBar

    public class TitleBarButton : DockPanel
    {
        private Image _mainImage;
        private ImageSource _image;
        private ImageSource _hoverImage;

        public event EventHandler Clicked;

        public ImageSource Image
        {
            get { return _image; }
            set
            {
                _image = value;
                if (IsMouseOver)
                    _mainImage.Source = _hoverImage;
                else
                    _mainImage.Source = _image;
            }
        }

        public ImageSource HoverImage
        {
            get { return _hoverImage; }
            set
            {
                _hoverImage = value;
            }
        }

        public TitleBarButton(ImageSource image, ImageSource hoverImage)
        {
            _image = image;
            _hoverImage = hoverImage;

            Height = 25;
            Width = 30;
            Background = new SolidColorBrush(ColorScheme.GlobalBlue);

            MouseEnter += TitleBarButton_MouseEnter;
            MouseLeave += TitleBarButton_MouseLeave;
            PreviewMouseLeftButtonDown += TitleBarButton_PreviewMouseLeftButtonDown;

            _mainImage = new Image()
            {
                Height = 10,
                Margin = new Thickness(7.5),
                Source = image
            };

            RenderOptions.SetBitmapScalingMode(_mainImage, BitmapScalingMode.LowQuality);
            RenderOptions.SetEdgeMode(_mainImage, EdgeMode.Aliased);
            _mainImage.UseLayoutRounding = true;
            _mainImage.SnapsToDevicePixels = true;

            Children.Add(_mainImage);
        }

        private void TitleBarButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PreviewMouseLeftButtonUp += TitleBarButton_PreviewMouseLeftButtonUp;
        }

        private void TitleBarButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Clicked?.Invoke(this, EventArgs.Empty);
            PreviewMouseLeftButtonUp -= TitleBarButton_PreviewMouseLeftButtonUp;
        }

        private void TitleBarButton_MouseLeave(object sender, MouseEventArgs e)
        {
            _mainImage.Source = _image;
            Background = new SolidColorBrush(ColorScheme.GlobalBlue);
        }

        private void TitleBarButton_MouseEnter(object sender, MouseEventArgs e)
        {
            _mainImage.Source = _hoverImage;;
            Background = new SolidColorBrush(ColorScheme.MenuLight);
        }
    }
    #endregion

    class CustomComboBox : DockPanel
    {
        private Popup _mainPopup;
        private StackPanel _popupStack;
        private TextBlock _textBlock;

        public List<CustomComboBoxItem> Items = new List<CustomComboBoxItem>();

        public event EventHandler SelectedItemChanged;

        public CustomComboBoxItem SelectedItem { get; set; }

        public CustomComboBox()
        {   
            Height = 40;

            #region ArrowDownButton
            Border arrowContainer = new Border()
            {
                Height = 40,
                Width = 50,
                Background = new SolidColorBrush(ColorScheme.MenuLight)
            };

            Image arrowImage = new Image()
            {
                Height = 10,
                Source = ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/arrow_down_blue.png")),
                VerticalAlignment= VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            arrowContainer.Child = arrowImage;
            DockPanel.SetDock(arrowContainer, Dock.Right);

            arrowContainer.MouseEnter += (s, ea) =>
            {
                arrowImage.Source =
                    ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/arrow_down_darker.png"));
                Cursor = Cursors.Hand;
            };

            arrowContainer.MouseLeave += (s, ea) =>
            {
                arrowImage.Source =
                    ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/arrow_down_blue.png"));
                Cursor = Cursors.Arrow;
            };

            arrowContainer.PreviewMouseLeftButtonDown += ArrowContainer_PreviewMouseLeftButtonDown;

            Children.Add(arrowContainer);
            #endregion

            #region MainTextBlock
            _textBlock = new TextBlock()
            {
                Height=40,
                Padding=new Thickness(10,10,10,0),
                FontSize=16,
                Text = "",
                Foreground = new SolidColorBrush(ColorScheme.GlobalBlue),
                Background = new SolidColorBrush(ColorScheme.MenuLight)
            };

            _textBlock.PreviewMouseLeftButtonDown += ArrowContainer_PreviewMouseLeftButtonDown;

            Children.Add(_textBlock);
            #endregion

            _mainPopup = new Popup()
            {
                PlacementTarget = this,
                Placement = PlacementMode.Bottom,
            };

            Border popupBorder = new Border()
            {
                BorderBrush = new SolidColorBrush(ColorScheme.GlobalBlue),
                BorderThickness = new Thickness(0,0,0,1)
            };

            _popupStack = new StackPanel()
            {
                Background = new SolidColorBrush(ColorScheme.MenuLight),
            };

            popupBorder.Child = _popupStack;
            _mainPopup.Child = popupBorder;

            Panel.SetZIndex(_mainPopup,302);
        }

        private void ArrowContainer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement clickedSender = (FrameworkElement)sender;
            clickedSender.PreviewMouseLeftButtonUp += ClickedSender_PreviewMouseLeftButtonUp;
        }

        private void ClickedSender_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement clickedSender = (FrameworkElement)sender;

            _mainPopup.PopupAnimation = PopupAnimation.Slide;
            _mainPopup.StaysOpen = false;
            _mainPopup.AllowsTransparency = true;
            _mainPopup.IsOpen = true;
            _mainPopup.Width = ActualWidth;

            clickedSender.PreviewMouseLeftButtonUp -= ClickedSender_PreviewMouseLeftButtonUp;
        }

        public void AddItem(string caption, bool enabled = true)
        {
            CustomComboBoxItem item = new CustomComboBoxItem(caption, enabled);

            item.SelectedChanged += Item_SelectedChanged;

            if (Items.Count == 0)
            {
                SelectedItem = item;
                item.Selected = true;
                _textBlock.Text = item.Caption;
            }

            Items.Add(item);
            _popupStack.Children.Add(item);
        }

        private void Item_SelectedChanged(object sender, EventArgs e)
        {
            if(SelectedItem != null)
                SelectedItem.Selected = false;

            SelectedItemChanged?.Invoke(this, EventArgs.Empty);

            SelectedItem = (CustomComboBoxItem)sender;
            _textBlock.Text = SelectedItem.Caption;
            _mainPopup.IsOpen = false;
        }
    }

    class CustomComboBoxItem:TextBlock
    {
        private bool _selected;

        public string Caption { get; private set; }
        public bool Enabled { get; private set; }

        public event EventHandler SelectedChanged;

        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                if (Selected)
                {
                    Foreground = new SolidColorBrush(ColorScheme.MenuLight);
                    Background = new SolidColorBrush(ColorScheme.GlobalBlue);
                }
                else
                {
                    Foreground = new SolidColorBrush(ColorScheme.GlobalBlue);
                    Background = new SolidColorBrush(ColorScheme.MenuLight);
                }
            }
        }
        public CustomComboBoxItem(string caption, bool enabled)
        {
            Caption = caption;
            Enabled = enabled;
            VerticalAlignment = VerticalAlignment.Top;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Padding = new Thickness(10, 10, 0, 10);
            Text = caption;
            Foreground = Enabled ? new SolidColorBrush(ColorScheme.GlobalBlue) : new SolidColorBrush(ColorScheme.MenuDark);
            Background = new SolidColorBrush(ColorScheme.MenuLight);
            FontSize = 16;
            Height = 40;

            MouseEnter += (s, ea) =>
            {
                if (!Selected && Enabled)
                {
                    Foreground = new SolidColorBrush(ColorScheme.MenuLight);
                    Background = new SolidColorBrush(ColorScheme.GlobalBlue);
                }
            };

            MouseLeave += (s, ea) =>
            {
                if (!Selected && Enabled)
                {
                    Foreground = new SolidColorBrush(ColorScheme.GlobalBlue);
                    Background = new SolidColorBrush(ColorScheme.MenuLight);
                }
            };

            PreviewMouseLeftButtonDown += (s, ea) =>
            {
                    PreviewMouseLeftButtonUp += CustomComboBoxItem_PreviewMouseLeftButtonUp;
            };
        }

        private void CustomComboBoxItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!Selected && Enabled)
            {
                Selected = true;
                SelectedChanged?.Invoke(this, EventArgs.Empty);
            }

            PreviewMouseLeftButtonUp -= CustomComboBoxItem_PreviewMouseLeftButtonUp;
        }
    }

    class CustomCheckBox : DockPanel
    {
        private string _caption;
        private int _height = 20;
        private Label _captionLabel;
        private Border _checkBoxBorder;
        private bool _selected;

        public event EventHandler SelectedChanged;

        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;

                if (Selected)
                {
                    _checkBoxBorder.BorderBrush = new SolidColorBrush(ColorScheme.GlobalBlue);
                    _checkBoxBorder.BorderThickness = new Thickness(6);
                }
                else
                {
                    _checkBoxBorder.BorderThickness = new Thickness(1);
                }
            }
        }

        public string Caption
        {
            get { return _caption; }
            set
            {
                _caption = value;
                _captionLabel.Content = Caption;
            }
        }
        public CustomCheckBox()
        {
            LastChildFill = false;
            Height = 40;
  
            _checkBoxBorder = new Border()
            {
                Height = _height,
                Width = _height,
                CornerRadius = new CornerRadius(0.2*_height),
                BorderBrush = new SolidColorBrush(ColorScheme.MenuDark),
                BorderThickness = new Thickness(1),
                Margin = new Thickness(10),
                Background = new SolidColorBrush(Colors.Transparent)
            };

            SetDock(_checkBoxBorder, Dock.Left);
            Children.Add(_checkBoxBorder);

            _checkBoxBorder.MouseEnter += _checkBoxBorder_MouseEnter;
            _checkBoxBorder.MouseLeave += _checkBoxBorder_MouseLeave;

            _checkBoxBorder.PreviewMouseLeftButtonDown += _checkBoxBorder_PreviewMouseLeftButtonDown;

            _captionLabel = new Label()
            {
                Height = 40,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(0, 0, 10, 0),
                Foreground = new SolidColorBrush(ColorScheme.GlobalBlue),
                FontSize = 16
            };

            Children.Add(_captionLabel);
        }

        private void _checkBoxBorder_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _checkBoxBorder.PreviewMouseLeftButtonUp += _checkBoxBorder_PreviewMouseLeftButtonUp;
        }

        private void _checkBoxBorder_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Selected = !Selected;
            SelectedChanged?.Invoke(this, EventArgs.Empty);
            _checkBoxBorder.PreviewMouseLeftButtonUp -= _checkBoxBorder_PreviewMouseLeftButtonUp;
        }

        private void _checkBoxBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!Selected)
                _checkBoxBorder.BorderBrush = new SolidColorBrush(ColorScheme.MenuDark);
        }

        private void _checkBoxBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!Selected)
                _checkBoxBorder.BorderBrush = new SolidColorBrush(ColorScheme.GlobalBlue);
        }
    }

    public enum UpperMenuMode
    {
        HomePage,
        Conversation,
        Document,
        Email,
        Contact
    }


}
