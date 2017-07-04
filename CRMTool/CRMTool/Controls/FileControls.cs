using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Licencjat_new.CustomClasses;

namespace Licencjat_new.Controls
{
    public class FileListItem : Border
    {
        private TextBlock _fileName;

        public event EventHandler RenameFile;
        public event EventHandler DownloadFile;
        public event EventHandler RemoveFile;
        public event EventHandler DataChanged;
        public event EventHandler SelectedChanged;

        private ContextMenu _contextMenu;
        private MenuItem _detailsItem;
        private MenuItem _renameItem;
        private Separator _separator1;
        private MenuItem _downloadItem;
        private Separator _separator2;
        private MenuItem _removeItem;

        private bool _allowShowDetails = true;
        private bool _allowRename = true;
        private bool _allowDownload = true;
        private bool _allowDelete = true;

        private bool _selected;

        public bool Selected
        {
            get { return _selected; }
            set
            {
                _selected = value;

                if (Selected)
                    Background = new SolidColorBrush(ColorScheme.GlobalBlue) {Opacity = 0.3};
                else
                    Background = new SolidColorBrush(Colors.Transparent);
            }
        }

        public bool AllowSelect { get; set; }
   
        public bool AllowShowDetails
        {
            get { return _allowShowDetails; }
            set
            {
                _allowShowDetails = value;
                if (!AllowShowDetails)
                {
                    _contextMenu.Items.Remove(_detailsItem);
                }
            }
        }

        public bool AllowRename
        {
            get { return _allowRename; }
            set
            {
                _allowRename = value;
                if (!AllowRename)
                {
                    _contextMenu.Items.Remove(_renameItem);
                }
            }
        }

        public bool AllowDownload
        {
            get { return _allowDownload; }
            set
            {
                _allowDownload = value;
                if (!AllowDownload)
                {
                    _contextMenu.Items.Remove(_downloadItem);
                }
            }
        }

        public bool AllowDelete
        {
            get { return _allowDelete; }
            set
            {
                _allowDelete = value;
                if (!AllowDelete)
                {
                    _contextMenu.Items.Remove(_allowDelete);
                }
            }
        }

        public FileModel File { get; set; }
        public FileListItem(FileModel file, double width = 60)
        {
            File = file;
            Width = width;
            Redraw();

            file.DataChanged += File_DataChanged;
        }

        private void RenameItem_Click(object sender, RoutedEventArgs e)
        {
            RenameFile?.Invoke(this, EventArgs.Empty);
        }

        private void DownloadItem_Click(object sender, RoutedEventArgs e)
        {
            DownloadFile?.Invoke(this, EventArgs.Empty);
        }

        private void _removeItem_Click(object sender, RoutedEventArgs e)
        {
            RemoveFile?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.NewSize.Height < 60)
            {
                _fileName.Height = 16;
            }
            else
            {
                _fileName.Height = 38;
            }
        }

        private void File_DataChanged(object sender, EventArgs e)
        {
            Redraw();
            DataChanged?.Invoke(sender, e);
        }

