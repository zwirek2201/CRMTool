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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Licencjat_new.Controls;
using Licencjat_new.CustomClasses;

namespace Licencjat_new.Windows.HelperWindows
{
    /// <summary>
    /// Interaction logic for NewPhoneConversationMessage.xaml
    /// </summary>
    public partial class NewPhoneConversationMessage : UserControl
    {
        private bool _darkened;
        private MainWindow _parent;
        private ConversationModel _conversation;

        public event EventHandler ReadyButtonClicked;
        public event EventHandler CancelButtonClicked;

        public PhoneMessageType MessageType { get; set; } = PhoneMessageType.MadeCall;

        public PersonModel SelectedPerson { get; private set; }
        public PhoneNumberModel SelectedPhoneNumber { get; private set; }
        public string Message { get; set; } = "";

        public bool CallAnswered { get; private set; }


        public List<FileModel> Attachments { get; set; } = new List<FileModel>();
        public List<FileModel> FilesAwaiting { get; set; } = new List<FileModel>();


        public bool Darkened
        {
            get { return _darkened; }
            set
            {
                _darkened = value;
                if (Darkened)
                {
                    Darkener.Visibility = Visibility.Visible;

                    DoubleAnimation fadeInAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
                    Darkener.BeginAnimation(OpacityProperty, fadeInAnimation);
                }
                else
                {
                    DoubleAnimation fadeOutAnimation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
                    fadeOutAnimation.Completed += (s, e) => { Darkener.Visibility = Visibility.Collapsed; };
                    Darkener.BeginAnimation(OpacityProperty, fadeOutAnimation);
                }
            }
        }

        public NewPhoneConversationMessage(MainWindow parent, ConversationModel conversation)
        {
            _parent = parent;
            _conversation = conversation;

            InitializeComponent();

            CallAnsweredCheckBox.Selected = true;

            MessageTypeComboBox.AddItem("Rozmowa wykonana");
            MessageTypeComboBox.AddItem("Rozmowa odebrana");

            chooseSender.Text = "Wybierz odbiorcę";

            MessageTypeComboBox.SelectedItem = MessageTypeComboBox.Items[0];

            MessageTypeComboBox.SelectedItemChanged += MessageTypeComboBox_SelectedItemChanged;

            chooseSender.Clicked += ChooseSender_Clicked;

            ReadyButton.Clicked += (s, ea) =>
            {
                Message = messageBox.Text;
                CallAnswered = CallAnsweredCheckBox.Selected;
                ReadyButtonClicked?.Invoke(this, EventArgs.Empty);
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
                listItem.AllowShowDetails = false;
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

        private void MessageTypeComboBox_SelectedItemChanged(object sender, EventArgs e)
        {
            switch (MessageTypeComboBox.Items.IndexOf(MessageTypeComboBox.SelectedItem))
            {
                case 0:
                    chooseSender.Text = "Wybierz nadawcę";
                    MessageType = PhoneMessageType.ReceivedCall;
                    break;
                case 1:
                    chooseSender.Text = "Wybierz odbiorcę";
                    MessageType = PhoneMessageType.MadeCall;
                    break;
            }
        }

        private void ChooseSender_Clicked(object sender, EventArgs e)
        {
            Darkened = true;
            ChoosePerson choose = new ChoosePerson(_parent, _parent.Persons.Where(obj => !_conversation.Members.Contains(obj) || obj.Company == null).ToList(), false, ChoosePersonMode.ChoosePhoneNumber);
            _parent.mainCanvas.Children.Add(choose);

            choose.ReadyButtonClicked += Choose_ReadyButtonClicked;

            choose.CancelButtonClicked += (s, ea) =>
            {
                _parent.mainCanvas.Children.Remove(choose);
                Darkened = false;
                senderLabel.Content = "";
            };
        }

        private void Choose_ReadyButtonClicked(object sender, EventArgs e)
        {
            ChoosePerson choose = (ChoosePerson)sender;

            PersonModel selectedPerson = choose.SelectedPersons.First();
            SelectedPerson = selectedPerson;
            senderLabel.Content = selectedPerson.FullName;

            _parent.mainCanvas.Children.Remove(choose);

            AdditionalChoose addChoose = new AdditionalChoose(selectedPerson.PhoneNumbers.Cast<object>().ToList(), "Wybierz numer telefonu");

            addChoose.ReadyButtonClicked += (s2, ea2) =>
            {
                PhoneNumberModel selectedNumber = (PhoneNumberModel)addChoose.SelectedItems[0];
                SelectedPhoneNumber = selectedNumber;
                senderLabel.Content += " (" + selectedNumber.Number + ")";
                _parent.mainCanvas.Children.Remove(addChoose);
                Darkened = false;
            };

            addChoose.CancelButtonClicked += (s2, ea2) =>
            {
                _parent.mainCanvas.Children.Remove(addChoose);
                Darkened = false;
                senderLabel.Content = "";
            };

            _parent.mainCanvas.Children.Add(addChoose);
        }
    }

    public enum PhoneMessageType
    {
        ReceivedCall,
        MadeCall
    }
}
