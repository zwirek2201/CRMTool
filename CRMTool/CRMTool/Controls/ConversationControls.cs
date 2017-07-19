using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Licencjat_new.CustomClasses;
using Licencjat_new.Windows;
using TheArtOfDev.HtmlRenderer.WPF;
using ContextMenu = System.Windows.Controls.ContextMenu;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using Label = System.Windows.Controls.Label;
using MenuItem = System.Windows.Controls.MenuItem;
using Orientation = System.Windows.Controls.Orientation;

namespace Licencjat_new.Controls
{
    #region ConversationList
    class ConversationListContainer : DockPanel
    {
        #region Variables

        private bool _showButtons;
        private ToolBarMainMenuStrip _mainMenu;

        public event EventHandler AddConversation;
        #endregion

        #region Properties

        public bool ShowButtons
        {
            get { return _showButtons; }
            set
            {
                _showButtons = value;
                if (!ShowButtons)
                    _mainMenu?.Children.Clear();
            }
        }
        #endregion

        public ConversationListContainer()
        {
            Width = double.NaN;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;
            Background = new SolidColorBrush(ColorScheme.MenuLight);
            LastChildFill = true;

            _mainMenu = new ToolBarMainMenuStrip();

            ToolBarButton addConversation = new ToolBarButton("Nowa konwersacja",
                new Uri("pack://application:,,,/resources/add_conversation.png"));

            addConversation.Click += AddConversation_Click;
            _mainMenu.Children.Add(addConversation);

            Children.Insert(0, _mainMenu);
        }

        private void AddConversation_Click(object sender, EventArgs e)
        {
            AddConversation?.Invoke(this, EventArgs.Empty);
        }
    }

    class ConversationList : StackPanel
    {
        #region Variables
        private ConversationModel _selectedConversation;
        private ConversationListItem _selectedConversationItem;
        private bool _displayItemContextMenus = true;

        public event EventHandler<SelectedConversationChangedEventArgs> SelectedConversationChanged;
        public event EventHandler RemoveConversation;
        public event EventHandler RenameConversation;
        public event EventHandler ShowConversationDetails;
        public event EventHandler ShowConversationSettings;

        private Label NoElementsPlaceholder;
        #endregion

        #region Properties
        public bool DisplayItemContextMenus
        {
            get { return _displayItemContextMenus; }
            set
            {
                _displayItemContextMenus = value;

                if (!DisplayItemContextMenus)
                {
                    foreach (ConversationListItem conversationItem in Conversations)
                        conversationItem.ContextMenu = null;
                }
            }
        }
        internal ConversationListItem SelectedConversationItem
        {
            get { return _selectedConversationItem; }
            set
            {
                if (_selectedConversationItem != value)
                {
                    if (_selectedConversationItem != null)
                        _selectedConversationItem.Selected = false;

                    value.Selected = true;
                    _selectedConversationItem = value;
                    SelectedConversation = _selectedConversationItem.Conversation;
                }
            }
        }

        public ConversationModel SelectedConversation
        {
            get { return _selectedConversation; }
            internal set
            {
                    _selectedConversation = value;
                    OnSelectedConversationChanged(new SelectedConversationChangedEventArgs()
                    {
                        Conversation = _selectedConversation
                    });
            }
        }

        public List<ConversationListItem> Conversations { get; } = new List<ConversationListItem>();
        #endregion

        #region Constructors
        public ConversationList()
        {
            Name = "ConversationList";
            Background = new SolidColorBrush(ColorScheme.MenuLight);
            Orientation = Orientation.Vertical;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;

            NoElementsPlaceholder = new Label()
            {
                Content = "Brak konwersacji",
                Foreground = new SolidColorBrush(ColorScheme.MenuDark),
                FontSize = 14,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Top,
                VerticalAlignment = VerticalAlignment.Stretch,
                Padding = new Thickness(0, 20, 0, 0)
            };

            Children.Add(NoElementsPlaceholder);
        }
        #endregion

        #region Events
        internal virtual void OnSelectedConversationChanged(SelectedConversationChangedEventArgs e)
        {
            EventHandler<SelectedConversationChangedEventArgs> handler = SelectedConversationChanged;
            handler?.Invoke(this, e);
        }

        private void ConversationItem_RenameConversation(object sender, EventArgs e)
        {
            RenameConversation?.Invoke(sender, e);
        }

        private void ConversationItem_RemoveConversation(object sender, EventArgs e)
        {
            RemoveConversation?.Invoke(sender, e);
        }

        private void ConversationItem_ShowConversationDetails(object sender, EventArgs e)
        {
            ShowConversationDetails?.Invoke(sender, e);
        }

        private void ConversationItem_ShowConversationSettings(object sender, EventArgs e)
        {
            ShowConversationSettings?.Invoke(sender, e);
        }
        #endregion

        #region Methods

        public void AddConversations(List<ConversationModel> conversations)
        {
            foreach (ConversationModel conversation in conversations)
            {
                ConversationListItem conversationItem = new ConversationListItem(conversation);
                conversationItem.PreviewMouseRightButtonDown += ConversationItem_PreviewMouseRightButtonDown;
                conversationItem.ConversationList = this;
                conversationItem.RemoveConversation += ConversationItem_RemoveConversation;
                conversationItem.RenameConversation += ConversationItem_RenameConversation;
                conversationItem.ShowConversationDetails += ConversationItem_ShowConversationDetails;

                Conversations.Add(conversationItem);
                Children.Add(conversationItem);

                if (!DisplayItemContextMenus)
                    conversationItem.ContextMenu = null;

                Children.Remove(NoElementsPlaceholder);
            }
        }

        private void ConversationItem_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ConversationListItem item = (ConversationListItem)sender;
            SelectedConversationItem = item;
        }

        public void AddConversation(ConversationListItem conversationItem)
        {
            conversationItem.ConversationList = this;
            conversationItem.PreviewMouseRightButtonDown += ConversationItem_PreviewMouseRightButtonDown;
            conversationItem.RemoveConversation += ConversationItem_RemoveConversation;
            conversationItem.RenameConversation += ConversationItem_RenameConversation;
            conversationItem.ShowConversationDetails += ConversationItem_ShowConversationDetails;

            Conversations.Add(conversationItem);
            Children.Add(conversationItem);

            if (!DisplayItemContextMenus)
                conversationItem.ContextMenu = null;

            Children.Remove(NoElementsPlaceholder);
        }

        public void Clear()
        {
            Children.Clear();
            Conversations.Clear();
        }

