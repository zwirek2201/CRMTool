using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Media;
using Licencjat_new.CustomClasses;

namespace Licencjat_new.Controls
{
    #region EmailList
    class EmailList : DockPanel
    {
        #region Constructors
        public EmailList()
        {
            Width = double.NaN;
            Height = double.NaN;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            LastChildFill = true;

            ToolBarMainMenuStrip mainMenu = new ToolBarMainMenuStrip();
            mainMenu.Children.Add(new ToolBarButton("Nowy adres e-mail", new Uri("pack://application:,,,/resources/addEmailClient.png")));
            Children.Insert(0,mainMenu);
        }
        #endregion
    }
    #endregion

    #region EmailMain
    class EmailMain : DockPanel
    {
        #region Constructors
        public EmailMain()
        {
            Width = double.NaN;
            Height = double.NaN;
            Background = new SolidColorBrush(ColorScheme.MenuLight);

            ToolBarMainMenuStrip mainMenu = new ToolBarMainMenuStrip();

            Children.Add(mainMenu);
        }
        #endregion
    }

    class EmailMessagesList : DockPanel
    {
        #region Constructors
        public EmailMessagesList()
        {
            Width = double.NaN;
            Height = double.NaN;
            Background = new SolidColorBrush(ColorScheme.MenuLight);
        }
        #endregion
    }

    class EmailMessageDetails : DockPanel
    {
        #region Constructors
        public EmailMessageDetails()
        {
            Width = double.NaN;
            Height = double.NaN;
            Background = new SolidColorBrush(ColorScheme.MenuLight);
        }
        #endregion
    }
    #endregion

    class DragIndicator : Popup
    {
        #region Variables
        private TextBlock _label;
        private string _text;
        #endregion

        #region Properties
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                _label.Inlines.Clear();
                _label.Inlines.Add("Dodaj do ");
                _label.Inlines.Add(new Run(_text) {FontWeight = FontWeights.Medium});
            }
        }
        #endregion

        #region Constructors
        public DragIndicator()
        {

            Width = double.NaN;
            Height = double.NaN;
            MaxWidth = 150;
            AllowsTransparency = true;

            Border _border = new Border()
            {
                CornerRadius = new CornerRadius(8),
                Background = new SolidColorBrush(ColorScheme.GlobalBlue) { Opacity = 1 },
                BorderBrush = new SolidColorBrush(ColorScheme.MenuDarker),
                BorderThickness = new Thickness(1)
            };

            _label = new TextBlock()
            {
                Padding = new Thickness(10),
                MaxWidth = 150,
                Foreground = Brushes.White,
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap
            };

            _border.Child = _label;

            this.Child = _border;
        }
        #endregion
    }
}
