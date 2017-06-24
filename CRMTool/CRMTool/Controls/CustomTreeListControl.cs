using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Licencjat_new.CustomClasses;
using ContextMenu = System.Windows.Controls.ContextMenu;
using Label = System.Windows.Controls.Label;
using MenuItem = System.Windows.Controls.MenuItem;
using Orientation = System.Windows.Controls.Orientation;

namespace Licencjat_new.Controls
{
    class CustomTreeListControl : StackPanel
    {
        #region Variables
        public event EventHandler<CustomTreeListSelectedNodeChangedEventArgs> SelectedNodeChanged;

        private CustomTreeListNode _selectedNode;
        #endregion

        #region Properties

        public List<CustomTreeListNode> Nodes { get; set; } = new List<CustomTreeListNode>();

        public int SelectedIndex { get; private set; }

        public CustomTreeListNode SelectedNode
        {
            get { return _selectedNode; }
            internal set
            {
                _selectedNode = value;
                SelectedNodeChanged(this,
                    new CustomTreeListSelectedNodeChangedEventArgs() {SelectedNode = _selectedNode});
            }
        }
        #endregion

        #region Contstructors
        public CustomTreeListControl()
        {
            Width = double.NaN;
            Height = double.NaN;
            Background = new SolidColorBrush(ColorScheme.MenuLight);
        }
        #endregion

        #region Events
        public virtual void OnLoginStatusChanged(CustomTreeListSelectedNodeChangedEventArgs e)
        {
            EventHandler<CustomTreeListSelectedNodeChangedEventArgs> handler = SelectedNodeChanged;
            handler?.Invoke(this, e);
        }
        #endregion

        #region Methods
        public void AddNode(CustomTreeListNode node)
        {
            Nodes.Add(node);
            Children.Insert(Children.Count, node);
            node.Parent = this;
        }

        public CustomTreeListNode AddNode(string text)
        {
            CustomTreeListNode node = new CustomTreeListNode();
            node.Text = text;
            Nodes.Add(node);
            Children.Insert(Children.Count, node);
            node.Parent = this;
            return node;
        }

        public CustomTreeListNode AddNode(string text, ImageSource image)
        {
            CustomTreeListNode node = new CustomTreeListNode();
            node.IconSource = image;
            node.Text = text;
            node.Parent = this;
            Nodes.Add(node);
            Children.Insert(Children.Count, node);
            node.Parent = this;
            return node;
        }
        #endregion
    }

    public class CustomTreeListNode : DockPanel
    {
        #region Variables
        private CustomTreeListControl _tree;
        private int _level;
        private object _parent;

        private const int _indent = 10;
        private const double _height = 22;

        private string _text;
        private bool _bold = false;

        #endregion

        #region Properties
        public int Level
        {
            get { return _level; }
            internal set {
                _level = value;
                StackPanel marginHolder = (StackPanel)LogicalTreeHelper.FindLogicalNode(this, "marginHolder");

                if (marginHolder != null)
                    marginHolder.Width = _level*_indent;
            }
        }
        public List<CustomTreeListNode> Nodes { get; private set; } = new List<CustomTreeListNode>();
        internal object Parent
        {
            get { return _parent; }
            set
            {
                _parent = value;

                if (_parent.GetType().Name == "CustomTreeListNode")
                {
                    CustomTreeListNode _parent2 = (CustomTreeListNode) _parent;
                    Level = _parent2.Level + 1;

                    if (_tree != null)
                        Tree = _parent2._tree;

                    foreach (CustomTreeListNode node in Nodes)
                    {
                        node.Parent = this;
                    }

                    Height = 0;
                }
                else
                {
                    CustomTreeListControl parent2 = (CustomTreeListControl) _parent;
                    Tree = parent2;
                    Level = 0;
                }
            }
        }
        internal CustomTreeListControl Tree
        {
            get { return _tree; }
            set
            {
                if (_tree == null)
                {
                    for (int i = 0; i < Nodes.Count; i++)
                    {
                        CustomTreeListNode node = Nodes[i];
                        value.Children.Add(node);
                        node.Tree = value;
                    }
                }
                _tree = value;

            }
        }
        public string Text
        {
            get { return _text; }
            set
            {
                _text = value;
                Redraw();
            }
        }
        public ImageSource IconSource { get; set; } = null;
        public bool Bold
        {
            set
            {
                _bold = value;
                Label label = (Label)LogicalTreeHelper.FindLogicalNode(this, "lblTextLabel");
                label.FontWeight = value ? FontWeights.Medium : FontWeights.Regular;
            }
        }
        public bool Opened { get; private set; } = false;
        public bool HasChildren { get; set; }
        public object ChildObject { get; set; }
        #endregion

