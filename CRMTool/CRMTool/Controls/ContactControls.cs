using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Licencjat_new.CustomClasses;

namespace Licencjat_new.Controls
{
    #region FilterContainer
    class ContactFilterContainer : DockPanel
    {
        public ContactFilterContainer()
        {
            Width = double.NaN;
            Height = double.NaN;
            Background = new SolidColorBrush(ColorScheme.MenuLight);
        }
    }

    public class ContactSearchBox : Border
    {
        #region Variables
        private readonly TextBox _searchBox;
        private readonly Timer _searchTimer;

        public string SearchPhrase = "";

        public event EventHandler SearchPhraseChanged;
        #endregion

        #region Constructors
        public ContactSearchBox()
        {
            VerticalAlignment = VerticalAlignment.Top;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Height = 30;
            Background = new SolidColorBrush(Colors.White);
            BorderBrush = new SolidColorBrush(ColorScheme.MenuDarker);
            BorderThickness = new Thickness(1);
            CornerRadius = new CornerRadius(5);
            Margin = new Thickness(10);

            DockPanel innerDock = new DockPanel()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                LastChildFill = true,
            };

            Image icon = new Image()
            {
                Height = 15,
                Width = 15,
                Source = ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/search_off.png")),
                Margin = new Thickness(7.5)
            };

            DockPanel.SetDock(icon, Dock.Left);
            innerDock.Children.Add(icon);

            _searchBox = new TextBox()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Bottom,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                FontSize = 15,
                Text = "Wyszukaj",
                Foreground = new SolidColorBrush(ColorScheme.MenuDarker),
                Padding = new Thickness(0, 5, 0, 5),
                Margin = new Thickness(0, 0, 5, 0),
                BorderThickness = new Thickness(0)
            };

            _searchBox.GotFocus += SearchBox_GotFocus;
            _searchBox.LostFocus += SearchBox_LostFocus;
            _searchBox.TextChanged += _searchBox_TextChanged;

            _searchTimer = new Timer(500);
            _searchTimer.Elapsed += _searchTimer_Elapsed;

            innerDock.Children.Add(_searchBox);

