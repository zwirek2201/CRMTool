using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using Licencjat_new.CustomClasses;

namespace Licencjat_new.Controls
{
    #region Panels
    internal class NotificationsPanelDarkener : Grid
    {
        #region Constructors
        public NotificationsPanelDarkener()
        {
            VerticalAlignment = VerticalAlignment.Stretch;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Background = new SolidColorBrush(Colors.Black) { Opacity = 0.5 };
        }
        #endregion
    }

    public class NotificationsPanel : Canvas
    {
        #region Variables
        private NotificationButton _boundNotificationButton;
        private JustInTimeNotificationPanel _boundJustInTimeNotificationPanel;

        public EventHandler<NotificationsReadEventArgs>  NotificationsRead;
        private NotificationsPanelDarkener _darkener;
        private Canvas _notificationsMainCanvas;
        private StackPanel _notificationsStackPanel;
        #endregion

        #region Properties
        public double MainHeight { get; }

        public double MainWidth { get; }

        public double PanelWidth { get; }

        public double PanelHeight { get; }

        public List<NotificationItem> Notifications { get; set; } = new List<NotificationItem>();

        public NotificationButton BoundNotificationButton
        {
            get { return _boundNotificationButton; }
            set
            {
                _boundNotificationButton = value;
                _boundNotificationButton.Clicked += _boundNotificationButton_Clicked;
            }
        }

        public JustInTimeNotificationPanel BoundJustInTimeNotificationPanel
        {
            get { return _boundJustInTimeNotificationPanel; }
            set
            {
                _boundJustInTimeNotificationPanel = value;
                _boundJustInTimeNotificationPanel.NotificationElapsed += _boundJustInTimeNotificationPanel_NotificationElapsed;
            }
        }
        #endregion

        #region Constructors
        public NotificationsPanel(double width, double height)
        {
            MainWidth = width;
            MainHeight = height;

            PanelWidth = 300;
            PanelHeight = MainHeight - 20;

            Height = MainHeight;
            Width = MainWidth;
            Canvas.SetZIndex(this, 100);

            Visibility = Visibility.Collapsed;

            #region Darkener

            _darkener = new NotificationsPanelDarkener()
            {
                Width = MainWidth,
                Height = MainHeight
            };

            _darkener.PreviewMouseLeftButtonDown += _darkener_PreviewMouseLeftButtonDown;

            this.Children.Add(_darkener);

            #endregion

            #region MainNotificationCanvas

            _notificationsMainCanvas = new Canvas()
            {
                Width = PanelWidth,
                Height = PanelHeight,
            };

            _notificationsMainCanvas.BeginInit();

            Panel.SetZIndex(_notificationsMainCanvas, 101);

            Canvas.SetTop(_notificationsMainCanvas, 5);

            _notificationsMainCanvas.Effect =
                new DropShadowEffect
                {
                    Direction = 320,
                    ShadowDepth = 0,
                    Opacity = 0.4,
                    BlurRadius = 8
                };

            this.Children.Add(_notificationsMainCanvas);

            #endregion

            #region BlueArrow

            Image blueArrowImage = new Image()
            {
                Source =
                    ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/blue_arrow.png")),
                Width = 20,
                Height = 10,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            RenderOptions.SetBitmapScalingMode(blueArrowImage, BitmapScalingMode.HighQuality);

            Canvas.SetTop(blueArrowImage, 0);
            Canvas.SetRight(blueArrowImage, 10);

            _notificationsMainCanvas.Children.Add(blueArrowImage);

            #endregion

            #region NotificationInnerDock

            DockPanel notificationInnerDock = new DockPanel()
            {
                Width = PanelWidth,
                Height = PanelHeight - 10,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(ColorScheme.MenuLight)
            };

            Canvas.SetTop(notificationInnerDock, 10);

            _notificationsMainCanvas.Children.Add(notificationInnerDock);

            #endregion

            #region TitlePanel

            StackPanel titlePanel = new StackPanel()
            {
                Height = 30,
                Width = width,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(ColorScheme.GlobalBlue),
                Orientation = Orientation.Horizontal
            };

            DockPanel.SetDock(titlePanel, Dock.Top);

            notificationInnerDock.Children.Add(titlePanel);

            Label titleTextBlock = new Label()
            {
                Height = 30,
                FontSize = 15,
                Margin = new Thickness(10, 0, 0, 0),
                Foreground = new SolidColorBrush(ColorScheme.GlobalWhite),
                Content = "Powiadomienia",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Center
            };

            titlePanel.Children.Add(titleTextBlock);

            #endregion

            #region NotificationsScroll
            ScrollViewer notificationsScroll = new ScrollViewer()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalContentAlignment = VerticalAlignment.Top,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            };
            #endregion

            #region NotificationsStackPanel

            _notificationsStackPanel = new StackPanel()
            {
                Background = new SolidColorBrush(ColorScheme.MenuLight),
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Left,
                Orientation = Orientation.Vertical,
            };

            notificationsScroll.Content = _notificationsStackPanel;

            notificationInnerDock.Children.Add(notificationsScroll);

            #endregion

        }
        #endregion