        #region Constructors
        public CustomTreeListNode()
        {
            Width = double.NaN;
            Height = _height;
            Background = new SolidColorBrush(ColorScheme.GlobalWhite);

            MouseEnter += CustomTreeListNode_MouseEnter;
            MouseLeave += CustomTreeListNode_MouseLeave;

            HandleContextMenu();
        }

        //public CustomTreeListNode(string text)
        //{
        //    Width = double.NaN;
        //    Height = _height;
        //    Background = new SolidColorBrush(ColorScheme.GlobalWhite);
        //    _text = text;
        //    Redraw();

        //    MouseEnter += CustomTreeListNode_MouseEnter;
        //    MouseLeave += CustomTreeListNode_MouseLeave;
        //}

        public CustomTreeListNode(string text, ImageSource image)
        {
            Width = double.NaN;
            Height = _height;
            Background = new SolidColorBrush(ColorScheme.GlobalWhite);
            _text = text;
            IconSource = image;
            Redraw();

            MouseEnter += CustomTreeListNode_MouseEnter;
            MouseLeave += CustomTreeListNode_MouseLeave;

            HandleContextMenu();
        }
        #endregion

        #region Events
        private void Expand_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            StackPanel image = (StackPanel)sender;
            image.MouseLeftButtonUp += Expand_MouseUp;
        }

        private void Expand_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            StackPanel imageContainer = (StackPanel)sender;
            CustomTreeListNode node = (CustomTreeListNode)imageContainer.Parent;

            if (node.HasChildren)
            {
                node.Opened = !node.Opened;

                foreach (CustomTreeListNode node2 in Nodes)
                {
                    if (node.Opened)
                    {
                        Image image = (Image)imageContainer.Children[0];
                        RotateTransform trans = new RotateTransform(0);
                        image.RenderTransform = trans;
                        node2.Open();
                    }
                    else
                    {
                        Image image = (Image)imageContainer.Children[0];
                        RotateTransform trans = new RotateTransform(-90);
                        image.RenderTransform = trans;
                        node2.Close();
                    }
                }
            }
            else
            {
                System.Windows.MessageBox.Show("changed");
            }