        public void RemoveConversationFromList(ConversationListItem conversationItem)
        {
            Conversations.Remove(conversationItem);
            Children.Remove(conversationItem);

            if(Conversations.Count == 0)
                Children.Add(NoElementsPlaceholder);
        }
        #endregion
    }

    class ConversationListItem : Border
    {
        #region Variables
        private ConversationModel _conversation;
        private int _height = 45;
        private DockPanel _mainDock;
        private Boolean _selected;

        public event EventHandler RemoveConversation;
        public event EventHandler RenameConversation;
        public event EventHandler MuteConversation;
        public event EventHandler ShowConversationDetails;

        #endregion

        #region Properties
        public ConversationList ConversationList { get; internal set; }

        public Boolean Selected
        {
            get { return _selected; }
            internal set
            {
                _selected = value;
                if (_selected)
                {
                    _mainDock.Background = new SolidColorBrush(ColorScheme.MenuDarker);
                    //_settingsImage.Source =
                    //    ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/settings.png"));
                }
                else
                {
                    _mainDock.Background = new SolidColorBrush(ColorScheme.MenuLight);
                    //_settingsImage.Source = null;
                }
            }
        }

        public ConversationModel Conversation {
            get
            {
                return _conversation;
            }
            internal set
            {
                _conversation = value;
                Redraw();
            }
        }
        #endregion

        #region Constructors
        public ConversationListItem()
        {
            Init();
        }

        public ConversationListItem(ConversationModel conversation)
        {
            Conversation = conversation;

            Conversation.DataChanged += Conversation_DataChanged;

            Init();
        }
        #endregion

        #region Initialization

        private void Init()
        {
            AllowDrop = true;
            MouseEnter += ConversationListItem_MouseEnter;
            MouseLeave += ConversationListItem_MouseLeave;
            MouseLeftButtonDown += ConversationListItem_MouseDown;

            ContextMenu contextMenu = new ContextMenu();

            MenuItem detailsItem = new MenuItem()
            {
                Header = "Pokaż szczegóły",
                Icon =
        new Image()
        {
            Source =
                ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/info_context.png"))
        }
            };
            detailsItem.Click += DetailsItem_Click;
            contextMenu.Items.Add(detailsItem);

            MenuItem renameItem = new MenuItem()
            {
                Header = "Zmień nazwę",
                Icon =
                    new Image()
                    {
                        Source =
                            ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/rename_context.png"))
                    }
            };
            renameItem.Click += RenameItem_Click;
            contextMenu.Items.Add(renameItem);

            contextMenu.Items.Add(new Separator());

            MenuItem removeItem = new MenuItem()
            {
                Header = "Usuń",
                Icon =
                    new Image()
                    {
                        Source =
                            ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/remove_context.png"))
                    }
            };
            removeItem.Click += RemoveItem_Click;
            contextMenu.Items.Add(removeItem);

            ContextMenu = contextMenu;
        }

        private void _settingsImage_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void _settingsImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        #endregion

        #region Events
        private void ConversationListItem_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            SetBackgroundColor(new SolidColorBrush(ColorScheme.MenuDarker));
    ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/settings.png"));
        }

        private void ConversationListItem_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!_selected)
            {
                SetBackgroundColor(new SolidColorBrush(ColorScheme.MenuLight));
            }
        }

        private void ConversationListItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseLeftButtonUp += ConversationListItem_MouseUp;
        }

        private void ConversationListItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseLeftButtonUp -= ConversationListItem_MouseUp;

            ConversationList.SelectedConversationItem = this;
        }

        private void RenameItem_Click(object sender, RoutedEventArgs e)
        {
            RenameConversation?.Invoke(this, EventArgs.Empty);
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            RemoveConversation?.Invoke(this, EventArgs.Empty);
        }

        private void DetailsItem_Click(object sender, RoutedEventArgs e)
        {
            ShowConversationDetails?.Invoke(this, EventArgs.Empty);
        }

        private void Conversation_DataChanged(object sender, EventArgs e)
        {
            Redraw();
        }
        #endregion

        #region Methods
        public void Redraw()
        {
            Height = _height;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            BorderBrush = new SolidColorBrush(ColorScheme.MenuDarker);
            BorderThickness = new Thickness(0, 0, 0, 1);

            _mainDock = new DockPanel()
            {
                Height = _height,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush(ColorScheme.MenuLight),
            };

            StackPanel rightPanel = new StackPanel()
            {
                Orientation = Orientation.Vertical,
                Height = _height,
                Width = _height,
                Background = new SolidColorBrush(Colors.Transparent),
                HorizontalAlignment = HorizontalAlignment.Right
            };

            DockPanel.SetDock(rightPanel, Dock.Right);
       
            _mainDock.Children.Add(rightPanel);


            StackPanel mainPanel = new StackPanel()
            {
                Margin = new Thickness(0, 0, 0, 0),
                Orientation = Orientation.Vertical,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(Colors.Transparent)
            };

            Label mainLabel = new Label()
            {
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                FontWeight = FontWeights.Medium,
                Padding = new Thickness(10,0,5,0),
                Foreground = new SolidColorBrush(ColorScheme.GlobalBlue),
                Content = Conversation.Name,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                FontSize = 13,
                Height = 25,
                Background = new SolidColorBrush(Colors.Transparent)
            };

            Label lowerLabel = new Label()
            {
                VerticalContentAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(10, 0, 5, 0),
                Foreground = new SolidColorBrush(ColorScheme.GlobalDarkText),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                FontSize = 12,
                Height = 20,
                Background = new SolidColorBrush(Colors.Transparent)
            };

            lowerLabel.Content = "";

            if (Conversation.Members.Count > 1)
            {
                foreach(PersonModel member in Conversation.Members)
                {
                    string name = member.FullName;
                    string[] nameSplit = name.Split(' ');
                    nameSplit[0] = nameSplit[0].Substring(0, 1).ToUpper() + ".";

                    lowerLabel.Content += nameSplit[0] + " " + nameSplit[1];

                    if (Conversation.Members.Last() != member)
                        lowerLabel.Content += ", ";
                }
            }
            else
            {
                lowerLabel.Content = Conversation.Members[0].FullName;
            }

            mainPanel.Children.Add(mainLabel);
            mainPanel.Children.Add(lowerLabel);
            _mainDock.Children.Add(mainPanel);
            Child = _mainDock;
        }

        public void SetBackgroundColor(Brush color)
        {
            _mainDock.Background = color;

        }
        #endregion
    }
    #endregion

    #region ConversationMainContainer
    class ConversationMainContainer : Canvas
    {
        public ConversationMainContainer()
        {
            Background = new SolidColorBrush(ColorScheme.MenuLight);
            VerticalAlignment = VerticalAlignment.Stretch;
            HorizontalAlignment = HorizontalAlignment.Stretch;
        }
    }

    #region ConversationMessageList
    class ConversationMessageList : StackPanel
    {
        #region Variables
        private List<ConversationMessageListItem> _conversationItems;

        public event EventHandler DownloadAllAttachments;
        public event EventHandler RenameFile;
        public event EventHandler DownloadFile;
        public event EventHandler ShowMessageDetails;

        private Label NoElementsPlaceholder;
        #endregion

        #region Properties
        public ConversationList BoundConversationList { get; set; }
        #endregion

        #region Constructors
        public ConversationMessageList()
        {
            Orientation = Orientation.Vertical;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            Background = new SolidColorBrush(ColorScheme.MenuLight);

            NoElementsPlaceholder = new Label()
            {
                Content = "Brak wiadomości",
                Foreground = new SolidColorBrush(ColorScheme.MenuDark),
                FontSize = 14,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Top,
                VerticalAlignment = VerticalAlignment.Stretch,
                Padding = new Thickness(0, 20, 0, 0)
            };

            Children.Add(NoElementsPlaceholder);
        }
        #endregion

        #region Methods
        public void AddMessage(ConversationMessageModel message)
        {
            ConversationMessageListItem messageItem = new ConversationMessageListItem(message);

            messageItem.DownloadAllAttachments += MessageItem_DownloadAllAttachments;
            messageItem.RenameFile += MessageItem_RenameFile;
            messageItem.DownloadFile += MessageItem_DownloadFile;
            messageItem.ShowDetails += MessageItem_ShowDetails;

            Children.Remove(NoElementsPlaceholder);

            Children.Add(messageItem);
        }

        private void MessageItem_ShowDetails(object sender, EventArgs e)
        {
            ShowMessageDetails?.Invoke(sender, e);
        }

        private void MessageItem_RenameFile(object sender, EventArgs e)
        {
            RenameFile?.Invoke(sender, e);
        }

        private void MessageItem_DownloadFile(object sender, EventArgs e)
        {
            DownloadFile?.Invoke(sender, e);
        }

        public void AddMessages(List<ConversationMessageModel> messages)
        {
            DateTime? previousDate = null;

            foreach (ConversationMessageModel message in messages)
            {
                DateTime messageDate = message.InitialDate.Value.Date;
                if (previousDate == null)
                {
                    Children.Add(new ConversationMessageListDateItem(messageDate.ToString("dd.MM.yyyy")));
                    previousDate = messageDate;
                }
                else
                {
                    if (messageDate != previousDate)
                    {
                        if (messageDate == DateTime.Today.Date)
                        {
                            Children.Add(new ConversationMessageListDateItem("Dzisiaj"));
                        }
                        else if (messageDate == DateTime.Today.Date.AddDays(-1))
                        {
                            Children.Add(new ConversationMessageListDateItem("Wczoraj"));
                        }
                        else
                        {
                            Children.Add(
                                new ConversationMessageListDateItem(messageDate.ToString("dd.MM.yyyy")));
                        }

                        previousDate = messageDate;
                    }
                }
                Children.Remove(NoElementsPlaceholder);
                AddMessage(message);
            }
        }

        public void ClearMessages()
        {
            Children.Clear();
            Children.Add(NoElementsPlaceholder);
        }
        #endregion

        #region Events
        private void MessageItem_DownloadAllAttachments(object sender, EventArgs e)
        {
            DownloadAllAttachments?.Invoke(sender, e);
        }

        #endregion
    }

    class ConversationMessageListItem : DockPanel
    {
        #region Variables

        public event EventHandler DownloadAllAttachments;
        public event EventHandler RenameFile;
        public event EventHandler DownloadFile;
        public event EventHandler ShowDetails;
        #endregion

        #region Properties

        public ConversationMessageModel Message { get; set; }
        #endregion

        #region Constructors
        public ConversationMessageListItem(ConversationMessageModel message)
        {
            Message = message;
            PreviewMouseLeftButtonDown += ConversationMessageListItem_MouseLeftButtonDown;

            SolidColorBrush leadColor = new SolidColorBrush(Message.Color);

            DockPanel outerDock = new DockPanel()
            {
                Width = double.NaN,
                Height = double.NaN,
                Margin = new Thickness(30, 5, 30, 0),
                VerticalAlignment = VerticalAlignment.Top
            };

            DockPanel messageMainDock = new DockPanel()
            {
                MinWidth = 250,
                MinHeight = 100,
                MaxWidth = 400,
                Width = double.NaN,
                Height = double.NaN,
                Background = new SolidColorBrush(ColorScheme.GlobalWhite),
            };

            #region Title
            Border titleBorder = new Border()
            {
                Height = 25,
                VerticalAlignment = VerticalAlignment.Top,
                Background = leadColor,
                CornerRadius = new CornerRadius(10, 10, 0, 0),
                BorderBrush = new SolidColorBrush(ColorScheme.MenuDarker),
                BorderThickness = new Thickness(1, 1, 1, 0)
            };

            DockPanel titlePanel = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush(Colors.Transparent),
                LastChildFill = true
            };

            Image messageTypeImage = new Image()
            {
                Height = 15,
                Width = 15,
                Margin = new Thickness(5, 5, 5, 5),
                VerticalAlignment = VerticalAlignment.Center,
            };

            Label authorLabel = new Label()
            {
                Height = 25,
                FontSize = 13,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(ColorScheme.GlobalWhite),
                Padding = new Thickness(0)
            };

            Label dateLabel = new Label()
            {
                Height = 25,
                FontSize = 13,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                FontWeight = FontWeights.Normal,
                Foreground = new SolidColorBrush(ColorScheme.GlobalWhite),
                Content = Message.InitialDate.Value.ToString("HH:mm"),
                Padding = new Thickness(0),
                Margin = new Thickness(0, 0, 10, 0)
            };

            DockPanel.SetDock(messageTypeImage, Dock.Left);
            titlePanel.Children.Add(messageTypeImage);

            DockPanel.SetDock(dateLabel, Dock.Right);
            titlePanel.Children.Add(dateLabel);

            DockPanel.SetDock(authorLabel, Dock.Left);
            titlePanel.Children.Add(authorLabel);

            titleBorder.Child = titlePanel;

            messageMainDock.Children.Add(titleBorder);
            #endregion

            #region attachmentBorder
            Border attachmentBorder = new Border()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush(ColorScheme.GlobalWhite),
                CornerRadius = new CornerRadius(0, 0, 10, 10),
                BorderBrush = new SolidColorBrush(ColorScheme.MenuDarker),
                BorderThickness = new Thickness(1, 0, 1, 1)
            };

            DockPanel innerDock = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                LastChildFill = true
            };

            if (message.Attachments.Count > 0)
            {
                ConversationMessageAttachmentContainer attachmentPanel =
                    new ConversationMessageAttachmentContainer(message.Attachments);

                attachmentPanel.RenameFile += AttachmentPanel_RenameFile;
                attachmentPanel.DownloadFile += AttachmentPanel_DownloadFile;

                DockPanel.SetDock(attachmentPanel, Dock.Bottom);
                innerDock.Children.Add(attachmentPanel);
            }

            attachmentBorder.Child = innerDock;

            DockPanel.SetDock(titleBorder, Dock.Top);
            messageMainDock.Children.Add(attachmentBorder);
            #endregion

            if (message is ConversationEmailMessageModel)
            {
                Image messageContentImage = new Image()
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(10),
                };

                ConversationEmailMessageModel emailMessage = (ConversationEmailMessageModel) message;

                Dispatcher.Invoke(delegate()
                {
                    if (emailMessage.MessageContent != "")
                        messageContentImage.Source = message.PreviewImage;
                });

                innerDock.Children.Add(messageContentImage);

                authorLabel.Content = message.Author.FirstName + " " + message.Author.LastName;

                messageTypeImage.Source = ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/inbox.png"));
            }
            else if (message is ConversationPhoneMessageModel)
            {
                ConversationPhoneMessageModel phoneMessage = (ConversationPhoneMessageModel)message;

                if (!phoneMessage.CallAnswered)
                {
                    Image notAnsweredImage = new Image()
                    {
                        Height = 25,
                        Margin = new Thickness(0, 10, 0, 0),
                        Source =
                            ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/call_unanswered.png"))
                    };

                    RenderOptions.SetBitmapScalingMode(notAnsweredImage, BitmapScalingMode.HighQuality);

                    DockPanel.SetDock(notAnsweredImage, Dock.Top);
                    innerDock.Children.Add(notAnsweredImage);

                    Label notAnsweredLabel = new Label()
                    {
                        FontSize = 11,
                        VerticalContentAlignment = VerticalAlignment.Top,
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        Foreground = new SolidColorBrush(ColorScheme.GlobalBlue),
                        Padding = new Thickness(0, 5, 0, 5),
                        Content = "Połączenie nieodebrane"
                    };

                    DockPanel.SetDock(notAnsweredLabel, Dock.Top);
                    innerDock.Children.Add(notAnsweredLabel);
                }

                Image arrowImage = new Image()
                {
                    Height = 13,
                    Width = 15,
                    Margin = new Thickness(5, 7, 5, 5),
                    VerticalAlignment = VerticalAlignment.Center,
                    Source =
                            ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/arrow_right_photo.png"))
                };

                DockPanel.SetDock(arrowImage, Dock.Left);
                titlePanel.Children.Add(arrowImage);

                Label recipientLabel = new Label()
                {
                    Height = 25,
                    FontSize = 13,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    FontWeight = FontWeights.Medium,
                    Foreground = new SolidColorBrush(ColorScheme.GlobalWhite),
                    Padding = new Thickness(0),
                    Content = phoneMessage.Recipient.FullName
                };

                DockPanel.SetDock(recipientLabel, Dock.Left);
                titlePanel.Children.Add(recipientLabel);

                Image messageContentImage = new Image()
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(10),
                };

                Dispatcher.Invoke(delegate ()
                {
                    if (phoneMessage.CallDescription != "")
                        messageContentImage.Source = message.PreviewImage;
                });

                innerDock.Children.Add(messageContentImage);

                authorLabel.Content = message.Author.FirstName + " " + message.Author.LastName;

                messageTypeImage.Source = ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/phone.png"));
            }

            #region MessageWedge
            ConversationMessageWedge messageWedge = new ConversationMessageWedge()
            {
                Height = 15,
                Width = 12,
                Margin = new Thickness(0, 0, 0, 6),
                VerticalAlignment = VerticalAlignment.Bottom,
                Fill = leadColor,
                SnapsToDevicePixels = false,
            };

            RenderOptions.SetEdgeMode(messageWedge, EdgeMode.Unspecified);

            if (Message.Received)
            {
                HorizontalAlignment = HorizontalAlignment.Left;
                TransformGroup transformGroup = new TransformGroup();
                ScaleTransform trans = new ScaleTransform(-1, 1);
                transformGroup.Children.Add(trans);
                TranslateTransform trans2 = new TranslateTransform();
                trans2.X = 12;
                transformGroup.Children.Add(trans2);
                messageWedge.RenderTransform = transformGroup;
                DockPanel.SetDock(messageWedge, Dock.Left);
                DockPanel.SetDock(outerDock, Dock.Left);
            }
            else
            {
                HorizontalAlignment = HorizontalAlignment.Right;
                DockPanel.SetDock(messageWedge, Dock.Right);
                DockPanel.SetDock(outerDock, Dock.Right);
            }

            outerDock.Children.Add(messageWedge);
            #endregion

            outerDock.Children.Add(messageMainDock);
            Children.Add(outerDock);

            ContextMenu contextMenu = new ContextMenu();

            MenuItem detailsItem = new MenuItem()
            {
                Header = "Pokaż szczegóły",
                Icon =
                    new Image()
                    {
                        Source =
                            ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/info_context.png"))
                    }
            };
            detailsItem.Click += DetailsItem_Click;
            contextMenu.Items.Add(detailsItem);

            if (Message.Attachments.Count > 0)
            {
                contextMenu.Items.Add(new Separator());

                MenuItem downloadAttachmentsItem = new MenuItem()
                {
                    Header =  Message.Attachments.Count == 1 ? "Pobierz załącznik" : "Pobierz załączniki",
                    Icon =
                        new Image()
                        {
                            Source =
                                ImageHelper.UriToImageSource(
                                    new Uri(@"pack://application:,,,/resources/download_context.png"))
                        }
                };
                downloadAttachmentsItem.Click += DownloadAttachmentsItem_Click;
                contextMenu.Items.Add(downloadAttachmentsItem);
            }

            ContextMenu = contextMenu;
        }

        private void ConversationMessageListItem_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ShowDetails?.Invoke(this, EventArgs.Empty);
            }
        }

        private void DetailsItem_Click(object sender, RoutedEventArgs e)
        {
            ShowDetails?.Invoke(this, EventArgs.Empty);
        }

        private void AttachmentPanel_RenameFile(object sender, EventArgs e)
        {
            RenameFile?.Invoke(sender, e);
        }

        private void AttachmentPanel_DownloadFile(object sender, EventArgs e)
        {
            DownloadFile?.Invoke(sender, e);
        }

        private void DownloadAttachmentsItem_Click(object sender, RoutedEventArgs e)
        {
            DownloadAllAttachments?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }

    public class ConversationMessageAttachmentContainer : Border
    {
        private StackPanel _attachmentStack;
        private Image _expandImage;
        private Label _attachmentCountLabel;
        private bool _expanded;

        public event EventHandler RenameFile;
        public event EventHandler DownloadFile;

        public List<FileModel> Files { get; private set; }

        public bool Expanded
        {
            get { return _expanded; }
            private set
            {
                _expanded = value;

                if (Expanded)
                {
                    DoubleAnimation slideDownAnimation = new DoubleAnimation()
                    {
                        From = ActualHeight,
                        To = 105,
                        Duration = TimeSpan.FromMilliseconds(100)
                    };

                    BeginAnimation(HeightProperty, slideDownAnimation);

                    _expandImage.Source =
                        ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/arrow_up_blue.png"));
                }
                else
                {
                    DoubleAnimation slideUpAnimation = new DoubleAnimation()
                    {
                        From = ActualHeight,
                        To = 20,
                        Duration = TimeSpan.FromMilliseconds(100)
                    };

                    BeginAnimation(HeightProperty, slideUpAnimation);

                    _expandImage.Source =
                        ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/arrow_down_blue.png"));
                }
            }
        }

        public ConversationMessageAttachmentContainer(List<FileModel> files)
        {
            try
            {
                Files = files;

                Height = 20;
                HorizontalAlignment = HorizontalAlignment.Stretch;
                Background = new SolidColorBrush(ColorScheme.MenuDarker);
                CornerRadius = new CornerRadius(0, 0, 10, 10);

                DockPanel innerDock = new DockPanel()
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };

                DockPanel upperDock = new DockPanel()
                {
                    Height = 20,
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                };

                _attachmentCountLabel = new Label()
                {
                    Margin = new Thickness(15, 0, 0, 0),
                    Content = Files.Count + " załączniki",
                    Padding = new Thickness(0),
                    FontSize = 11,
                    Foreground = new SolidColorBrush(ColorScheme.GlobalBlue),
                    VerticalAlignment = VerticalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center
                };

                DockPanel.SetDock(_attachmentCountLabel, Dock.Left);
                upperDock.Children.Add(_attachmentCountLabel);

                _expandImage = new Image()
                {
                    Height = 6,
                    Margin = new Thickness(7),
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    Source =
                        ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/arrow_down_blue.png"))
                };

                _expandImage.PreviewMouseLeftButtonDown += _expandImage_PreviewMouseLeftButtonDown;

                RenderOptions.SetBitmapScalingMode(_expandImage, BitmapScalingMode.HighQuality);

                DockPanel.SetDock(_expandImage, Dock.Right);
                upperDock.Children.Add(_expandImage);

                _attachmentStack = new StackPanel()
                {
                    VerticalAlignment = VerticalAlignment.Stretch,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    Margin = new Thickness(3),
                    Orientation = Orientation.Horizontal
                };

                foreach (FileModel file in Files)
                {
                    FileListItem fileItem = new FileListItem(file);

                    fileItem.RenameFile += FileItem_RenameFile;
                    fileItem.DownloadFile += FileItem_DownloadFile;

                    _attachmentStack.Children.Add(fileItem);
                }

                DockPanel.SetDock(upperDock, Dock.Top);
                innerDock.Children.Add(upperDock);

                innerDock.Children.Add(_attachmentStack);

                this.Child = innerDock;
            }
            catch (Exception ex)
            {
                
            }
        }

        private void FileItem_DownloadFile(object sender, EventArgs e)
        {
            DownloadFile?.Invoke(sender, e);
        }

        private void FileItem_RenameFile(object sender, EventArgs e)
        {
            RenameFile?.Invoke(sender, e);
        }

        private void _expandImage_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _expandImage.PreviewMouseLeftButtonUp += _expandImage_PreviewMouseLeftButtonUp;
        }

        private void _expandImage_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Expanded = !Expanded;
            _expandImage.PreviewMouseLeftButtonUp -= _expandImage_PreviewMouseLeftButtonUp;
        }

        public void AddFile(FileModel file)
        {
            _attachmentStack.Children.Add(new FileListItem(file));
        }

        public void AddFiles(List<FileModel> files)
        {
            foreach (FileModel file in Files)
            {
                _attachmentStack.Children.Add(new FileListItem(file));
            }
        }
    }

    public class ConversationMessageListDateItem : DockPanel
    {
        #region Constructors
        public ConversationMessageListDateItem(string text)
        {
            VerticalAlignment = VerticalAlignment.Stretch;
            HorizontalAlignment = HorizontalAlignment.Stretch;

            Border datePanel = new Border()
            {
                Height = 30,
                CornerRadius = new CornerRadius(15),
                Background = new SolidColorBrush(ColorScheme.GlobalBlue),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(10),
                Padding = new Thickness(10, 0, 10, 0)
            };

            Label dateLabel = new Label()
            {
                FontSize = 13,
                Content = text,
                Foreground = new SolidColorBrush(ColorScheme.GlobalWhite),
                HorizontalAlignment = HorizontalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Center
            };

            datePanel.Child = dateLabel;

            Children.Add(datePanel);
        }
        #endregion
    }
    #endregion

    public class NewConversationMessagePanel:Border
    {
        private DockPanel _innerDock;

        private Image _settingsImage;
        public NewConversationMessagePanel()
        {
            BorderThickness = new Thickness(0, 0, 0, 0);
            Height = 60;
            BorderBrush = new SolidColorBrush(ColorScheme.GlobalBlue);
            Background = new SolidColorBrush(ColorScheme.MenuDarker);

            HorizontalAlignment = HorizontalAlignment.Stretch;

            Effect = new DropShadowEffect
            {
                Direction = 45,
                ShadowDepth = 1.5,
                Opacity = 0.3,
                BlurRadius = 6
            };

            _innerDock = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                LastChildFill = false
            };

            Label addLabel = new Label()
            {
                FontSize = 17,
                Foreground = new SolidColorBrush(ColorScheme.GlobalBlue),
                FontWeight = FontWeights.Medium,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Content = "Dodaj:",
                Padding = new Thickness(10,0,10,0)
            };

            _innerDock.Children.Add(addLabel);

            Child = _innerDock;
        }

        public void AddButton(ImageSource image, ImageSource hoverImage, string toolTipText)
        {
            NewConversationMessagePanelButton button = new NewConversationMessagePanelButton(image, hoverImage, toolTipText);
            DockPanel.SetDock(button, Dock.Left);
            _innerDock.Children.Add(button);
        }

        public void AddButton(NewConversationMessagePanelButton button)
        {
            DockPanel.SetDock(button, Dock.Left);
            _innerDock.Children.Add(button);
        }
    }

    public class NewConversationMessagePanelButton:Grid
    {
        private ImageSource _iconImage;
        private ImageSource _iconHoverImage;

        private Image _image;

        public event EventHandler Click;
        public NewConversationMessagePanelButton(ImageSource image, ImageSource hoverImage, string toolTipText)
        {
            _iconImage = image;
            _iconHoverImage = hoverImage;
            Height = 50;
            Background = new SolidColorBrush(ColorScheme.MenuDarker);

            _image = new Image()
            {
                Source = image,
                Height = 40,
                Margin = new Thickness(5)
            };

            RenderOptions.SetBitmapScalingMode(_image, BitmapScalingMode.HighQuality);

            ToolTip = new Label()
            {
                FontSize = 13,
                Padding = new Thickness(10),
                Content = toolTipText
            };

            ToolTipService.SetInitialShowDelay(this, 2000);
            ToolTipService.SetBetweenShowDelay(this, 2000);

            Children.Add(_image);

            MouseEnter += NewConversationMessagePanelButton_MouseEnter;
            MouseLeave += NewConversationMessagePanelButton_MouseLeave;
            PreviewMouseLeftButtonDown += NewConversationMessagePanelButton_PreviewMouseLeftButtonDown;
        }

        private void NewConversationMessagePanelButton_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PreviewMouseLeftButtonUp += NewConversationMessagePanelButton_PreviewMouseLeftButtonUp;
        }

        private void NewConversationMessagePanelButton_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Click?.Invoke(this, EventArgs.Empty);
            PreviewMouseLeftButtonUp -= NewConversationMessagePanelButton_PreviewMouseLeftButtonUp;
        }

        private void NewConversationMessagePanelButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _image.Source = _iconImage;
        }

        private void NewConversationMessagePanelButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _image.Source = _iconHoverImage;
        }
    }

    #region ConversationMemberList
    class ConversationMemberList : DockPanel
    {
        #region Variables

        private Border _centerPanel;
        private StackPanel _leftPanel;
        private StackPanel _rightPanel;

        private DockPanel _memberListDock;
        private Image _expandImage;

        private bool _expanded = false;
        private double _memberListLastHeight = 90;
        private bool _disableAdding;
        private bool _disableToggle;

        public List<MemberListItem> Members = new List<MemberListItem>();

        public event EventHandler AddFilter;
        public event EventHandler RemoveFilter;
        public event EventHandler ClearFilter;
        public event EventHandler AddMember;
        public event EventHandler RemoveMember;
        #endregion

        #region Properties
        public bool Expanded
        {
            get
            {
                return _expanded;             
            }
            set
            {
                _expanded = value;
                DoubleAnimation animation = new DoubleAnimation();
                animation.From = _memberListDock.ActualHeight;
                animation.To = _memberListLastHeight;

                animation.Duration = new Duration(TimeSpan.FromMilliseconds(100));
                _memberListDock.BeginAnimation(HeightProperty, animation);
            }
        }

        public bool DisableAdding
        {
            get { return _disableAdding; }
            set
            {
                _disableAdding = value;
                if (DisableAdding)
                {
                    Grid parent = (Grid)_centerPanel.Parent;
                    parent.Children.Remove(_centerPanel);
                }                  
            }
        }

        public bool DisableToggle
        {
            
            get { return _disableToggle; }
            set
            {
                _disableToggle = value;
                if (DisableToggle)
                {
                    Expanded = true;
                    DockPanel parent = (DockPanel)_expandImage.Parent;
                    parent.Children.Remove(_expandImage);
                }
            }
        }

        #endregion

        #region Constructors
        public ConversationMemberList()
        {
            VerticalAlignment = VerticalAlignment.Stretch;
            HorizontalAlignment = HorizontalAlignment.Stretch;

            Effect = new DropShadowEffect
            {
                Direction = 315,
                ShadowDepth = 1.5,
                Opacity = 0.3,
                BlurRadius = 5
            };

            #region TitlePanel
            DockPanel _titlePanel = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
                Background = new SolidColorBrush(ColorScheme.MenuDarker),
                Height = 23
            };

            #region CenterLabel
            Label _centerLabel = new Label()
            {
                FontSize = 15,
                Foreground = new SolidColorBrush(ColorScheme.GlobalBlue),
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Content = "Uczestnicy",
                Padding = new Thickness(5, 3, 5, 3)
            };

            _titlePanel.Children.Add(_centerLabel);
            #endregion

            DockPanel.SetDock(_titlePanel, Dock.Top);
            Children.Add(_titlePanel);

            #endregion

            #region LowerPanel

            DockPanel lowerPanel = new DockPanel()
            {
                Height = 17,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(ColorScheme.MenuDarker)
            };

            _expandImage = new Image()
            {
                Height = 7,
                Margin = new Thickness(4),
                Source = ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/arrow_down_darker.png")),
                HorizontalAlignment = HorizontalAlignment.Center
            };

            lowerPanel.Children.Add(_expandImage);

            DockPanel.SetDock(lowerPanel, Dock.Bottom);

            lowerPanel.PreviewMouseLeftButtonDown += LowerPanel_MouseDown;
            lowerPanel.MouseEnter += LowerPanel_MouseEnter;
            lowerPanel.MouseLeave += LowerPanel_MouseLeave;

            Children.Add(lowerPanel);
            #endregion

            #region MemberListDock

            _memberListDock = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Height = 0
            };

            #region MemberListTopDock
            DockPanel memberListTopDock = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(ColorScheme.MenuDarker)
            };

            #region LeftLabel

            Label leftLabel = new Label()
            {
                FontSize = 13,
                Padding = new Thickness(15,3,5,3),
                Content = "Osoby kontaktowe",
                Foreground = new SolidColorBrush(ColorScheme.GlobalBlue),
                HorizontalAlignment = HorizontalAlignment.Left,
                HorizontalContentAlignment = HorizontalAlignment.Left
            };

            DockPanel.SetDock(leftLabel, Dock.Left);
            memberListTopDock.Children.Add(leftLabel);
            #endregion

            #region RightLabel
            Label rightLabel = new Label()
            {
                FontSize = 13,
                Padding = new Thickness(5, 3, 15, 3),
                Content = "Pracownicy",
                Foreground = new SolidColorBrush(ColorScheme.GlobalBlue),
                HorizontalAlignment = HorizontalAlignment.Right,
                HorizontalContentAlignment = HorizontalAlignment.Right
            };

            DockPanel.SetDock(rightLabel, Dock.Right);
            memberListTopDock.Children.Add(rightLabel);
            #endregion

            DockPanel.SetDock(memberListTopDock, Dock.Top);
            _memberListDock.Children.Add(memberListTopDock);

            #endregion

            #region MemberList
            Grid memberList = new Grid()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Background = new SolidColorBrush(ColorScheme.MenuDarker),
            };

            ColumnDefinition leftColumn = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star)};
            ColumnDefinition centerColumn = new ColumnDefinition() { Width = new GridLength(80)};
            ColumnDefinition rightColumn = new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) };

            memberList.ColumnDefinitions.Add(leftColumn);
            memberList.ColumnDefinitions.Add(centerColumn);
            memberList.ColumnDefinitions.Add(rightColumn);

            #region LeftPanel

            ScrollViewer leftScrollViewer = new ScrollViewer()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden
            };

            _leftPanel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Orientation = Orientation.Horizontal,
            };

            leftScrollViewer.Content = _leftPanel;

            Grid.SetColumn(leftScrollViewer, 0);
            Grid.SetRow(leftScrollViewer, 0);

            memberList.Children.Add(leftScrollViewer);

            #endregion

            #region CenterPanel

            _centerPanel = new Border()
            {
                Height = 30,
                Width = 30,
                Margin = new Thickness(10,5,10,5),
                CornerRadius = new CornerRadius(15),
                Background = new SolidColorBrush(ColorScheme.GlobalBlue),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            #region PlusImage
            Image plusImage = new Image()
            {
                Source = ImageHelper.UriToImageSource(new Uri("pack://application:,,,/resources/plus_gray.png")),
                Width = 10,
                Height = 10,
                Margin = new Thickness(10),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            _centerPanel.Child = plusImage;
            #endregion

            _centerPanel.PreviewMouseLeftButtonDown += _centerPanel_PreviewMouseLeftButtonDown;

            Grid.SetColumn(_centerPanel, 1);
            Grid.SetRow(_centerPanel, 0);

            memberList.Children.Add(_centerPanel);
            #endregion

            #region RightPanel

            ScrollViewer rightScrollViewer = new ScrollViewer()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Hidden
            };

            _rightPanel = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                Orientation = Orientation.Horizontal,
            };

            rightScrollViewer.Content = _rightPanel;

            Grid.SetColumn(rightScrollViewer, 2);
            Grid.SetRow(rightScrollViewer, 0);

            memberList.Children.Add(rightScrollViewer);
            #endregion

            _memberListDock.Children.Add(memberList);
            #endregion

            #endregion

            DockPanel.SetDock(_memberListDock, Dock.Top);
            Children.Add(_memberListDock);
            }

        #endregion

        #region Events
        private void LowerPanel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _expandImage.Source =
                ImageHelper.UriToImageSource(new Uri(_expanded ? "pack://application:,,,/resources/arrow_up_darker.png" : "pack://application:,,,/resources/arrow_down_darker.png"));
        }

        private void LowerPanel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _expandImage.Source =
                ImageHelper.UriToImageSource(new Uri(_expanded ? "pack://application:,,,/resources/arrow_up_blue.png" : "pack://application:,,,/resources/arrow_down_blue.png"));
        }

        private void LowerPanel_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DockPanel panel = (DockPanel)sender;
            panel.PreviewMouseLeftButtonUp += Panel_PreviewMouseLeftButtonUp;
        }

        private void Panel_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            DockPanel panel = (DockPanel)sender;
            Expanded = !Expanded;
            _memberListLastHeight = _memberListDock.ActualHeight;
            panel.PreviewMouseLeftButtonUp -= Panel_PreviewMouseLeftButtonUp;
        }

        private void _centerPanel_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _centerPanel.PreviewMouseLeftButtonUp += _centerPanel_PreviewMouseLeftButtonUp;
        }

        private void _centerPanel_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            AddMember?.Invoke(this, EventArgs.Empty);
            _centerPanel.PreviewMouseLeftButtonUp -= _centerPanel_PreviewMouseLeftButtonUp;
        }

        private void PersonItem_RemoveFilter(object sender, EventArgs e)
        {
            RemoveFilter?.Invoke(sender, e);
        }

        private void PersonItem_AddFilter(object sender, EventArgs e)
        {
            AddFilter?.Invoke(sender, e);
        }

        private void PersonItem_ClearFilter(object sender, EventArgs e)
        {
            ClearFilter?.Invoke(sender, e);
        }

        private void PersonItem_RemoveMember(object sender, EventArgs e)
        {
            RemoveMember?.Invoke(sender, e);
        }
        #endregion

        #region Methods
        public void AddMemberToList(PersonModel person, Color color, bool showContextMenu = true)
        {
            MemberListItem personItem = new MemberListItem(person, color, showContextMenu);

            Members.Add(personItem);

            if (!person.IsInternalUser)
                _leftPanel.Children.Add(personItem);
            else
                _rightPanel.Children.Add(personItem);

            personItem.AddFilter += PersonItem_AddFilter;
            personItem.RemoveFilter += PersonItem_RemoveFilter;
            personItem.ClearFilter += PersonItem_ClearFilter;
            personItem.RemoveMember += PersonItem_RemoveMember;
        }

        public void ClearMembers()
        {
            _leftPanel.Children.Clear();
            _rightPanel.Children.Clear();
            Members.Clear();
        }
        #endregion

        public void RemoveMemberFromList(PersonModel person)
        {
            MemberListItem memberItem = Members.Find(obj => obj.Person == person);
            if (memberItem.Person.IsInternalUser)
                _rightPanel.Children.Remove(memberItem);
            else
                _leftPanel.Children.Remove(memberItem);
            Members.Remove(memberItem);
        }
    }

    public class MemberListItem : DockPanel
    {
        #region Variables
        private Border _imageBorder;
        private Image _image;
        private TextBlock _nameBlock;
        private bool _filtered;
        private bool _filtering;

        private MenuItem _addFilterItem;
        private MenuItem _removeFilterItem;
        private MenuItem _clearFilterItem;

        public event EventHandler AddFilter;
        public event EventHandler RemoveFilter;
        public event EventHandler ClearFilter;
        public event EventHandler RemoveMember;
        #endregion

        #region Properties

        public bool Filtered
        {
            get { return _filtered; }
            set
            {
                _filtered = value;
                if (Filtered)
                {
                    _imageBorder.BorderThickness = new Thickness(2.5);
                    _imageBorder.BorderBrush = new SolidColorBrush(ColorScheme.GlobalDarkRed);
                    _image.Height = 25;
                    _image.Width = 25;
                    _nameBlock.Foreground = new SolidColorBrush(ColorScheme.GlobalDarkRed);

                    _removeFilterItem.Visibility = Visibility.Visible;
                    _addFilterItem.Visibility = Visibility.Collapsed;
                    _clearFilterItem.Visibility = Visibility.Visible;
                }
                else
                {
                    _imageBorder.BorderThickness = new Thickness(0);
                    _image.Height = 30;
                    _image.Width = 30;
                    _nameBlock.Foreground = new SolidColorBrush(Colors.Black);

                    _removeFilterItem.Visibility = Visibility.Collapsed;
                    _addFilterItem.Visibility = Visibility.Visible;
                    _clearFilterItem.Visibility = Visibility.Collapsed;
                }
            }
        }

        public bool Filtering
        {
            get { return _filtering; }
            set
            {
                _filtering = value;
                if (Filtering)
                    _clearFilterItem.Visibility = Visibility.Visible;
                else
                    _clearFilterItem.Visibility = Visibility.Collapsed;
            }
        }

        public PersonModel Person { get; set; }
        #endregion

        #region Constructors

        public MemberListItem(PersonModel person, Color color, bool showContextMenu)
        {
            Person = person;

            Width = 65;
            MouseEnter += MemberListItem_MouseEnter;
            MouseLeave += MemberListItem_MouseLeave;
            Background = new SolidColorBrush(ColorScheme.MenuDarker);


            _imageBorder = new Border()
            {
                Height = 30,
                Width = 30,
                CornerRadius = new CornerRadius(15),
                Background = new SolidColorBrush(color),
                VerticalAlignment = VerticalAlignment.Top,
                BorderBrush = new SolidColorBrush(ColorScheme.GlobalBlue),
                BorderThickness = new Thickness(0),
                Margin = new Thickness(5, 5, 5, 1)
            };

            #region Image

            _image = new Image()
            {
                Width = 30,
                Height = 30,
                Source =
                    ImageHelper.UriToImageSource(
                        new Uri(person.Gender == Gender.Female
                            ? "pack://application:,,,/resources/person_female.png"
                            : "pack://application:,,,/resources/person_male.png"))
            };

            RenderOptions.SetBitmapScalingMode(_image, BitmapScalingMode.HighQuality);

            _imageBorder.Child = _image;

            #endregion

            DockPanel.SetDock(_imageBorder, Dock.Top);
            Children.Add(_imageBorder);

            #region Name

            _nameBlock = new TextBlock()
            {
                FontSize = 11,
                TextWrapping = TextWrapping.WrapWithOverflow,
                TextTrimming = TextTrimming.CharacterEllipsis,
                Width = 65,
                Padding = new Thickness(3, 2, 2, 2),
                Text = person.FullName,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top
            };

            Children.Add(_nameBlock);

            #endregion

            if (showContextMenu)
            {
                ContextMenu contextMenu = new ContextMenu();

                MenuItem removeMemberItem = new MenuItem()
                {
                    Header = "Usuń z konwersacji",
                    Icon =
                        new Image()
                        {
                            Source =
                                ImageHelper.UriToImageSource(
                                    new Uri(@"pack://application:,,,/resources/remove_context.png"))
                        }
                };
                removeMemberItem.Click += RemoveMemberItem_Click;
                contextMenu.Items.Add(removeMemberItem);

                ContextMenu = contextMenu;
            }
        }

        #endregion

        #region Events

        private void MemberListItem_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (Filtered)
            {
                _imageBorder.BorderThickness = new Thickness(2.5);
                _imageBorder.BorderBrush = new SolidColorBrush(ColorScheme.GlobalDarkRed);
                _image.Height = 25;
                _image.Width = 25;
                _nameBlock.Foreground = new SolidColorBrush(ColorScheme.GlobalDarkRed);
            }
            else
            {
                _imageBorder.BorderThickness = new Thickness(0);
                _imageBorder.BorderBrush = new SolidColorBrush(ColorScheme.GlobalBlue);
                _image.Height = 30;
                _image.Width = 30;
                _nameBlock.Foreground = new SolidColorBrush(Colors.Black);
            }
        }

        private void MemberListItem_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _imageBorder.BorderThickness = new Thickness(1.5);
            _imageBorder.BorderBrush = new SolidColorBrush(ColorScheme.GlobalBlue);
            _image.Height = 27;
            _image.Width = 27;
            _nameBlock.Foreground = new SolidColorBrush(ColorScheme.GlobalBlue);
        }

        private void _clearFilterItem_Click(object sender, RoutedEventArgs e)
        {
            ClearFilter?.Invoke(this, EventArgs.Empty);
        }

        private void AddFilterItem_Click(object sender, RoutedEventArgs e)
        {
            AddFilter?.Invoke(this, EventArgs.Empty);
        }

        private void _removeFilterItem_Click(object sender, RoutedEventArgs e)
        {
            RemoveFilter?.Invoke(this, EventArgs.Empty);
        }

        private void RemoveMemberItem_Click(object sender, RoutedEventArgs e)
        {
            RemoveMember?.Invoke(this, EventArgs.Empty);
        }
        #endregion
    }
    #endregion
    #endregion

    internal class ConversationMessageWedge : Shape
    {
        #region Properties
        protected override Geometry DefiningGeometry
        {
            get { return CreateGeometry(); }
        }
        #endregion

        #region Methods
        private Geometry CreateGeometry()
        {
            StreamGeometry geometry = new StreamGeometry();

            using (StreamGeometryContext context = geometry.Open())
            {
                context.BeginFigure(new System.Windows.Point(0, 12), true, true);

                context.ArcTo(new System.Windows.Point(12, 8), new System.Windows.Size(15, 5), 0.0, false,
                    SweepDirection.Counterclockwise, true, true);
                context.ArcTo(new System.Windows.Point(0, 0), new System.Windows.Size(15, 15), 0.0, false,
                    SweepDirection.Clockwise, true, true);
                context.LineTo(new System.Windows.Point(0, 15), true, true);
            }

            return geometry;
        }
        #endregion
    }

    public class CustomDetailsTable : Grid
    {
        private Dictionary<string, string> Items { get; set; } = new Dictionary<string, string>();
        private int _rowHeight = 25;
        private int _columnWidth = 150;


        public CustomDetailsTable()
        {
            ColumnDefinitions.Add(new ColumnDefinition() {Width = new GridLength(_columnWidth) });
            ColumnDefinitions.Add(new ColumnDefinition());
        }

        public void AddItem(string key, string value)
        {
            Label keyLabel = new Label()
            {
                Padding = new Thickness(0, 0, 15, 0),
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Right,
                Foreground = new SolidColorBrush(ColorScheme.GlobalBlue),
                FontSize = 15,
                FontWeight = FontWeights.Medium,
                Content = key
            };

            TextBlock valueBlock = new TextBlock()
            {
                Padding = new Thickness(5, 2, 0, 0),
                TextAlignment = TextAlignment.Left,
                LineHeight = _rowHeight,
                FontSize = 15,
                Text = value
            };

            RowDefinitions.Add(new RowDefinition() {Height = new GridLength(25)});

            Grid.SetColumn(keyLabel, 0);
            Grid.SetRow(keyLabel, RowDefinitions.Count - 1);


            Grid.SetColumn(valueBlock, 1);
            Grid.SetRow(valueBlock, RowDefinitions.Count - 1);

            Children.Add(keyLabel);
            Children.Add(valueBlock);
        }
    }

    public class SelectedConversationChangedEventArgs : EventArgs
    {
        public ConversationModel Conversation { get; set; }
    }
}
