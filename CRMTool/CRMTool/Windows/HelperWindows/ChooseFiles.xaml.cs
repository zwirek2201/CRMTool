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
using Licencjat_new.Controls;
using Licencjat_new.CustomClasses;

namespace Licencjat_new.Windows.HelperWindows
{
    /// <summary>
    /// Interaction logic for ChooseFiles.xaml
    /// </summary>
    public partial class ChooseFiles : UserControl
    {
        public event EventHandler ReadyButtonClicked;
        public event EventHandler CancelButtonClicked;

        private List<ConversationModel> _conversations;
        private List<FileModel> _files;

        public List<FileModel> SelectedFiles = new List<FileModel>();

        public ConversationModel SelectedConversation { get; set; }

        public ChooseFiles(List<ConversationModel> conversations, List<FileModel> files)
        {
            _conversations = conversations;
            _files = files;
            InitializeComponent();

            SelectedCountLabel.Visibility = Visibility.Collapsed;

            FileSortButton sortButton = (FileSortButton)LogicalTreeHelper.FindLogicalNode(this, "FileSortButton");
            FileList.BoundSortButton = sortButton;
            FileList.AllowSelect = true;
            FileList.SelectedListChanged += FileList_SelectedListChanged;

            ReadyButton.Clicked += (s, ea) =>
            {
                SelectedFiles = FileList.SelectedFiles;
                ReadyButtonClicked?.Invoke(this, EventArgs.Empty);
            };

            CancelButton.Clicked += (s, ea) =>
            {
                CancelButtonClicked?.Invoke(this, EventArgs.Empty);
            };

            ConversationList.Clear();
            ConversationList.AddConversations(_conversations);

            ConversationList.SelectedConversationChanged += ConversationList_SelectedConversationChanged;

            if (ConversationList.Conversations.Any())
                ConversationList.SelectedConversationItem = ConversationList.Conversations.First();
        }

        private void FileList_SelectedListChanged(object sender, EventArgs e)
        {
            if (FileList.SelectedFiles.Count > 0)
            {
                SelectedCountLabel.Content = FileList.SelectedFiles.Count + " plików";
                SelectedCountLabel.Visibility = Visibility.Visible;
            }
            else
            {
                SelectedCountLabel.Visibility = Visibility.Collapsed;
            }
        }

        private void ConversationList_SelectedConversationChanged(object sender, Controls.SelectedConversationChangedEventArgs e)
        {
            SelectedConversation = e.Conversation;
            FileList.ClearFiles();
            FileList.AddFiles(_files.Where(obj => obj.ConversationId == e.Conversation.Id).ToList());
            FileList.Sort();
        }
    }
}