        private void FileListItem_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(!Selected)
            Background = new SolidColorBrush(Colors.Transparent);
        }

        private void FileListItem_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(!Selected)
            Background = new SolidColorBrush(ColorScheme.GlobalBlue) {Opacity = 0.3};
        }

        private void FileListItem_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            PreviewMouseLeftButtonUp += FileListItem_PreviewMouseLeftButtonUp;
        }

        private void FileListItem_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (AllowSelect)
                Selected = !Selected;
            SelectedChanged?.Invoke(this, EventArgs.Empty);

            PreviewMouseLeftButtonUp -= FileListItem_PreviewMouseLeftButtonUp;
        }

        public void Redraw()
        {
            Height = double.NaN;
            CornerRadius = new CornerRadius(5);
            Margin = new Thickness(2);

            MouseEnter += FileListItem_MouseEnter;
            MouseLeave += FileListItem_MouseLeave;

            PreviewMouseLeftButtonDown += FileListItem_PreviewMouseLeftButtonDown;

            DockPanel innerDock = new DockPanel()
            {
                Height = double.NaN,
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch
            };

            _fileName = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Padding = new Thickness(5, 0, 5, 3),
                Foreground = new SolidColorBrush(Colors.Black),
                TextWrapping = TextWrapping.Wrap,
                Text = File.Name,
                Height = 38,
                TextTrimming = TextTrimming.CharacterEllipsis,
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
            };

            Image fileIcon = new Image()
            {
                Width = Width - 30,
                Height = double.NaN,
                Margin = new Thickness(3, 5, 3, 5)
            };

            if (File.ContentType != null)
            {
                fileIcon.Source = FileHelper.GetFileIcon(File.ContentType);
            }
            else
            {
                fileIcon.Source = File.Icon;
            }


            RenderOptions.SetBitmapScalingMode(fileIcon, BitmapScalingMode.HighQuality);

            DockPanel.SetDock(_fileName, Dock.Bottom);
            innerDock.Children.Add(_fileName);

            innerDock.Children.Add(fileIcon);

            innerDock.ToolTip = new FileListItemToolTip(File);

            Child = innerDock;

            _contextMenu = new ContextMenu();

            _detailsItem = new MenuItem()
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
            _contextMenu.Items.Add(_detailsItem);

            _renameItem = new MenuItem()
            {
                Header = "Zmień nazwę",
                Icon =
                    new Image()
                    {
                        Source =
                            ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/rename_context.png"))
                    }
            };
            _renameItem.Click += RenameItem_Click;
            _contextMenu.Items.Add(_renameItem);

            _separator1 = new Separator();
            _contextMenu.Items.Add(_separator1);

            _downloadItem = new MenuItem()
            {
                Header = "Pobierz",
                Icon =
                    new Image()
                    {
                        Source =
                            ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/download_context.png"))
                    }
            };
            _downloadItem.Click += DownloadItem_Click;
            _contextMenu.Items.Add(_downloadItem);

            _separator2 = new Separator();
            _contextMenu.Items.Add(_separator2);

            _removeItem = new MenuItem()
            {
                Header = "Usuń",
                Icon =
                    new Image()
                    {
                        Source =
                            ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/remove_context.png"))
                    }
            };
            _removeItem.Click += _removeItem_Click;
            _contextMenu.Items.Add(_removeItem);

            ContextMenu = _contextMenu;

            if (Height < 60)
                _fileName.Height = 16;
        }
    }

    class FileListItemToolTip : Grid
    {
        public FileListItemToolTip(FileModel file)
        {
            ColumnDefinitions.Add(new ColumnDefinition());
            ColumnDefinitions.Add(new ColumnDefinition());

            RowDefinitions.Add(new RowDefinition());
            RowDefinitions.Add(new RowDefinition());
            RowDefinitions.Add(new RowDefinition());


            Label nameLabel = new Label()
            {
                HorizontalContentAlignment = HorizontalAlignment.Right,
                FontSize = 11,
                Padding = new Thickness(5,3,5,3),
                Content = "Nazwa:"
            };

            Label dateLabel = new Label()
            {
                HorizontalContentAlignment = HorizontalAlignment.Right,
                FontSize = 11,
                Padding = new Thickness(5, 3, 5, 3),
                Content = "Data dodania:"
            };

            Label sizeLabel = new Label()
            {
                HorizontalContentAlignment = HorizontalAlignment.Right,
                FontSize = 11,
                Padding = new Thickness(5, 3, 5, 3),
                Content = "Rozmiar:"
            };

            TextBlock nameData = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(0, 3, 5, 3),
                Text = file.Name
            };

            TextBlock dateData = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(0, 3, 5, 3),
                Text = file.DateAdded.ToString("dd.MM.yyyy")
            };

            string sizeString = "";
            double sizeDouble = (double) file.Size;

            if (sizeDouble/1024 < 1)
                sizeString = "< 1KB";
            else if (sizeDouble / 1024 < 1024)
                sizeString = sizeDouble / 1024 + " KB";
            else if (sizeDouble/ 1024576 < 100)
                sizeString = Math.Round(sizeDouble/1024576,1) + " MB";

            TextBlock sizeData = new TextBlock()
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(0, 3, 5, 3),
                Text = sizeString
            };

            Grid.SetColumn(nameLabel, 0);
            Grid.SetRow(nameLabel, 0);
            Children.Add(nameLabel);

            Grid.SetColumn(dateLabel, 0);
            Grid.SetRow(dateLabel, 1);
            Children.Add(dateLabel);

            Grid.SetColumn(sizeLabel, 0);
            Grid.SetRow(sizeLabel, 2);
            Children.Add(sizeLabel);

            Grid.SetColumn(nameData, 1);
            Grid.SetRow(nameData, 0);
            Children.Add(nameData);

            Grid.SetColumn(dateData, 1);
            Grid.SetRow(dateData, 1);
            Children.Add(dateData);

            Grid.SetColumn(sizeData, 1);
            Grid.SetRow(sizeData, 2);
            Children.Add(sizeData);
        }
    }

    class FileMain : DockPanel
    {
        public FileMain()
        {
            Width = double.NaN;
            Height = double.NaN;
            Background = new SolidColorBrush(ColorScheme.MenuLight);

            ToolBarMainMenuStrip mainMenu = new ToolBarMainMenuStrip();

            FileSortButton sortButton = new FileSortButton();
            mainMenu.AddButton(sortButton, Dock.Right);

            Children.Add(mainMenu);
        }
    }

    class FileList : ScrollViewer
    {
        private FileSortButton _boundSortButton;
        private StackPanel _innerStack;
        private List<WrapPanel> sortWrapPanels = new List<WrapPanel>();
        private bool _allowSelect;

        public event EventHandler RenameFile;
        public event EventHandler DownloadFile;
        public event EventHandler SelectedListChanged;

        public List<FileListItem> Files = new List<FileListItem>();

        public List<FileModel> SelectedFiles = new List<FileModel>();

        private Label NoElementsPlaceholder;

        public bool AllowSelect
        {
            get { return _allowSelect; }
            set
            {
                _allowSelect = value;
                foreach (FileListItem fileItem in Files)
                {
                    fileItem.AllowSelect = AllowSelect;
                }
            }
        }

        public FileSortButton BoundSortButton
        {
            get { return _boundSortButton; }
            set
            {
                _boundSortButton = value;
                BoundSortButton.SortModeChanged += BoundSortButton_SortChanged;
                BoundSortButton.SortDirectionChanged += BoundSortButton_SortChanged;
            }
        }

        public void Sort()
        {
            _innerStack.Children.Clear();
            _innerStack.Children.Add(NoElementsPlaceholder);
            sortWrapPanels.ForEach(obj => obj.Children.Clear());
            sortWrapPanels.Clear();
            List<FileListItem> sortedFiles = new List<FileListItem>();
            List<string> sortElements = new List<string>();

            switch (BoundSortButton.SortMode)
            {
                case SortMode.NameSort:
                    sortedFiles = BoundSortButton.SortDirection == SortDirection.Ascending ? Files.OrderBy(obj => obj.File.Name).ToList() : Files.OrderByDescending(obj => obj.File.Name).ToList();
                    sortElements = sortedFiles.Select(obj => obj.File.Name.First().ToString().ToUpper()).Distinct().ToList();

                    foreach (string sortElement in sortElements)
                    {
                        _innerStack.Children.Add(new FileListSortElementItem(sortElement));

                        List<FileListItem> sortElementItems = Files.Where(obj => obj.File.Name.First().ToString().ToUpper() == sortElement).ToList();

                        WrapPanel sortElementPanel = new WrapPanel()
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch
                        };

                        sortWrapPanels.Add(sortElementPanel);

                        sortElementItems.ForEach(obj => sortElementPanel.Children.Add(obj));
                        _innerStack.Children.Remove(NoElementsPlaceholder);
                        _innerStack.Children.Add(sortElementPanel);
                    }
                    break;
                case SortMode.DateSort:
                    sortedFiles = BoundSortButton.SortDirection == SortDirection.Ascending ? Files.OrderBy(obj => obj.File.DateAdded).ToList() : Files.OrderByDescending(obj => obj.File.DateAdded).ToList();
                    sortElements = sortedFiles.Select(obj => obj.File.DateAdded.ToString("dd.MM.yyyy")).Distinct().ToList();

                    foreach (string sortElement in sortElements)
                    {
                        _innerStack.Children.Add(new FileListSortElementItem(sortElement));

                        List<FileListItem> sortElementItems = Files.Where(obj => obj.File.DateAdded.ToString("dd.MM.yyyy") == sortElement).ToList();

                        WrapPanel sortElementPanel = new WrapPanel()
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch
                        };

                        sortWrapPanels.Add(sortElementPanel);

                        sortElementItems.ForEach(obj => sortElementPanel.Children.Add(obj));
                        _innerStack.Children.Remove(NoElementsPlaceholder);
                        _innerStack.Children.Add(sortElementPanel);
                    }
                    break;
                case SortMode.TypeSort:
                    sortedFiles = BoundSortButton.SortDirection == SortDirection.Ascending ? Files.OrderBy(obj => obj.File.Name.Substring(obj.File.Name.LastIndexOf('.'))).ToList() : Files.OrderByDescending(obj => obj.File.Name.Substring(obj.File.Name.LastIndexOf('.'))).ToList();
                    sortElements = sortedFiles.Select(obj => obj.File.Name.Substring(obj.File.Name.LastIndexOf('.'))).Distinct().ToList();

                    foreach (string sortElement in sortElements)
                    {
                        _innerStack.Children.Add(new FileListSortElementItem(sortElement));

                        List<FileListItem> sortElementItems = Files.Where(obj => obj.File.Name.Substring(obj.File.Name.LastIndexOf('.')) == sortElement).ToList();

                        WrapPanel sortElementPanel = new WrapPanel()
                        {
                            HorizontalAlignment = HorizontalAlignment.Stretch
                        };

                        sortWrapPanels.Add(sortElementPanel);

                        sortElementItems.ForEach(obj => sortElementPanel.Children.Add(obj));
                        _innerStack.Children.Remove(NoElementsPlaceholder);
                        _innerStack.Children.Add(sortElementPanel);
                    }
                    break;
            }
        }

        private void BoundSortButton_SortChanged(object sender, EventArgs e)
        {
            Sort();
        }

        public FileList()
        {
            VerticalAlignment = VerticalAlignment.Stretch;
            HorizontalAlignment = HorizontalAlignment.Stretch;

            _innerStack = new StackPanel()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            NoElementsPlaceholder = new Label()
            {
                Content = "Brak dokumentów",
                Foreground = new SolidColorBrush(ColorScheme.MenuDark),
                FontSize = 14,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalContentAlignment = VerticalAlignment.Top,
                VerticalAlignment = VerticalAlignment.Stretch,
                Padding = new Thickness(0, 20, 0, 0)
            };

            _innerStack.Children.Add(NoElementsPlaceholder);

            Content = _innerStack;
        }

        public void AddFiles(List<FileModel> files)
        {
            foreach (FileModel file in files)
            {
                FileListItem item = new FileListItem(file);
                item.Margin = new Thickness(5);

                item.RenameFile += Item_RenameFile;
                item.DownloadFile += Item_DownloadFile;
                item.SelectedChanged += Item_SelectedChanged;
                item.DataChanged += Item_DataChanged;
               
                if(SelectedFiles.Contains(file))
                    item.Selected = true;

                item.AllowSelect = AllowSelect;

                Files.Add(item);
            }

            _innerStack.Children.Remove(NoElementsPlaceholder);

            Sort();
        }

        private void Item_SelectedChanged(object sender, EventArgs e)
        {
            FileListItem fileItem = (FileListItem)sender;

            if (fileItem.Selected)
            {
                SelectedFiles.Add(fileItem.File);
            }
            else
            {
                SelectedFiles.Remove(fileItem.File);
            }

            SelectedListChanged?.Invoke(this, EventArgs.Empty);
        }

        private void Item_DataChanged(object sender, EventArgs e)
        {
            Sort();
        }

        private void Item_DownloadFile(object sender, EventArgs e)
        {
            DownloadFile?.Invoke(sender, e);
        }

        private void Item_RenameFile(object sender, EventArgs e)
        {
            RenameFile?.Invoke(sender, e);
        }

        public void ClearFiles()
        {
            foreach (FileListItem file in Files)
            {
                file.RenameFile -= Item_RenameFile;
                file.DownloadFile -= Item_DownloadFile;
            }
            Files.Clear();
            sortWrapPanels.ForEach(obj => obj.Children.Clear());
            _innerStack.Children.Clear();;
            sortWrapPanels.Clear();

            _innerStack.Children.Add(NoElementsPlaceholder);
        }
    }

    public class FileListSortElementItem : DockPanel
    {
        public FileListSortElementItem(string text)
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Height = 30;

            Label elementLabel = new Label()
            {
                VerticalAlignment = VerticalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                HorizontalContentAlignment = HorizontalAlignment.Left,
                Content = text,
                FontSize = 14,
                Padding = new Thickness(15,0,0,0),
                Foreground = new SolidColorBrush(ColorScheme.GlobalBlue)
            };

            Children.Add(elementLabel);
        }
    }

    public class FileSortButton : DockPanel
    {
        private List<ToolBarWideButton> _sortButtons = new List<ToolBarWideButton>();
        private List<ToolBarWideButton> _directionButtons = new List<ToolBarWideButton>();
        private SortMode _sortMode;
        private SortDirection _sortDirection;

        public event EventHandler SortModeChanged;
        public event EventHandler SortDirectionChanged;

        public SortMode SortMode
        {
            get { return _sortMode; }
            set
            {
                _sortMode = value;
                ToolBarWideButton sortButton = _sortButtons.Find(obj => (SortMode) obj.HelperChild == SortMode);

                if (sortButton != null && !sortButton.Toggled)
                    sortButton.Toggled = true;

                _sortButtons.Where(obj => obj != sortButton)
                    .ToList()
                    .ForEach(obj => obj.Toggled = false);

                SortModeChanged?.Invoke(this, EventArgs.Empty);

                _directionButtons[0].Toggled = true;
            }
        }

        public SortDirection SortDirection
        {
            get { return _sortDirection; }
            set
            {
                _sortDirection = value;
                _directionButtons.Where(obj => (SortDirection) obj.HelperChild != SortDirection)
                    .ToList()
                    .ForEach(obj => obj.Toggled = false);

                SortDirectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public FileSortButton()
        {
            VerticalAlignment = VerticalAlignment.Stretch;
            HorizontalAlignment = HorizontalAlignment.Right;
            Name = "FileSortButton";

            StackPanel sortModeStackPanel = new StackPanel()
            {
                VerticalAlignment = VerticalAlignment.Stretch,
                Orientation = Orientation.Horizontal
            };

            Image sortingImage = new Image()
            {
                Width = 18,
                Height = 18,
                Margin = new Thickness(0, 11,10,11),
                Source = ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/sorting_blue.png"))
            };

            RenderOptions.SetBitmapScalingMode(sortingImage, BitmapScalingMode.HighQuality);

            ToolBarWideButton nameSortButton = new ToolBarWideButton("Nazwa",null,true);
            nameSortButton.ToggledChanged += SortButton_ToggledChanged;
            nameSortButton.HelperChild = SortMode.NameSort;

            ToolBarWideButton typeSortButton = new ToolBarWideButton("Typ", null, true);
            typeSortButton.ToggledChanged += SortButton_ToggledChanged;
            typeSortButton.HelperChild = SortMode.TypeSort;

            ToolBarWideButton dateSortButton = new ToolBarWideButton("Data", null, true);
            dateSortButton.ToggledChanged += SortButton_ToggledChanged;
            dateSortButton.HelperChild = SortMode.DateSort;

            ToolBarSpacer spacer = new ToolBarSpacer(0.6, new Thickness(5, 8, 5, 8));

            ToolBarWideButton ascendingSortButton = new ToolBarWideButton("Rosnąco", null, true);
            ascendingSortButton.ToggledChanged += SortDirectionButton_ToggledChanged;
            ascendingSortButton.HelperChild = SortDirection.Ascending;

            ToolBarWideButton descendingSortButton = new ToolBarWideButton("Malejąco", null, true);
            descendingSortButton.ToggledChanged += SortDirectionButton_ToggledChanged;
            descendingSortButton.HelperChild = SortDirection.Descending;

            sortModeStackPanel.Children.Add(sortingImage);
            sortModeStackPanel.Children.Add(nameSortButton);
            sortModeStackPanel.Children.Add(typeSortButton);
            sortModeStackPanel.Children.Add(dateSortButton);
            sortModeStackPanel.Children.Add(spacer);
            sortModeStackPanel.Children.Add(ascendingSortButton);
            sortModeStackPanel.Children.Add(descendingSortButton);

            _sortButtons.Add(nameSortButton);
            _sortButtons.Add(typeSortButton);
            _sortButtons.Add(dateSortButton);

            _directionButtons.Add(ascendingSortButton);
            _directionButtons.Add(descendingSortButton);

            DockPanel.SetDock(sortModeStackPanel, Dock.Left);
            Children.Add(sortModeStackPanel);

            SortMode = SortMode.NameSort;
        }

        private void SortDirectionButton_ToggledChanged(object sender, EventArgs e)
        {
            ToolBarWideButton button = (ToolBarWideButton)sender;

            _directionButtons.Where(obj => (SortDirection)obj.HelperChild != (SortDirection)button.HelperChild)
                .ToList()
                .ForEach(obj => obj.Toggled = false);

            SortDirection = (SortDirection)button.HelperChild;
            SortDirectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void SortButton_ToggledChanged(object sender, EventArgs e)
        {
            ToolBarWideButton button = (ToolBarWideButton)sender;

            SortMode = (SortMode) button.HelperChild;
        }
    }

    public enum SortMode
    {
        NameSort,
        DateSort,
        TypeSort
    }

    public enum SortDirection
    {
        Ascending,
        Descending
    }
}