            Visibility = Visibility.Visible;
            imageContainer.MouseLeftButtonUp -= Expand_MouseUp;
        }

        private void CustomTreeListNode_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CustomTreeListNode node = (CustomTreeListNode)sender;
            LogicalTreeHelper.FindLogicalNode(node, "btnExpand");
            Image image = (Image)LogicalTreeHelper.FindLogicalNode(node, "btnExpand");

            if (image != null)
            {
                if (HasChildren)
                {
                    image.Source =
                        ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/expand_down_active.png"));
                }
            }

            node.Background = new SolidColorBrush(ColorScheme.MenuDarker);
        }

        private void CustomTreeListNode_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            CustomTreeListNode node = (CustomTreeListNode)sender;
            Image image = (Image)LogicalTreeHelper.FindLogicalNode(node, "btnExpand");

            if (image != null)
            {
                if (HasChildren)
                {
                    image.Source =
                        ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/expand_down_unactive.png"));
                }
            }

            node.Background = new SolidColorBrush(ColorScheme.MenuLight);
        }

        private void CustomTreeListNode_LeftMouseButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            StackPanel properNodeContainer = (StackPanel)sender;
            properNodeContainer.MouseLeftButtonUp += CustomTreeListNode_LeftMouseButtonUp;
        }

        private void CustomTreeListNode_LeftMouseButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            StackPanel properNodeContainer = (StackPanel)sender;
            Tree.SelectedNode = this;
            properNodeContainer.MouseLeftButtonUp -= CustomTreeListNode_LeftMouseButtonUp;
        }
        #endregion

        #region Methods

        private void HandleContextMenu()
        {
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

            MenuItem lockItem = new MenuItem()
            {
                Header = "Zablokuj",
                Icon =
                    new Image()
                    {
                        Source =
                            ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/lock_context.png"))
                    }
            };
            //renameItem.Click += RenameItem_Click;
            contextMenu.Items.Add(lockItem);

            MenuItem unlockItem = new MenuItem()
            {
                Header = "Odblokuj",
                Icon =
        new Image()
        {
            Source =
                ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/unlock_context.png"))
        }
            };
            //renameItem.Click += RenameItem_Click;
            contextMenu.Items.Add(unlockItem);

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

        public void AddNode(CustomTreeListNode node)
        {
            node.Parent = this;
            Nodes.Add(node);

            if (_tree != null)
                _tree.Children.Insert(Children.Count, node);

            HasChildren = true;
            Redraw();
        }

        public CustomTreeListNode AddNode(string text)
        {
            CustomTreeListNode node = new CustomTreeListNode();
            node.Text = text;
            node.Parent = this;
            Nodes.Add(node);
            if (_tree != null)
                _tree.Children.Insert(Children.Count, node);

            HasChildren = true;
            Redraw();

            return node;
        }

        public CustomTreeListNode AddNode(string text, ImageSource image)
        {
            CustomTreeListNode node = new CustomTreeListNode();
            node.IconSource = image;
            node.Text = text;
            node.Parent = this;
            Nodes.Add(node);
            if (_tree != null)
                _tree.Children.Insert(Children.Count, node);

            HasChildren = true;
            Redraw();

            return node;
        }

        private void Open()
        {
            if (Opened)
            {
                foreach (CustomTreeListNode node in Nodes)
                {
                    node.Open();
                }
            }

            DoubleAnimation animIn = new DoubleAnimation();
            animIn.Duration = TimeSpan.FromSeconds(0.05);

            animIn.From = 0;
            animIn.To = _height;

            Storyboard.SetTarget(animIn, this);
            Storyboard.SetTargetProperty(animIn, new PropertyPath(StackPanel.HeightProperty));

            var sb = new Storyboard();
            sb.Children.Add(animIn);

            sb.Begin();
        }

        private void Close()
        {
            foreach (CustomTreeListNode node in Nodes)
            {
                    node.Close();
            }

                DoubleAnimation animOut = new DoubleAnimation();
                animOut.Duration = TimeSpan.FromSeconds(0.05);

                animOut.From = Height;
                animOut.To = 0;

                Storyboard.SetTarget(animOut, this);
                Storyboard.SetTargetProperty(animOut, new PropertyPath(StackPanel.HeightProperty));

                var sb = new Storyboard();
                sb.Children.Add(animOut);

                sb.Begin();
        }

        private void Redraw()
        {
            LastChildFill = true;
            Width = double.NaN;
            Background = new SolidColorBrush(ColorScheme.MenuLight);

            StackPanel marginHolder = new StackPanel()
            {
                Name = "marginHolder",
                Background = new SolidColorBrush(Colors.Transparent),
                Width = _level * _indent
            };

            StackPanel expandImageContainer = new StackPanel()
            {
                Height = _height,
                Width = _height,
                Margin = new Thickness(0),
                Background = new SolidColorBrush(Colors.Transparent)
            };
            DockPanel.SetDock(expandImageContainer, Dock.Left);

            Image expandImage = new Image()
            {
                Name = "btnExpand",
                Height = _height - 10,
                Width = _height - 10,
                Source = ImageHelper.UriToImageSource(new Uri(@"pack://application:,,,/resources/expand_down_unactive.png")),
                Margin = new Thickness(5,5,0,5),
                RenderTransformOrigin = new Point(0.5,0.5)                
            };

            expandImageContainer.Children.Add(expandImage);

            RotateTransform trans = new RotateTransform(-90);
            expandImage.RenderTransform = trans;

            StackPanel ProperNodeContainer = new StackPanel()
            {
                Height = _height,
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                Margin = new Thickness(0),
                Background = new SolidColorBrush(Colors.Transparent)
            };
            ProperNodeContainer.MouseLeftButtonDown += CustomTreeListNode_LeftMouseButtonDown;

            Image image = new Image()
            {
                Height = _height - 6,
                Width = _height - 6,
                Source = IconSource,
                Margin = new Thickness(5, 3, 0, 3),
                VerticalAlignment = System.Windows.VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };

            Label text = new Label()
            {
                Name = "lblTextLabel",
                Height = _height,
                Width = double.NaN,
                Padding = new Thickness(5, 0, 0, 0),
                FontSize = 12,
                Content = _text,
                Margin = new Thickness(0),
                VerticalContentAlignment = System.Windows.VerticalAlignment.Center
            };

            text.FontWeight = _bold ? FontWeights.Medium : FontWeights.Regular;

            Children.Clear();

            Children.Add(marginHolder);

            if (!HasChildren)
            {
                expandImage.Source = null;
            }
            else
            {
                expandImageContainer.MouseLeftButtonDown += Expand_MouseDown;
            }

            Children.Add(expandImageContainer);

            if (image.Source != null)
                ProperNodeContainer.Children.Add(image);

            ProperNodeContainer.Children.Add(text);

            Children.Add(ProperNodeContainer);
        }
        #endregion
    }

    public class CustomTreeListSelectedNodeChangedEventArgs : EventArgs
    {
        public CustomTreeListNode SelectedNode { get; set; }
    }
}
