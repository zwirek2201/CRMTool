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
using Licencjat_new.Windows.HelperWindows;

namespace Licencjat_new.Windows
{
    /// <summary>
    /// Interaction logic for Document.xaml
    /// </summary>
    public partial class Document : UserControl
    {
        #region Variables
        private MainWindow _parent;

        #region Lists
        private List<ConversationModel> _conversations;
        private List<FileModel> _files;
        #endregion
        #endregion

        #region Properties
        public ConversationModel SelectedConversation { get; set; }
        public bool WindowInitialized { get; private set; }
        #endregion

        #region Constructors
        public Document()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialization
        public void Init()
        {
            _parent = (MainWindow)Window.GetWindow(this);

            ConversationList.DisplayItemContextMenus = false;
            FileList.RenameFile += FileList_RenameFile;
            FileList.DownloadFile += FileList_DownloadFile;

            FileSortButton sortButton = (FileSortButton)LogicalTreeHelper.FindLogicalNode(this, "FileSortButton");
            FileList.BoundSortButton = sortButton;

            if (_parent != null && _parent.Conversations == null)
            {
                _parent.ConversationWorker.RunWorkerCompleted += ConversationWorker_RunWorkerCompleted;
            }
            else
            {
                if (_parent?.Conversations != null)
                {
                    _conversations = _parent.Conversations;
                    LoadConversations();
                }
            }

            if (_parent != null && _parent.Files == null)
            {
                _parent.FileWorker.RunWorkerCompleted += FileWorker_RunWorkerCompleted;
            }
            else
            {
                if (_parent?.Files != null)
                {
                    _files = _parent.Files;
                    LoadFiles();
                }
            }

            _parent.NotificationClient.NewFilesArrived += NotificationClient_NewFilesArrived;

            WindowInitialized = true;
        }

        private void NotificationClient_NewFilesArrived(object sender, Server.NewFilesArrivedEventArgs e)
        {
            this.Dispatcher.Invoke(() =>
            {
                List<FileModel> filesToAdd = new List<FileModel>();
                foreach (FileModel file in e.Files)
                {
                    FileModel newFile =
                        _parent.Files
                            .FirstOrDefault(obj => obj.Id == file.Id && obj.ConversationId == file.ConversationId);
                    if(newFile == null)
                    {
                        filesToAdd.Add(file);
                    }
                }
                FileList.AddFiles(filesToAdd);

                //FileList.Sort();
            });
        }

        private void FileWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            _files = _parent?.Files;
            LoadFiles();
        }
        #endregion

        #region Events
        private void ConversationWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            _conversations = _parent?.Conversations;
            LoadConversations();
        }

        private void ConversationList_SelectedConversationChanged(object sender, SelectedConversationChangedEventArgs e)
        {
            SelectedConversation = e.Conversation;
            FileList.ClearFiles();
            FileList.AddFiles(_files.Where(obj => obj.ConversationId == e.Conversation.Id).ToList());
            FileList.Sort();
        }

        private void FileList_RenameFile(object sender, EventArgs e)
        {
            FileListItem item = (FileListItem)sender;

            _parent.Darkened = true;

            Rename renameWindow = new Rename();

            renameWindow.CancelButtonClicked += (o, args) =>
            {
                _parent.mainCanvas.Children.Remove(renameWindow);
                _parent.Darkened = false;
            };

            renameWindow.ReadyButtonClicked += (s, ev) =>
            {
                try
                {
                    renameWindow.LoadingOn = true;

                    _parent.Client.RenameFile(item.File.Id, item.File.Name,
                        renameWindow.NewName + item.File.Name.Substring(item.File.Name.LastIndexOf('.')));

                    renameWindow.LoadingOn = false;

                    _parent.mainCanvas.Children.Remove(renameWindow);
                    _parent.Darkened = false;
                }
                catch (Exception ex)
                {
                    
                }
            };

            _parent.mainCanvas.Children.Add(renameWindow);
        }

        private void FileList_DownloadFile(object sender, EventArgs e)
        {
            FileListItem fileItem = (FileListItem)sender;

            if (fileItem.File.Data == null)
                _parent.DownloadClient.DownloadFile(fileItem.File);

            DownloadHelper.DownloadFile(fileItem.File);
        }
        #endregion

        #region Methods
        private void LoadConversations()
        {
            ConversationList.Clear();
            ConversationList.AddConversations(_conversations);

            ConversationList.SelectedConversationChanged += ConversationList_SelectedConversationChanged;
        }

        private void LoadFiles()
        {
            if (ConversationList.Conversations.Any())
                ConversationList.SelectedConversationItem = ConversationList.Conversations.First();
        }
        #endregion
    }
}
