using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Licencjat_new.CustomClasses;

namespace Licencjat_new.Windows.HelperWindows
{
    /// <summary>
    /// Interaction logic for AdditionalChoose.xaml
    /// </summary>
    public partial class AdditionalChoose : UserControl
    {
        private bool _multipleSelection;
        private List<AdditionalChooseItem> _chooseItems = new List<AdditionalChooseItem>();

        public event EventHandler ReadyButtonClicked;
        public event EventHandler CancelButtonClicked;
        

        public List<object> Items { get; set; }
        public List<object> SelectedItems { get; private set; } = new List<object>();


        public AdditionalChoose(List<object> items, string title, bool multipleSelection = false)
        {
            _multipleSelection = multipleSelection;

            InitializeComponent();

            Titlelabel.Content = title;
            Items = items;

            foreach (object item in Items)
            {
                AdditionalChooseItem chooseItem = new AdditionalChooseItem(item);
                chooseItem.Click += ChooseItem_Click;
                _chooseItems.Add(chooseItem);
                ItemsPanel.Children.Add(chooseItem);
            }

            ReadyButton.Clicked += (s, ea) =>
            {
                ReadyButtonClicked?.Invoke(this, EventArgs.Empty);
            };

            CancelButton.Clicked += (s, ea) =>
            {
                CancelButtonClicked?.Invoke(this, EventArgs.Empty);
            };
        }

        private void ChooseItem_Click(object sender, EventArgs e)
        {
            AdditionalChooseItem item = (AdditionalChooseItem) sender;

            if (_multipleSelection)
                item.Selected = !item.Selected;
            else
            {
                _chooseItems.ForEach(obj => obj.Selected = false);
                item.Selected = true;
                SelectedItems.Clear();
            }

            SelectedItems.Add(item.Object);
        }
    }

    public class AdditionalChooseItem : DockPanel
    {
        private Image _selectImage;

        private bool _selected;

        public event EventHandler Click;

        public object Object { get; set; }

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

        public AdditionalChooseItem(object item)
        {
            Object = item;

            _selectImage = new Image()
            {
                Height = 29,
                Width = 29,
                Margin = new Thickness(10),
                Source = ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/select_off.png"))
            };

            DockPanel.SetDock(_selectImage, Dock.Left);
            Children.Add(_selectImage);

            TextBlock textBlock = new TextBlock()
            {
                Height = 40,
                Padding = new Thickness(10),
                FontSize = 16,
                Text = item.ToString(),
                Foreground = new SolidColorBrush(ColorScheme.GlobalBlue),
            };

            Children.Add(textBlock);

            MouseEnter += AdditionalChooseItem_MouseEnter;
            MouseLeave += AdditionalChooseItem_MouseLeave;
            PreviewMouseLeftButtonDown += AdditionalChooseItem_PreviewMouseLeftButtonDown;
        }

        private void AdditionalChooseItem_MouseLeave(object sender, MouseEventArgs e)
        {
            Background = new SolidColorBrush(ColorScheme.MenuLight);
        }

        private void AdditionalChooseItem_MouseEnter(object sender, MouseEventArgs e)
        {
            Background = new SolidColorBrush(ColorScheme.MenuDarker);
        }

        private void AdditionalChooseItem_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            PreviewMouseLeftButtonUp += AdditionalChooseItem_PreviewMouseLeftButtonUp;
        }

        private void AdditionalChooseItem_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Click?.Invoke(this, EventArgs.Empty);
            PreviewMouseLeftButtonUp -= AdditionalChooseItem_PreviewMouseLeftButtonUp;
        }
    }
}