            Child = innerDock;
        }
        #endregion

        #region Events
        private void _searchTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                Dispatcher.Invoke(() =>
                {
                    SearchPhrase = _searchBox.Text;
                    _searchTimer.Stop();

                    SearchPhraseChanged?.Invoke(this, EventArgs.Empty);
                });
            }
            catch (Exception ex)
            {
            }
        }

        private void _searchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_searchTimer.Enabled)
            {
                _searchTimer.Start();
                _searchTimer.Enabled = true;
            }
            else
            {
                _searchTimer.Stop();
                _searchTimer.Start();
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox searchBox = (TextBox)sender;

            if (searchBox.Text == String.Empty)
            {
                searchBox.Foreground = new SolidColorBrush(ColorScheme.MenuDarker);
                searchBox.Text = "Wyszukaj";
            }
        }

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox searchBox = (TextBox)sender;

            if (searchBox.Text == "Wyszukaj")
            {
                searchBox.Foreground = new SolidColorBrush(ColorScheme.GlobalBlue);
                searchBox.Clear();
            }
        }
        #endregion
    }
    #endregion

    #region MainContainer
        class ContactMainContainer :DockPanel
    {
        public ContactMainContainer()
        {
            Width = double.NaN;
            Height = double.NaN;
            Background = new SolidColorBrush(ColorScheme.MenuLight);
        }
    }

        #region ContactList

    #region ContactListItems

    public class ContactPersonListItem : Border
    {
        private int _height = 50;
        private DockPanel _innerPanel;
        private bool _selectionMode;
        private Image _selectImage;
        private bool _selected;

        public event EventHandler Click;
        public PersonModel Person { get; private set; }

        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;
                if (Selected)
                    _selectImage.Source =
                        ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/select_on.png"));
                else
                    _selectImage.Source =
                        ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/select_off.png"));
            }
        }

        public ContactPersonListItem(PersonModel person, bool selectionMode = false)
        {
            Person = person;
            _selectionMode = selectionMode;
            Redraw();

            MouseEnter += ContactPersonListItem_MouseEnter;
            MouseLeave += ContactPersonListItem_MouseLeave;
            PreviewMouseLeftButtonDown += ContactPersonListItem_PreviewMouseLeftButtonDown;

            if (Person.Company != null)
            {
                Person.Company.DataChanged += (s, ea) =>
                {
                    Redraw();
                };
            }

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
            //renameItem.Click += RenameItem_Click;
            contextMenu.Items.Add(detailsItem);

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
            //renameItem.Click += RenameItem_Click;
            contextMenu.Items.Add(removeItem);

            ContextMenu = contextMenu;
        }

        private void ContactPersonListItem_PreviewMouseLeftButtonDown(object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            PreviewMouseLeftButtonUp += ContactPersonListItem_PreviewMouseLeftButtonUp;
        }

        private void ContactPersonListItem_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Click?.Invoke(this, EventArgs.Empty);
            PreviewMouseLeftButtonUp -= ContactPersonListItem_PreviewMouseLeftButtonUp;
        }

        internal void Redraw()
        {
            Background = new SolidColorBrush(ColorScheme.GlobalWhite);
            Height = _height;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            BorderThickness = new Thickness(0, 0, 0, 1);
            BorderBrush = new SolidColorBrush(ColorScheme.MenuDarker);

            _innerPanel = new DockPanel()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            DockPanel leftPanel = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush(Colors.Transparent),
                LastChildFill = true
            };

            if (_selectionMode)
            {
                _selectImage = new Image()
                {
                    Height = 29,
                    Width = 29,
                    Margin = new Thickness(10),
                    Source = ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/select_off.png"))
                };

                RenderOptions.SetBitmapScalingMode(_selectImage, BitmapScalingMode.HighQuality);

                DockPanel.SetDock(_selectImage, Dock.Left);
                _innerPanel.Children.Add(_selectImage);
            }

            Label nameLabel = new Label()
            {
                Content = Person.FirstName + " " + Person.LastName,
                FontSize = 15,
                Padding = new Thickness(10, 6, 0, 0),
                Margin = new Thickness(0),
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(Colors.Black),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Left
            };

            DockPanel.SetDock(nameLabel, Dock.Top);
            leftPanel.Children.Add(nameLabel);

            Label companyLabel = new Label()
            {
                Content = Person.Company != null ? Person.Company.Name : "",
                FontSize = 13,
                Padding = new Thickness(10, 0, 0, 0),
                Margin = new Thickness(0),
                Foreground = new SolidColorBrush(ColorScheme.GlobalBlue),
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Top,
                HorizontalContentAlignment = HorizontalAlignment.Left
            };

            leftPanel.Children.Add(companyLabel);

            DockPanel.SetDock(leftPanel, Dock.Left);
            _innerPanel.Children.Add(leftPanel);

            List<object> detailsListObjects = new List<object>();
            if (Person.EmailAddresses.Count > 0)
            {
                if (Person.PhoneNumbers.Count > 0)
                {
                    detailsListObjects.Add(Person.EmailAddresses[0]);
                    detailsListObjects.Add(Person.PhoneNumbers[0]);
                }
                else
                {
                    detailsListObjects.Add(Person.EmailAddresses[0]);
                    if (Person.EmailAddresses.Count > 1)
                        detailsListObjects.Add(Person.EmailAddresses[1]);
                }
            }
            else
            {
                if (Person.PhoneNumbers.Count > 0)
                {
                    detailsListObjects.Add(Person.PhoneNumbers[0]);
                    if (Person.EmailAddresses.Count > 1)
                        detailsListObjects.Add(Person.PhoneNumbers[1]);
                }
            }

            StackPanel rightPanel = new StackPanel()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0, 3, 20, 3),
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Right,

            };

            if (detailsListObjects.Count > 0)
            {
                foreach (object item in detailsListObjects)
                {
                    DockPanel detailPanel = new DockPanel()
                    {
                        Height = 15,
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                    };

                    Image detailIcon = new Image()
                    {
                        Source = ImageHelper.UriToImageSource(new Uri(item.GetType().Name == "EmailAddressModel" ? @"pack://application:,,,/resources/mail2.png" : @"pack://application:,,,/resources/phone2.png")),
                        Height = 12,
                        Width = 12
                    };

                    //RenderOptions.SetBitmapScalingMode(detailIcon, BitmapScalingMode.HighQuality);

                    DockPanel.SetDock(detailIcon, Dock.Left);
                    detailPanel.Children.Add(detailIcon);

                    string contentString = "";
                    if (item.GetType().Name == "EmailAddressModel")
                    {
                        EmailAddressModel emailModel = (EmailAddressModel)item;
                        contentString = emailModel.Address;
                    }
                    else if (item.GetType().Name == "PhoneNumberModel")
                    {
                        PhoneNumberModel phoneModel = (PhoneNumberModel)item;
                        contentString = phoneModel.Number;
                    }

                    TextBlock detailLabel = new TextBlock()
                    {
                        Text = contentString,
                        FontSize = 11,
                        Padding = new Thickness(5, 0, 0, 0),
                        Margin = new Thickness(0),
                        Foreground = new SolidColorBrush(Colors.Black),
                        VerticalAlignment = VerticalAlignment.Stretch,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        LineHeight = 11,
                    };

                    detailPanel.Children.Add(detailLabel);

                    rightPanel.Children.Add(detailPanel);
                }

                int moreCount = Person.EmailAddresses.Count + Person.PhoneNumbers.Count - detailsListObjects.Count;

                if (moreCount > 0)
                {
                    string moreContent = "";

                    if (moreCount == 1)
                        moreContent = "inny";
                    else if (Convert.ToInt32(moreCount.ToString().First()) != 1 &&
                             Convert.ToInt32(moreCount.ToString().Last()) >= 2 &&
                             Convert.ToInt32(moreCount.ToString().Last()) <= 4)
                        moreContent = "inne";
                    else
                        moreContent = "innych";

                    Label moreLabel = new Label()
                    {
                        Content = "+ " + moreCount + " " + moreContent,
                        FontSize = 10,
                        Padding = new Thickness(0),
                        Margin = new Thickness(0),
                        Foreground = new SolidColorBrush(ColorScheme.GlobalBlue),
                        VerticalAlignment = VerticalAlignment.Stretch,
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalContentAlignment = VerticalAlignment.Top,
                        HorizontalContentAlignment = HorizontalAlignment.Left
                    };

                    rightPanel.Children.Add(moreLabel);
                }

                DockPanel.SetDock(rightPanel, Dock.Right);
                _innerPanel.Children.Add(rightPanel);
            }

            Child = _innerPanel;
        }

        private void ContactPersonListItem_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _innerPanel.Background = new SolidColorBrush(Colors.White);
        }

        private void ContactPersonListItem_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _innerPanel.Background = new SolidColorBrush(ColorScheme.MenuDarker);
        }

        public override string ToString()
        {
            return Person.FullName;
        }
    }

    class AlphabetElementListItem : Border
    {
        private int _height = 50;
        public string Element { get; private set;}

        public AlphabetElementListItem(string element)
        {
            Element = element;
            Redraw();
        }

        public override string ToString()
        {
            return Element;
        }

        internal void Redraw()
        {
            Background = new SolidColorBrush(ColorScheme.MenuLight);
            Height = _height;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            BorderThickness = new Thickness(0, 0, 0, 1);
            BorderBrush = new SolidColorBrush(ColorScheme.MenuDarker);

            DockPanel innerPanel = new DockPanel()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            TextBlock elementLabel = new TextBlock()
            {
                Text = Element.ToString(),
                FontSize = 40,
                Margin = new Thickness(0),
                Padding = new Thickness(20,0,0,0),
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(ColorScheme.GlobalBlue),
                LineHeight = 40,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            DockPanel.SetDock(elementLabel, Dock.Left);
            innerPanel.Children.Add(elementLabel);

            Child = innerPanel;
        }
    }

    class ContactCompanyListItem : Border
    {
        private int _height = 50;
        private DockPanel _innerPanel;
        public CompanyModel Company  { get; private set; }

        public event EventHandler DataChanged;
        public event EventHandler Rename;

        public ContactCompanyListItem(CompanyModel company)
        {
            Company = company;

            Company.DataChanged += Company_DataChanged;
            Redraw();

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
            //renameItem.Click += RenameItem_Click;
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
            //renameItem.Click += RenameItem_Click;
            contextMenu.Items.Add(removeItem);

            ContextMenu = contextMenu;
        }

        internal void Company_DataChanged(object sender, EventArgs e)
        {
            Redraw();
            DataChanged?.Invoke(this, EventArgs.Empty);
        }

        private void RenameItem_Click(object sender, RoutedEventArgs e)
        {
            Rename?.Invoke(this, EventArgs.Empty);
        }

        internal void Redraw()
        {
            Background = new SolidColorBrush(ColorScheme.MenuLight);
            Height = _height;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            BorderThickness = new Thickness(0, 0, 0, 1);
            BorderBrush = new SolidColorBrush(ColorScheme.MenuDarker);
            //MouseEnter += ContactPersonListItem_MouseEnter;
            //MouseLeave += ContactPersonListItem_MouseLeave;

            _innerPanel = new DockPanel()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            DockPanel leftPanel = new DockPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Stretch,
                Background = new SolidColorBrush(Colors.Transparent),
                LastChildFill = true
            };

            Label nameLabel = new Label()
            {
                Content = Company.Name,
                FontSize = 20,
                Padding = new Thickness(20, 6, 0, 0),
                Margin = new Thickness(0),
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(ColorScheme.GlobalBlue),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalContentAlignment = HorizontalAlignment.Left
            };

            DockPanel.SetDock(nameLabel, Dock.Top);
            leftPanel.Children.Add(nameLabel);

            DockPanel.SetDock(leftPanel, Dock.Left);
            _innerPanel.Children.Add(leftPanel);

            Child = _innerPanel;
        }

        private void ContactPersonListItem_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _innerPanel.Background = new SolidColorBrush(Colors.White);
        }

        private void ContactPersonListItem_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _innerPanel.Background = new SolidColorBrush(ColorScheme.MenuDarker);
        }

        public override string ToString()
        {
            return Company.Name;
        }
    }

    class ContactSearchDividerListItem : Border
    {
        private int _height = 40;
        private string _text = "";

        public ContactSearchDividerListItem(string text)
        {
            _text = text;
            Redraw();
        }

        internal void Redraw()
        {
            Background = new SolidColorBrush(ColorScheme.GlobalBlue);
            Height = _height;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Top;
            BorderThickness = new Thickness(0, 0, 0, 0);

            DockPanel innerPanel = new DockPanel()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            TextBlock elementLabel = new TextBlock()
            {
                Text = _text,
                FontSize = 20,
                Margin = new Thickness(0),
                Padding = new Thickness(20, 0, 0, 0),
                FontWeight = FontWeights.Medium,
                Foreground = new SolidColorBrush(ColorScheme.GlobalWhite),
                LineHeight = 20,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left
            };

            DockPanel.SetDock(elementLabel, Dock.Left);
            innerPanel.Children.Add(elementLabel);

            Child = innerPanel;
        }
    }
    #endregion
    #endregion

        #region AlphabetList
            public class AlphabetList : Grid
    {
        #region Variables
        List<string> _alphabet = "A,Ą,B,C,Ć,D,E,Ę,F,G,H,I,J,K,L,Ł,M,N,Ń,O,Ó,P,Q,R,S,Ś,T,U,V,W,X,Y,Z,Ź,Ż".Split(',').ToList();
        private List<string> _usedAlphabet = new List<string>();
        List<Label> _alphabetLabels = new List<Label>();

        public event EventHandler SelectedCharacterChanged;
        #endregion

        #region Properties
        public string SelectedCharacter { get; private set; }

        public List<string> Elements
        {
            get { return _usedAlphabet; }
            set
            {
                _usedAlphabet = value;

                Update();
            }
        }
        #endregion

        #region Constructors
        public AlphabetList()
        {
            VerticalAlignment = VerticalAlignment.Stretch;
            HorizontalAlignment = HorizontalAlignment.Stretch;

            ColumnDefinition column1 = new ColumnDefinition();
            column1.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinitions.Add(column1);

            ColumnDefinition column2 = new ColumnDefinition();
            column2.Width = new GridLength(1, GridUnitType.Star);
            ColumnDefinitions.Add(column2);

            for (int i = 0; i < _alphabet.Count; i++)
            {
                string character = _alphabet[i];
                Label characterLabel = new Label()
                {
                    Content = character,
                    Foreground = new SolidColorBrush(ColorScheme.MenuDarker),
                    FontSize = 18,
                    HorizontalContentAlignment = HorizontalAlignment.Center,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    Effect = null
                };

                if (i < 18)
                {
                    RowDefinition row = new RowDefinition();
                    row.Height = new GridLength(1, GridUnitType.Star);
                    RowDefinitions.Add(row);
                }

                Grid.SetColumn(characterLabel, i < 18 ? 0 : 1);
                Grid.SetRow(characterLabel, i < 18 ? i : i - 18);

                Children.Add(characterLabel);
                _alphabetLabels.Add(characterLabel);
            }
        }
        #endregion

        #region Events
        private void FoundLabel_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Label label = (Label)sender;

            SelectedCharacter = label.Content.ToString();
            e.Handled = true;

            SelectedCharacterChanged?.Invoke(this, null);

            label.PreviewMouseLeftButtonUp -= FoundLabel_PreviewMouseLeftButtonUp;
        }

        private void FoundLabel_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Label label = (Label)sender;
            label.PreviewMouseLeftButtonUp += FoundLabel_PreviewMouseLeftButtonUp;
        }

        private void FoundLabel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Label label = (Label)sender;
            label.Background = new SolidColorBrush(ColorScheme.MenuLight);
        }

        private void FoundLabel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Label label = (Label)sender;
            label.Background = new SolidColorBrush(ColorScheme.MenuDarker);
        }
        #endregion

        #region Methods
        private void Update()
        {
            foreach (Label element in _alphabetLabels)
            {
                element.Foreground = new SolidColorBrush(ColorScheme.MenuDarker);
            }

            foreach (string element in _usedAlphabet)
            {
                AddToUsedAlphabet(element);
            }
        }

        public void AddToUsedAlphabet(string character)
        {
            if(!_usedAlphabet.Contains(character))
                _usedAlphabet.Add(character);

            Label foundLabel = _alphabetLabels.Single(obj => obj.Content.ToString() == character);
            foundLabel.Foreground = new SolidColorBrush(ColorScheme.GlobalBlue);

            foundLabel.MouseEnter += FoundLabel_MouseEnter;
            foundLabel.MouseLeave += FoundLabel_MouseLeave;

            foundLabel.PreviewMouseLeftButtonDown += FoundLabel_PreviewMouseLeftButtonDown;
        }
        #endregion

    }
    #endregion

        #region TabControl
            public sealed class ContactTabControl:Border
    {

        #region Variables
        private StackPanel _innerStack;

        private ContactTabControlMode _defaultMode = ContactTabControlMode.Contacts;
        private ContactTabControlMode _selectedMode = ContactTabControlMode.Contacts;
        private bool _selectionMode = false;

        public event EventHandler SelectedModeChanged;
        #endregion

        #region Properties
        public List<ContactTabControlItem> Items { get; } = new List<ContactTabControlItem>();

        public ContactTabControlMode DefaultMode
        {
            get { return _defaultMode; }
            set
            {
                _defaultMode = value;
                Items.Single(obj => obj.ContactTabControlMode == _defaultMode).SetToggled(true);

                foreach (ContactTabControlItem loopItem in Items.Where(obj => obj.ContactTabControlMode != _defaultMode)
                    )
                {
                    loopItem.SetToggled(false);
                }

                _selectedMode = _defaultMode;
            }
        }

        public ContactTabControlMode SelectedMode
        {
            get { return _selectedMode; }
            set
            {
                _selectedMode = value;
                SelectedModeChanged?.Invoke(this, EventArgs.Empty);
            }
        }

                public bool SelectionMode
                {
                    get { return _selectionMode; }
                    set
                    {
                        _selectionMode = value;
                        if (SelectionMode)
                            RemoveItem(ContactTabControlMode.Companies);
                    }
        }
        #endregion

        #region Constructors
        public ContactTabControl()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Bottom;
            //Background = new SolidColorBrush(ColorScheme.MenuLight);
            BorderBrush = new SolidColorBrush(ColorScheme.MenuDark);
            BorderThickness = new Thickness(0, 0, 0, 0);
            Height = 40;

            _innerStack = new StackPanel()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Height = 40,
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0),
            };

            Child = _innerStack;

            AddItem("Kontakty", ContactTabControlMode.Contacts);
            AddItem("Firmy", ContactTabControlMode.Companies);
            AddItem("Wewnętrzne", ContactTabControlMode.InternalContacts);
            DefaultMode = ContactTabControlMode.Companies;
        }
        #endregion

        #region Events
        private void Item_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ContactTabControlItem item = (ContactTabControlItem)sender;
            item.PreviewMouseLeftButtonUp += Item_PreviewMouseLeftButtonUp;
        }

        private void Item_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ContactTabControlItem item = (ContactTabControlItem)sender;
            item.SetToggled(true);

            foreach (ContactTabControlItem loopItem in Items)
            {
                if (loopItem != item)
                    loopItem.SetToggled(false);
            }

            SelectedMode = item.ContactTabControlMode;

            item.PreviewMouseLeftButtonUp -= Item_PreviewMouseLeftButtonUp;
        }
        #endregion

        #region Methods
        public void AddItem(string text, ContactTabControlMode mode)
        {
            ContactTabControlItem item = new ContactTabControlItem(text) {ContactTabControlMode = mode};
            item.PreviewMouseLeftButtonDown += Item_PreviewMouseLeftButtonDown;
            Items.Add(item);
            _innerStack.Children.Add(item);
        }

                public void RemoveItem(ContactTabControlMode mode)
                {
                    ContactTabControlItem item = Items.Find(obj => obj.ContactTabControlMode == mode);
                    _innerStack.Children.Remove(item);
                    Items.Remove(item);
                }
        #endregion
    }

            public sealed class ContactTabControlItem : Border
    {
        #region Variables
        private bool _toggled;
        private readonly Label _textLabel;
        #endregion

        #region Properties
        public ContactTabControlMode ContactTabControlMode { get; internal set; }

        public bool Toggled
        {
            get { return _toggled; }
            private set
            {
                _toggled = value;
                if (_toggled)
                {
                    Height = 40;
                    Background = new SolidColorBrush(ColorScheme.MenuDarker);
                    _textLabel.Foreground = new SolidColorBrush(ColorScheme.GlobalBlue);
                }
                else
                {
                    Height = 30;
                    Background = new SolidColorBrush(ColorScheme.GlobalBlue);
                    _textLabel.Foreground = new SolidColorBrush(ColorScheme.GlobalWhite);
                }
            }
        }
        #endregion

        #region Constructors
        public ContactTabControlItem(string text)
        {
            Margin = new Thickness(0);
            VerticalAlignment = VerticalAlignment.Bottom;
            HorizontalAlignment = HorizontalAlignment.Left;

            _textLabel = new Label()
            {
                Content = text,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Bottom,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Bottom,
                Padding= new Thickness(10,0,10,5),
                FontSize = 15,
                Margin = new Thickness(0)
            };

            Child = _textLabel;
        }
        #endregion

        #region Methods
        public void SetToggled(bool toggled)
        {
            Toggled = toggled;   
        }
        #endregion
    }
    #endregion

    public class ContactList : ScrollViewer
    {
        private bool _selectionMode;
        private bool _multipleSelection;

        public List<ContactPersonListItem> SelectedPersons = new List<ContactPersonListItem>();

        private StackPanel _internalContactsStack;
        private StackPanel _externalContactsStack;
        private StackPanel _companiesStack;
        private StackPanel _externalGroupedContactsStack;
        private StackPanel _searchStack;

        private StackPanel _currentStack;
        private List<AlphabetElementListItem> _currentAlphabetElementList;

        private ContactTabControl _boundTabControl;
        private AlphabetList _boundAlphabetist;
        private ToolBarToggleButton _boundToggleButton;
        private ContactSearchBox _boundSearchBox;

        public event EventHandler SelectedItemsChanged;

        public List<PersonModel> Persons { get; private set; }
        public List<PersonModel> InternalContacts { get; private set; }
        public List<PersonModel> ExternalContacts { get; private set; }
        public List<CompanyModel> Companies { get; private set; }


        private List<PersonModel> _usedInternalContacts = new List<PersonModel>();
        private List<PersonModel> _usedExternalContacts = new List<PersonModel>();
        private List<CompanyModel> _usedCompanies = new List<CompanyModel>();

        public event EventHandler RenameCompany;

        //public List<PersonModel> BlockedPersons { get; private set; }

        private List<string> InternalContactsUsedAlphabet { get; set; } = new List<string>();
        private List<string> ExternalContactsUsedAlphabet { get; set; } = new List<string>();
        private List<string> CompaniesUsedAlphabet { get; set; } = new List<string>();


        private readonly List<AlphabetElementListItem> _internalContactsAlphabetElements =
            new List<AlphabetElementListItem>();

        private readonly List<AlphabetElementListItem> _companyiesAlphabetElements = new List<AlphabetElementListItem>();

        private readonly List<AlphabetElementListItem> _externalContactsAlphabetElements =
            new List<AlphabetElementListItem>();

        private readonly List<AlphabetElementListItem> _externalGroupedContactsAlphabetElements =
            new List<AlphabetElementListItem>();


        private ContactSearchDividerListItem _internalContactsDivider;
        private ContactSearchDividerListItem _companiesDivider;
        private ContactSearchDividerListItem _externalContactsDivider;

        private Label NoMatchPlaceholder;

        public ContactTabControl BoundTabControl
        {
            get { return _boundTabControl; }
            set
            {
                _boundTabControl = value;
                _boundTabControl.SelectedModeChanged += _boundTabControl_SelectedModeChanged;

                switch (BoundTabControl.DefaultMode)
                {
                    case ContactTabControlMode.Contacts:
                        Content = _externalContactsStack;
                        _currentStack = _externalContactsStack;
                        _currentAlphabetElementList = _externalContactsAlphabetElements;

                        if (BoundAlphabetList != null)
                            BoundAlphabetList.Elements = ExternalContactsUsedAlphabet;
                        break;
                    case ContactTabControlMode.Companies:
                        Content = _companiesStack;
                        _currentStack = _companiesStack;
                        _currentAlphabetElementList = _companyiesAlphabetElements;

                        if (BoundAlphabetList != null)
                            BoundAlphabetList.Elements = CompaniesUsedAlphabet;
                        break;
                    case ContactTabControlMode.InternalContacts:
                        Content = _internalContactsStack;
                        _currentStack = _internalContactsStack;
                        _currentAlphabetElementList = _internalContactsAlphabetElements;

                        if (BoundAlphabetList != null)
                            BoundAlphabetList.Elements = InternalContactsUsedAlphabet;
                        break;
                }
                if (!ExternalContacts.Any())
                    _boundTabControl.RemoveItem(ContactTabControlMode.Contacts);

                if (!Companies.Any())
                    _boundTabControl.RemoveItem(ContactTabControlMode.Companies);

                if (!InternalContacts.Any())
                    _boundTabControl.RemoveItem(ContactTabControlMode.InternalContacts);

            }
        }

        public AlphabetList BoundAlphabetList
        {
            get { return _boundAlphabetist; }
            set
            {
                _boundAlphabetist = value;
                _boundAlphabetist.SelectedCharacterChanged += _boundAlphabetist_SelectedCharacterChanged;
            }
        }

        public ToolBarToggleButton BoundToggleButton
        {
            get { return _boundToggleButton; }
            set
            {
                _boundToggleButton = value;
                _boundToggleButton.ToggleChanged += _boundToggleButton_ToggleChanged;
            }
        }

        public ContactSearchBox BoundSearchBox
        {
            get { return _boundSearchBox; }
            set
            {
                _boundSearchBox = value;
                _boundSearchBox.SearchPhraseChanged += _boundSearchBox_SearchPhraseChanged;
            }
        }

        public bool SelectionMode
        {
            get { return _selectionMode; }
            set { _selectionMode = value; }
        }

        public bool MultipleSelection
        {
            get { return _multipleSelection; }
            set { _multipleSelection = value; }
        }

        #region Constructors

        public ContactList(List<PersonModel> persons, List<CompanyModel> companies, bool selectionMode = false,
            bool multipleSelection = true)
        {
            SelectionMode = selectionMode;
            MultipleSelection = multipleSelection;
            Persons = persons;
            InternalContacts = persons.Where(obj => obj.Company == null).ToList();
            ExternalContacts = persons.Where(obj => obj.Company != null).ToList();
            Companies = companies;

            NoMatchPlaceholder = new Label()
            {
                Content = "Brak wyników wyszukiwania",
                Foreground = new SolidColorBrush(ColorScheme.MenuDark),
                FontSize = 14,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Top,
                VerticalAlignment = VerticalAlignment.Stretch,
                Padding = new Thickness(0, 20, 0, 0)
            };

            Init();
            HandleLayouts();
        }

        #endregion

        #region Events

        private void _boundAlphabetist_SelectedCharacterChanged(object sender, EventArgs e)
        {
            AlphabetElementListItem listItem =
                _currentAlphabetElementList.Single(obj => obj.Element == BoundAlphabetList.SelectedCharacter);

            if (listItem != null)
            {
                ScrollToBottom();
                listItem.BringIntoView();
            }
        }

        private void _boundTabControl_SelectedModeChanged(object sender, EventArgs e)
        {
            switch (BoundTabControl.SelectedMode)
            {
                case ContactTabControlMode.Contacts:
                    if (_boundToggleButton != null && _boundToggleButton.Toggled)
                    {
                        Content = _externalGroupedContactsStack;
                        _currentStack = _externalGroupedContactsStack;
                        _currentAlphabetElementList = _externalGroupedContactsAlphabetElements;
                    }
                    else
                    {
                        Content = _externalContactsStack;
                        _currentStack = _externalContactsStack;
                        _currentAlphabetElementList = _externalContactsAlphabetElements;
                    }

                    if (_boundToggleButton != null)
                        _boundToggleButton.Visibility = Visibility.Visible;

                    if (BoundAlphabetList != null)
                        BoundAlphabetList.Elements = ExternalContactsUsedAlphabet;
                    break;
                case ContactTabControlMode.Companies:
                    Content = _companiesStack;
                    _currentStack = _companiesStack;
                    _currentAlphabetElementList = _companyiesAlphabetElements;
                    if (_boundToggleButton != null)
                        _boundToggleButton.Visibility = Visibility.Collapsed;

                    if (BoundAlphabetList != null)
                        BoundAlphabetList.Elements = CompaniesUsedAlphabet;
                    break;
                case ContactTabControlMode.InternalContacts:
                    Content = _internalContactsStack;
                    _currentStack = _internalContactsStack;
                    _currentAlphabetElementList = _internalContactsAlphabetElements;
                    if (_boundToggleButton != null)
                        _boundToggleButton.Visibility = Visibility.Collapsed;

                    if (BoundAlphabetList != null)
                        BoundAlphabetList.Elements = InternalContactsUsedAlphabet;
                    break;
            }
        }

        private void _boundToggleButton_ToggleChanged(object sender, EventArgs e)
        {
            if (_boundToggleButton.Toggled)
            {
                Content = _externalGroupedContactsStack;
                _currentStack = _externalGroupedContactsStack;
                _currentAlphabetElementList = _externalGroupedContactsAlphabetElements;

                if (BoundAlphabetList != null)
                    BoundAlphabetList.Elements = CompaniesUsedAlphabet;
            }
            else
            {
                Content = _externalContactsStack;
                _currentStack = _externalContactsStack;
                _currentAlphabetElementList = _externalContactsAlphabetElements;

                if (BoundAlphabetList != null)
                    BoundAlphabetList.Elements = ExternalContactsUsedAlphabet;
            }
        }

        private void _boundSearchBox_SearchPhraseChanged(object sender, EventArgs e)
        {
            try
            {
                string searchPhrase = _boundSearchBox.SearchPhrase;

                bool matchFound = false;
                if (searchPhrase != "" && searchPhrase != "Wyszukaj")
                {
                    List<PersonModel> searchedExternalContacts =
                        ExternalContacts.Where(obj => ContainsSearchTerm(obj, searchPhrase)).ToList();

                    List<CompanyModel> searchedCompanies =
                        Companies.Where(obj => ContainsSearchTerm(obj, searchPhrase)).ToList();

                    List<PersonModel> searchedInternalContacts =
                        InternalContacts.Where(obj => ContainsSearchTerm(obj, searchPhrase)).ToList();

                    _searchStack.Children.Clear();

                    if (searchedExternalContacts.Any())
                    {
                        _searchStack.Children.Add(_externalContactsDivider);

                        foreach (PersonModel person in searchedExternalContacts)
                        {
                            ContactPersonListItem item = new ContactPersonListItem(person);
                            item.Click += PersonItem_Click;

                            _searchStack.Children.Add(item);
                        }
                        matchFound = true;;
                    }

                    if (searchedCompanies.Any())
                    {
                        _searchStack.Children.Add(_companiesDivider);

                        foreach (CompanyModel company in searchedCompanies)
                        {
                            ContactCompanyListItem item = new ContactCompanyListItem(company);

                            _searchStack.Children.Add(item);
                        }
                        matchFound = true; ;
                    }

                    if (searchedInternalContacts.Any())
                    {
                        _searchStack.Children.Add(_internalContactsDivider);

                        foreach (PersonModel person in searchedInternalContacts)
                        {
                            ContactPersonListItem item = new ContactPersonListItem(person);
                            item.Click += PersonItem_Click;

                            _searchStack.Children.Add(item);
                        }
                        matchFound = true; ;
                    }

                    BoundAlphabetList.Elements = new List<string>();


                    if (matchFound)
                    {
                        Content = _searchStack;
                        _boundToggleButton.Visibility = Visibility.Collapsed;
                        _boundTabControl.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Content = NoMatchPlaceholder;
                    }
                }
                else
                {
                    switch (_boundTabControl.SelectedMode)
                    {
                        case ContactTabControlMode.Contacts:
                            if (_boundToggleButton.Toggled)
                            {
                                Content = _externalGroupedContactsStack;
                                _currentStack = _externalGroupedContactsStack;
                                _currentAlphabetElementList = _externalGroupedContactsAlphabetElements;
                            }
                            else
                            {
                                Content = _externalContactsStack;
                                _currentStack = _externalContactsStack;
                                _currentAlphabetElementList = _externalContactsAlphabetElements;
                                BoundAlphabetList.Elements = ExternalContactsUsedAlphabet;
                            }
                            break;
                        case ContactTabControlMode.Companies:
                            Content = _companiesStack;
                            _currentStack = _companiesStack;
                            _currentAlphabetElementList = _companyiesAlphabetElements;
                            BoundAlphabetList.Elements = CompaniesUsedAlphabet;
                            break;
                        case ContactTabControlMode.InternalContacts:
                            Content = _internalContactsStack;
                            _currentStack = _internalContactsStack;
                            _currentAlphabetElementList = _internalContactsAlphabetElements;
                            BoundAlphabetList.Elements = InternalContactsUsedAlphabet;
                            break;
                    }

                    _boundToggleButton.Visibility = Visibility.Visible;
                    _boundTabControl.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {

            }
        }

        #endregion

        #region Methods

        public bool ContainsSearchTerm(object instance, string searchTerm)
        {
            searchTerm = searchTerm.ToLower();
            if (instance is PersonModel)
            {
                PersonModel person = (PersonModel) instance;

                return person.FirstName.ToLower().Contains(searchTerm) ||
                       person.LastName.ToLower().Contains(searchTerm) ||
                       person.PhoneNumbers.Any(
                           (obj =>
                               obj.Number.ToLower().Contains(searchTerm) ||
                               obj.Name.ToLower().Contains(searchTerm) && obj.Active)) ||
                       person.EmailAddresses.Any(
                           (obj =>
                               obj.Address.ToLower().Contains(searchTerm) ||
                               obj.Name.ToLower().Contains(searchTerm) && obj.Active));
            }

            if (instance is CompanyModel)
            {
                CompanyModel company = (CompanyModel) instance;

                return company.Name.ToLower().Contains(searchTerm);
            }

            return false;
        }

        private void Init()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Stretch;

            _internalContactsStack = new StackPanel();
            _externalContactsStack = new StackPanel();
            _companiesStack = new StackPanel();
            _externalGroupedContactsStack = new StackPanel();
            _searchStack = new StackPanel();

            _externalContactsDivider = new ContactSearchDividerListItem("Kontakty");
            _companiesDivider = new ContactSearchDividerListItem("Firmy");
            _internalContactsDivider = new ContactSearchDividerListItem("Kontakty wewnętrzne");

            //if (_listMode == ContactListMode.SearchMode)
            //{
            //    _personsDivider = new ContactSearchDividerListItem("Kontakty");
            //    _companiesDivider = new ContactSearchDividerListItem("Firmy");
            //    _internalContactsDivider = new ContactSearchDividerListItem("Wewnętrzne");

            //    _personsDivider.Visibility = Visibility.Collapsed;
            //    _companiesDivider.Visibility = Visibility.Collapsed;
            //    _internalContactsDivider.Visibility = Visibility.Collapsed;

            //    _innerStack.Children.Add(_personsDivider);
            //    _innerStack.Children.Add(_companiesDivider);
            //    _innerStack.Children.Add(_internalContactsDivider);
            //}
        }

        private void HandleLayouts()
        {
            foreach (PersonModel person in Persons)
            {
                AddPerson(person);
            }

            foreach (CompanyModel company in Companies)
            {
                if (!_usedCompanies.Contains(company))
                {
                    AddCompany(company);
                }
            }
        }

        public void AddPerson(PersonModel person)
        {
            ContactPersonListItem personItem;

            int listIndex = 0;
            bool nameCharExists = false;
            bool companyExists = false;

            StackPanel layoutPanel = person.Company == null
                ? _internalContactsStack
                : _externalContactsStack;

            List<string> usedAlphabet = person.Company == null
                ? InternalContactsUsedAlphabet
                : ExternalContactsUsedAlphabet;

            List<PersonModel> usedList = person.Company == null
                ? _usedInternalContacts
                : _usedExternalContacts;

            List<AlphabetElementListItem> usedAlphabetElements = person.Company == null
                ? _internalContactsAlphabetElements
                : _externalContactsAlphabetElements;

            #region Adding person

            string personNameChar = person.LastName.First().ToString().ToUpper();

            usedList.Add(person);
            usedList = usedList.OrderBy(obj => obj.LastName).ToList();

            if (!usedAlphabet.Contains(personNameChar))
            {
                usedAlphabet.Add(personNameChar);
            }
            else
            {
                nameCharExists = true;
                listIndex += 1;
            }

            usedAlphabet.Sort();

            listIndex += usedList.IndexOf(person) + usedAlphabet.IndexOf(personNameChar);

            personItem = new ContactPersonListItem(person, SelectionMode);

            personItem.Click += PersonItem_Click;

            if (nameCharExists)
            {
                layoutPanel.Children.Insert(listIndex, personItem);
            }
            else
            {
                AlphabetElementListItem alphabetItem = new AlphabetElementListItem(personNameChar);
                layoutPanel.Children.Insert(listIndex, alphabetItem);
                usedAlphabetElements.Add(alphabetItem);
                layoutPanel.Children.Insert(listIndex + 1, personItem);
            }

            #endregion

            #region Adding company

            ContactCompanyListItem companyItem;

            listIndex = 0;
            nameCharExists = false;

            if (person.Company != null)
            {
                string companyNameChar = person.Company.Name.First().ToString().ToUpper();

                if (!_usedCompanies.Contains(person.Company))
                {
                    _usedCompanies.Add(person.Company);
                }
                else
                {
                    companyExists = true;
                }

                _usedCompanies = _usedCompanies.OrderBy(obj => obj.Name).ToList();

                if (!CompaniesUsedAlphabet.Contains(companyNameChar))
                {
                    CompaniesUsedAlphabet.Add(companyNameChar);
                }
                else
                {
                    nameCharExists = true;
                    listIndex += 1;
                }

                CompaniesUsedAlphabet.Sort();

                listIndex += _usedCompanies.IndexOf(person.Company) + CompaniesUsedAlphabet.IndexOf(companyNameChar);

                companyItem = new ContactCompanyListItem(person.Company);

                companyItem.Rename += CompanyItem_Rename;
                //companyItem.Click += PersonItem_Click;

                if (!companyExists)
                {
                    if (nameCharExists)
                    {
                        _companiesStack.Children.Insert(listIndex, companyItem);
                    }
                    else
                    {
                        AlphabetElementListItem alphabetItem = new AlphabetElementListItem(companyNameChar);
                        _companiesStack.Children.Insert(listIndex, alphabetItem);
                        _companyiesAlphabetElements.Add(alphabetItem);
                        _companiesStack.Children.Insert(listIndex + 1, companyItem);
                    }
                }
            }

            #endregion

            #region Adding grouped person

            listIndex = 0;

            if (person.Company != null)
            {
                string companyNameChar = person.Company.Name.First().ToString().ToUpper();

                usedList = usedList.OrderBy(obj => obj.Company.Name).ThenBy(obj => obj.FullName).ToList();
                _usedCompanies = _usedCompanies.OrderBy(obj => obj.Name).ToList();
                CompaniesUsedAlphabet.Sort();

                if (nameCharExists)
                    listIndex ++;

                if (companyExists)
                    listIndex ++;

                listIndex += _usedCompanies.IndexOf(person.Company) + CompaniesUsedAlphabet.IndexOf(companyNameChar) +
                             usedList.IndexOf(person);

                companyItem = new ContactCompanyListItem(person.Company);
                companyItem.Rename += CompanyItem_Rename;

                personItem = new ContactPersonListItem(person, SelectionMode);
                personItem.Click += PersonItem_Click;

                if (!companyExists)
                {
                    if (nameCharExists)
                    {
                        _externalGroupedContactsStack.Children.Insert(listIndex, companyItem);
                        _externalGroupedContactsStack.Children.Insert(listIndex + 1, personItem);
                    }
                    else
                    {
                        AlphabetElementListItem alphabetItem = new AlphabetElementListItem(companyNameChar);
                        _externalGroupedContactsStack.Children.Insert(listIndex, alphabetItem);
                        _externalGroupedContactsAlphabetElements.Add(alphabetItem);
                        _externalGroupedContactsStack.Children.Insert(listIndex + 1, companyItem);
                        _externalGroupedContactsStack.Children.Insert(listIndex + 2, personItem);
                    }
                }
                else
                {
                    _externalGroupedContactsStack.Children.Insert(listIndex, personItem);
                }
            }

            #endregion
        }

        private void CompanyItem_Rename(object sender, EventArgs e)
        {
            RenameCompany?.Invoke(sender, e);
        }

        public void AddCompany(CompanyModel company)
        {
            try
            {
                ContactCompanyListItem companyItem;

                int listIndex = 0;
                bool nameCharExists = false;
                bool companyExists = false;

                if (company != null)
                {
                    string companyNameChar = company.Name.First().ToString().ToUpper();

                    if (!_usedCompanies.Contains(company))
                    {
                        _usedCompanies.Add(company);
                    }
                    else
                    {
                        companyExists = true;
                    }

                    _usedCompanies = _usedCompanies.OrderBy(obj => obj.Name).ToList();

                    if (!CompaniesUsedAlphabet.Contains(companyNameChar))
                    {
                        CompaniesUsedAlphabet.Add(companyNameChar);
                    }
                    else
                    {
                        nameCharExists = true;
                        listIndex += 1;
                    }

                    CompaniesUsedAlphabet.Sort();

                    listIndex += _usedCompanies.IndexOf(company) + CompaniesUsedAlphabet.IndexOf(companyNameChar);

                    companyItem = new ContactCompanyListItem(company);
                    companyItem.DataChanged += CompanyItem_DataChanged;
                    companyItem.Rename += CompanyItem_Rename;

                    //companyItem.Click += PersonItem_Click;

                    if (!companyExists)
                    {
                        if (nameCharExists)
                        {
                            _companiesStack.Children.Insert(listIndex, companyItem);
                        }
                        else
                        {
                            AlphabetElementListItem alphabetItem = new AlphabetElementListItem(companyNameChar);
                            _companiesStack.Children.Insert(listIndex, alphabetItem);
                            _companyiesAlphabetElements.Add(alphabetItem);
                            _companiesStack.Children.Insert(listIndex + 1, companyItem);

                            if (_boundTabControl.SelectedMode == ContactTabControlMode.Companies)
                                _boundAlphabetist.AddToUsedAlphabet(companyNameChar);
                        }
                    }
                }

            }
            catch (Exception ex)
            {

            }
        }

        private void CompanyItem_DataChanged(object sender, EventArgs e)
        {
            ContactCompanyListItem item = (ContactCompanyListItem) sender;
            _companiesStack.Children.Remove(item);
            _usedCompanies.Remove(item.Company);
            item.Company.DataChanged -= item.Company_DataChanged;
            AddCompany(item.Company);
            ClearAlphabetElements();
        }

        private void ClearAlphabetElements()
        {
            List<UIElement> elementsToDelete = new List<UIElement>();
            UIElement element1 = null;
            foreach (UIElement element in _companiesStack.Children)
            {
                if (element1 == null)
                {
                    element1 = element;
                    continue;
                }

                if (element1 is AlphabetElementListItem && element is AlphabetElementListItem)
                {
                    AlphabetElementListItem item = (AlphabetElementListItem)element1;
                    CompaniesUsedAlphabet.Remove(item.Element);
                    elementsToDelete.Add(element1);
                }

                element1 = element;
            }

            elementsToDelete.ForEach(obj => _companiesStack.Children.Remove(obj));
        }

        private void PersonItem_Click(object sender, EventArgs e)
        {
            ContactPersonListItem item = (ContactPersonListItem) sender;

            if (SelectionMode)
            {
                if (MultipleSelection)
                {
                    SelectedPersons.Add(item);
                }
                else
                {
                    SelectedPersons.ForEach(obj => obj.Selected = false);
                    SelectedPersons.Clear();

                    SelectedPersons.Add(item);
                }

                item.Selected = true;
                SelectedItemsChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {

            }
        }

        #endregion
    }

    #endregion

    #region Enums
    public enum ContactTabControlMode
    {
        Contacts,
        Companies,
        InternalContacts
    }

    public enum ContactListOuterMode
    {
        PersonOnlyMode,
        GroupedPersonsMode,
        CompanyMode
    }

    public enum ContactListMode
    {
        PersonMode,
        CompanyMode,
        SearchMode
    }
    #endregion
}