        #region Events
        private void _boundJustInTimeNotificationPanel_NotificationElapsed(object sender, EventArgs e)
        {
            BoundNotificationButton.UnreadNotificationsCount++;
        }

        private void _darkener_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            NotificationsPanelDarkener darkener = (NotificationsPanelDarkener)sender;
            darkener.PreviewMouseLeftButtonUp += Darkener_PreviewMouseLeftButtonUp;
        }

        private void Darkener_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            NotificationsPanelDarkener darkener = (NotificationsPanelDarkener)sender;
            _boundNotificationButton.Toggled = false;
            this.HideMe();
            darkener.PreviewMouseLeftButtonUp -= Darkener_PreviewMouseLeftButtonUp;
        }

        private void _boundNotificationButton_Clicked(object sender, EventArgs e)
        {
            NotificationButton notificationButton = (NotificationButton)sender;

            if (notificationButton.Toggled)
            {
                ShowMe();
            }
            else
            {
                HideMe();
            }
        }

        private void FadeOutAnimation_Completed(object sender, EventArgs e)
        {
            _darkener.Visibility = Visibility.Collapsed;
            this.Visibility = Visibility.Collapsed;
        }
        #endregion

        #region Methods
        private void ShowMe()
        {
            _darkener.Opacity = 0;
            Canvas.SetRight(_notificationsMainCanvas, 0);
            this.Visibility = Visibility.Visible;
            _darkener.Visibility = Visibility.Visible;

            DoubleAnimation fadeInAnimation = new DoubleAnimation()
            {
                From = 0,
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(200))
            };
            _darkener.BeginAnimation(OpacityProperty, fadeInAnimation);

            DoubleAnimation slideLeftAnimation = new DoubleAnimation()
            {
                From = -_notificationsMainCanvas.Width,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(200))
            };
            _notificationsMainCanvas.BeginAnimation(RightProperty, slideLeftAnimation);

            BoundNotificationButton.UnreadNotificationsCount = 0;

            List<NotificationModel> unreadNotifications = new List<NotificationModel>();

            foreach (NotificationItem notification in Notifications)
            {
                notification.UpdateDate();

                if (!notification.Notification.Read)
                {
                    unreadNotifications.Add(notification.Notification);
                }
            }

            NotificationsRead?.Invoke(this, new NotificationsReadEventArgs(unreadNotifications));
        }

        private void HideMe()
        {
            _darkener.Opacity = 1;
            Canvas.SetRight(_notificationsMainCanvas, -_notificationsMainCanvas.Width);

            DoubleAnimation fadeOutAnimation = new DoubleAnimation();
            fadeOutAnimation.From = 1;
            fadeOutAnimation.To = 0;
            fadeOutAnimation.Completed += FadeOutAnimation_Completed;
            fadeOutAnimation.Duration = new Duration(TimeSpan.FromMilliseconds(200));
            _darkener.BeginAnimation(OpacityProperty, fadeOutAnimation);

            DoubleAnimation slideRightAnimation = new DoubleAnimation()
            {
                From = 0,
                To = -_notificationsMainCanvas.Width,
                Duration = new Duration(TimeSpan.FromMilliseconds(200))
            };
            _notificationsMainCanvas.BeginAnimation(RightProperty, slideRightAnimation);
        }

        public void AddNotification(NotificationModel notification)
        {
            NotificationItem notificationItem = new NotificationItem(notification);
            Notifications.Add(notificationItem);

            _notificationsStackPanel.Children.Insert(0, notificationItem);
        }

        public void RemoveNotification(NotificationModel notification)
        {
            _notificationsStackPanel.Children.Remove(Notifications.Find(obj => obj.Notification == notification));
            this.Notifications.Remove(Notifications.Find(obj => obj.Notification == notification));
        }
        #endregion
    }

    public class JustInTimeNotificationPanel : StackPanel
    {
        #region Variables
        internal event EventHandler NotificationElapsed;
        public event EventHandler NotificationClosed;
        #endregion

        #region Constructors
        public JustInTimeNotificationPanel()
        {
            Width = 320;
            Orientation = Orientation.Vertical;

            Canvas.SetZIndex(this, 300);
            Canvas.SetBottom(this, 20);
            Canvas.SetRight(this, 0);
        }
        #endregion

        #region Events
        private void NotificationItem_JustInTimeNotificationElapsed(object sender, EventArgs e)
        {
            this.Dispatcher.Invoke(delegate ()
            {
                NotificationItem notification = (NotificationItem)sender;
                Children.Remove(notification);
                if(!notification.Notification.Ghost)
                    NotificationElapsed?.Invoke(this, EventArgs.Empty);
            });
        }

        private void NotificationItem_JustInTimeNotificationClosed(object sender, EventArgs e)
        {
            NotificationItem notification = (NotificationItem)sender;
            NotificationClosed?.Invoke(notification, EventArgs.Empty);
            Children.Remove(notification);
        }
        #endregion

        #region Methods

        public void AddNotification(NotificationModel notification)
        {
            NotificationItem notificationItem = new NotificationItem(notification, true);

            notificationItem.JustInTimeNotificationClosed += NotificationItem_JustInTimeNotificationClosed;
            notificationItem.JustInTimeNotificationElapsed += NotificationItem_JustInTimeNotificationElapsed;

            Children.Add(notificationItem);
        }

        #endregion
    }
    #endregion

    public class NotificationButton : Canvas
    {
        #region Variables
        private Image _notificationImage;
        private int _unreadNotificationsCount = 0;
        private bool _toggled = false;
        private Border _notificationsCountBlockContainer;
        private TextBlock _notificationsCountBlock;
        public event EventHandler Clicked;
        #endregion

        #region Properties
        public int UnreadNotificationsCount
        {
            get { return _unreadNotificationsCount; }
            set
            {
                _unreadNotificationsCount = value;
                if (_unreadNotificationsCount > 0)
                {
                    _notificationsCountBlock.Text = _unreadNotificationsCount.ToString();
                    _notificationsCountBlockContainer.Visibility = Visibility.Visible;
                }
                else
                {
                    _notificationsCountBlockContainer.Visibility = Visibility.Collapsed;
                }
            }
        }

        public bool Toggled
        {
            get { return _toggled; }

            set
            {
                _toggled = value;
                ChangeButtonAppearance(_toggled);
            }
        }
        #endregion

        #region Constructors
        public NotificationButton()
        {
            Height = 40;
            Width = 40;
            Background = new SolidColorBrush(ColorScheme.GlobalBlue);
            VerticalAlignment = VerticalAlignment.Center;
            Panel.SetZIndex(this, 103);

            _notificationImage = new Image()
            {
                Source =
                    ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/notification_off.png")),
                Width = 20,
                Height = 20,
                Margin = new Thickness(10, 10, 10, 10),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            RenderOptions.SetBitmapScalingMode(_notificationImage, BitmapScalingMode.HighQuality);
            Children.Add(_notificationImage);

            _notificationsCountBlockContainer = new Border()
            {
                Background = new SolidColorBrush(ColorScheme.GlobalDarkRed),
                CornerRadius = new CornerRadius(4),
                Height = 14,
                Visibility = Visibility.Collapsed
            };

            _notificationsCountBlock = new TextBlock()
            {
                Foreground = new SolidColorBrush(ColorScheme.GlobalWhite),
                LineHeight = 11,
                FontSize = 11,
                Padding = new Thickness(3, 2, 3, 2),
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                FontWeight = FontWeights.Medium,
                Text = UnreadNotificationsCount.ToString()
            };

            _notificationsCountBlockContainer.Child = _notificationsCountBlock;

            Canvas.SetBottom(_notificationsCountBlockContainer, 5);
            Canvas.SetRight(_notificationsCountBlockContainer, 5);

            Children.Add(_notificationsCountBlockContainer);

            PreviewMouseLeftButtonDown += NotificationButton_PreviewMouseLeftButtonDown;
            MouseEnter += NotificationButton_MouseEnter;
            MouseLeave += NotificationButton_MouseLeave;
        }
        #endregion

        #region Events
        private void NotificationButton_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!Toggled)
            {
                ChangeButtonAppearance(false);
            }
        }

        private void NotificationButton_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!Toggled)
            {
                ChangeButtonAppearance(true);
            }
        }

        private void NotificationButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PreviewMouseLeftButtonUp += NotificationButton_PreviewMouseLeftButtonUp;
        }

        private void NotificationButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Toggled = !Toggled;
            Clicked?.Invoke(this, EventArgs.Empty);
            PreviewMouseLeftButtonUp -= NotificationButton_PreviewMouseLeftButtonUp;
        }
        #endregion

        #region Methods
        private void ChangeButtonAppearance(bool toggled)
        {
            Background = new SolidColorBrush(toggled ? ColorScheme.MenuLight : ColorScheme.GlobalBlue);
            _notificationImage.Source =
                ImageHelper.UriToImageSource(
                    new Uri(toggled ? @"pack://application:,,,/resources/notification_on.png" : @"pack://application:,,,/resources/notification_off.png" ));
        }
        #endregion
    }

    public class NotificationItem : Border
    {
        #region Variables
        private Timer _closeTimer;
        private TextBlock _dateText;

        public event EventHandler JustInTimeNotificationElapsed;
        public event EventHandler JustInTimeNotificationClosed;
        #endregion

        #region Properties
        public NotificationModel Notification { get; private set; }
        public string NotificationText { get; private set; }
        public bool JustInTime { get; set; }
        #endregion

        #region Constructors
        public NotificationItem(NotificationModel notification, bool justInTime = false)
        {
            Notification = notification;
            NotificationText = notification.Text;
            JustInTime = justInTime;

            if (JustInTime)
            {
                _closeTimer = new Timer(5000);
                _closeTimer.Elapsed += CloseTimer_Elapsed;
                _closeTimer.Start();
            }

            Background = new SolidColorBrush(ColorScheme.MenuLight);
            BorderBrush = new SolidColorBrush(ColorScheme.MenuDarker);
            BorderThickness = JustInTime ? new Thickness(1) : new Thickness(0, 0, 0, 1);
            Margin = JustInTime ? new Thickness(0,0,5,8) : new Thickness(0);
            Height = 50;
            Width = 300;
            HorizontalAlignment = HorizontalAlignment.Left;

            if (JustInTime)
            {
                this.Effect = new DropShadowEffect
                {
                    Direction = 320,
                    ShadowDepth = 0,
                    Opacity = 0.4,
                    BlurRadius = 8
                };
            }

            DockPanel mainStack = new DockPanel()
            {
                Height = 50,
                Width = JustInTime ? 300 : 285,
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            StackPanel innerStack = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            TextBlock notificationText = new TextBlock()
            {
                FontSize = 12,
                LineHeight = 13,
                LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                Foreground = new SolidColorBrush(ColorScheme.GlobalDarkText),
                HorizontalAlignment = HorizontalAlignment.Left,
                TextWrapping = TextWrapping.Wrap,
                Padding = new Thickness(10, 0, 10, 0)
            };

            #region TextMatching

            MatchCollection matches = Regex.Matches(NotificationText, @"(\{.[^\{\}]*\})|(\[.[^\[\]]*\])");

            if (matches.Count > 0)
            {
                string processedText = NotificationText;
                foreach (Match match in matches)
                {
                    string normalText = processedText.Substring(0, processedText.IndexOf(match.Value));

                    notificationText.Inlines.Add(normalText);
                    notificationText.Inlines.Add(new Run(match.Value.Trim('{', '}','[',']')) {FontWeight = FontWeights.Medium});

                    int runLength = normalText.Length + match.Length;
                    processedText = processedText.Substring(runLength, processedText.Length - runLength);
                }

                notificationText.Inlines.Add(processedText);
            }
            else
            {
                notificationText.Inlines.Add(NotificationText);
            }

            #endregion

            innerStack.Children.Add(notificationText);

            if (!JustInTime)
            {
                _dateText = new TextBlock()
                {
                    FontSize = 12,
                    LineHeight = 15,
                    LineStackingStrategy = LineStackingStrategy.BlockLineHeight,
                    Foreground = new SolidColorBrush(ColorScheme.MenuDark),
                    HorizontalAlignment = HorizontalAlignment.Left,
                    TextWrapping = TextWrapping.Wrap,
                    Padding = new Thickness(10, 0, 10, 0),
                    Text = DateHelper.DateToDateContraction(notification.NotificationDate)
                };

                innerStack.Children.Add(_dateText);
            }

            if (JustInTime)
            {
                Image closeImage = new Image()
                {
                    Width = 11,
                    Height = 11,
                    Margin = new Thickness(5, 7, 7, 5),
                    Source = ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/close_gray.png")),
                    VerticalAlignment = VerticalAlignment.Top,
                };

                closeImage.PreviewMouseLeftButtonDown += CloseImage_PreviewMouseLeftButtonDown;

                DockPanel.SetDock(closeImage, Dock.Right);
                mainStack.Children.Add(closeImage);
            }

            mainStack.Children.Add(innerStack);

            Child = mainStack;
        }
        #endregion

        #region Events
        private void CloseImage_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Image image = (Image)sender;
            image.PreviewMouseLeftButtonUp += Image_PreviewMouseLeftButtonUp;
        }

        private void Image_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Image image = (Image)sender;
            _closeTimer?.Stop();
            JustInTimeNotificationClosed?.Invoke(this, EventArgs.Empty);
            image.PreviewMouseLeftButtonUp -= Image_PreviewMouseLeftButtonUp;
        }

        private void CloseTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            _closeTimer?.Stop();
            JustInTimeNotificationElapsed?.Invoke(this, EventArgs.Empty);
        }
        #endregion

        #region Methods
        public void UpdateDate()
        {
            _dateText.Text = DateHelper.DateToDateContraction(Notification.NotificationDate);
        }
        #endregion
    }

    public class NotificationsReadEventArgs
    {
        public List<NotificationModel> Notifications { get; private set; }

        public NotificationsReadEventArgs(List<NotificationModel> notifications)
        {
            Notifications = notifications;
        }
    }

}