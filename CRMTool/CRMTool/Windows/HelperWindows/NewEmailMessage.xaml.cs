using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    public partial class NewEmailMessage : UserControl
    {

        public event EventHandler ReadyButtonClicked;
        public event EventHandler CancelButtonClicked;

        private MainWindow _parent;


        public string OutputSubject { get; set; }
        public string Message { get; set; }
        public string SendingAddress { get; set; }
        public List<FileModel> Attachments { get; set; } = new List<FileModel>();
        public List<FileModel> FilesAwaiting { get; set; } = new List<FileModel>();

        public NewEmailMessage(ConversationModel conversation, MainWindow parent, string subject = "")
        {
            InitializeComponent();

            _parent = parent;

            if (_parent.EmailClients != null && _parent.EmailClients.Count > 0)
            {
                foreach (EmailModel email in _parent.EmailClients)
                {
                    EmailComboBox.AddItem(email.Address, email.Login != "" && !email.CannotConnect);
                }

                if (EmailComboBox.Items.Any(obj => obj.Enabled))
                    EmailComboBox.SelectedItem = EmailComboBox.Items.First(obj => obj.Enabled);

            }
            else
            {
                EmailComboBox.AddItem("Brak dostępnych adresów e-mail",false);
            }

            visibleIdLabel.Content = "(" + conversation.VisibleId + ")";
            titleBox.Text = subject;
            messageBox.Text = "";

            Loaded += (s, ea) =>
            {
                titleBox.Focusable = true;
                titleBox.Focus();
                Keyboard.Focus(titleBox);
            };

            ReadyButton.Clicked += (s, ea) =>
            {
                if (_parent.EmailClients != null && _parent.EmailClients.Count > 0 && EmailComboBox.SelectedItem != null)
                {
                    OutputSubject =
                        "(" + conversation.VisibleId + ")" + titleBox.Text;
                    SendingAddress = EmailComboBox.SelectedItem.Caption;
                    Message = messageBox.Text;
                    ReadyButtonClicked?.Invoke(this, EventArgs.Empty);
                }
            };

            CancelButton.Clicked += (s, ea) =>
            {
                CancelButtonClicked?.Invoke(this, EventArgs.Empty);
            };

            AddFileFromDisk.Clicked += AddFileFromDisk_Clicked;
            AddExistingFile.Clicked += AddExistingFile_Clicked;
            AddFileFromScanner.Clicked += AddFileFromScanner_Clicked;

            _parent.DownloadClient.FileDownloaded += DownloadClient_FileDownloaded;
        }

        private void DownloadClient_FileDownloaded(object sender, Server.FileDownloadedEventArgs e)
        {
            if (FilesAwaiting.Any())
            {
                if (FilesAwaiting.Select(obj => obj.Id).Contains(e.File.Id))
                {
                    AddFile(e.File);
                }
            }
        }

        private void AddFileFromScanner_Clicked(object sender, EventArgs e)
        {
            Scanning scan = new Scanning();
            _parent.mainCanvas.Children.Add(scan);

        }

        private void AddExistingFile_Clicked(object sender, EventArgs e)
        {
            ChooseFiles chooseFiles = new ChooseFiles(_parent.Conversations, _parent.Files);

            chooseFiles.CancelButtonClicked += (s, ea) =>
            {
                _parent.mainCanvas.Children.Remove(chooseFiles);
            };

            chooseFiles.ReadyButtonClicked += (s, ea) =>
            {
                AddFiles(chooseFiles.SelectedFiles);

                _parent.mainCanvas.Children.Remove(chooseFiles);

                FileListContainer.Visibility = Visibility.Visible;

            };

            _parent.mainCanvas.Children.Add(chooseFiles);
        }

        private void AddFileFromDisk_Clicked(object sender, EventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.Multiselect = true;
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                FileListContainer.Visibility = Visibility.Visible;

                for (int i = 0; i < dialog.FileNames.Count(); i++)
                {
                    string filePath = dialog.FileNames[i];
                    string fileName = dialog.SafeFileNames[i];

                    FileModel file = new FileModel(filePath);
                    AddFile(file);
                }
            }
        }

        private void AddFiles(List<FileModel> files)
        {
            foreach (FileModel file in files)
            {
                AddFile(file);
            }
        }

        private void AddFile(FileModel file)
        {
            if (file.Downloaded != true)
            {
                FilesAwaiting.Add(file);
                _parent.DownloadClient.DownloadQueue.Add(file);
                return;
            }

            Attachments.Add(file);

            this.Dispatcher.Invoke(() =>
            {
                FileListItem listItem = new FileListItem(file);
                listItem.AllowDownload = false;
                listItem.AllowRename = false;
                listItem.RemoveFile += ListItem_RemoveFile;

                FileList.Children.Add(listItem);
            });
        }

        private void ListItem_RemoveFile(object sender, EventArgs e)
        {
            FileListItem fileItem = (FileListItem)sender;
            Attachments.Remove(fileItem.File);
            FileList.Children.Remove(fileItem);
        }
    }
}
