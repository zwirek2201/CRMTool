using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Licencjat_new.CustomClasses;
using Licencjat_new.Windows;
using Licencjat_new.Windows.HelperWindows;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Licencjat_new.Server
{
    public class UploadClient
    {
        #region Variables

        private MainWindow _parent;

        private Thread _connectionThread;
        private TcpClient _client;
        private NetworkStream _stream;
        private BinaryReader _reader;
        private BinaryWriter _writer;
        private readonly string _server = Properties.Settings.Default.ServerIP;
        private int _port = Properties.Settings.Default.ServerPort;
        private bool _isConnected = false;
        private string _userId;
        public bool IsBusy { get; private set;} = false;

        public List<FileModel> UploadQueue { get; set; } = new List<FileModel>();

        public event EventHandler ConnectionSuccess;
        public event EventHandler ConnectionFailed;
        public event EventHandler<FileUploadedEventArgs> FileUploaded;
        #endregion

        #region Constructors

        public UploadClient(string userId, MainWindow parent)
        {
            _parent = parent;
            _userId = userId;
            Connect();
        }

        #endregion

        #region Methods

        public void Connect()
        {
            if (_connectionThread == null)
            {
                _connectionThread = new Thread(Setup);
                _connectionThread.IsBackground = true;
                _connectionThread.SetApartmentState(ApartmentState.STA);
                _connectionThread.Start();
            }
        }

        private void Setup()
        {
            try
            {
                _client = new TcpClient(_server, _port);
                _stream = _client.GetStream();

                _reader = new BinaryReader(_stream);
                _writer = new BinaryWriter(_stream);

                int result = _reader.ReadByte();
                if (result == MessageDictionary.Hello)
                {
                    _writer.Write(MessageDictionary.Hello);
                    _writer.Write(MessageDictionary.ImUploadClient);
                    _writer.Write(_userId);

                    if (_reader.Read() == MessageDictionary.OK)
                    {
                        _isConnected = true;

                        ConnectionSuccess?.Invoke(this, EventArgs.Empty);
                    }
                }
                else
                {
                    _isConnected = false;
                    ConnectionFailed?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                ConnectionFailed?.Invoke(this, EventArgs.Empty);
            }
        }

        public void UploadFile(ConversationMessageModel message, FileModel file)
        {
            try
            {
                IsBusy = true;
                _writer.Write(MessageDictionary.UploadFile);
                if (_reader.ReadByte() == MessageDictionary.OK)
                {
                    _writer.Write(file.Name);
                    _writer.Write(file.ContentType.MediaType);
                    _writer.Write(file.DateAdded.ToString("dd-MM-yyyy"));

                    SendFile(new MemoryStream(file.Data));

                    string fileId = _reader.ReadString();

                    file.Id = fileId;

                    FileUploaded?.Invoke(this, new FileUploadedEventArgs() {Message = message, File = file});
                }
                IsBusy = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                ErrorHelper.LogError(ex);
                Logout();
            }
        }

        private void Logout()
        {
            _parent.Dispatcher.Invoke(() =>
            {
                CustomMessageBox messageBox =
                    new CustomMessageBox(
                        "Wystąpił problem podczas połączenia z serwerem. Aplikacja zostanie zrestartowana.",
                        MessageBoxButton.OK);

                messageBox.OKButtonClicked += (s, ea) =>
                {
                    _parent.Darkened = false;
                    _parent.mainCanvas.Children.Remove(messageBox);
                    _parent.Logout();
                };

                _parent.Darkened = true;
                _parent.mainCanvas.Children.Add(messageBox);
            });
        }

        public void UploadFiles(ConversationMessageModel message, List<FileModel> files)
        {
            foreach (FileModel file in files)
            {
                UploadFile(message, file);
            }
        }

        public byte[] ReceiveFile()
        {
            byte[] buffer = new byte[1024*8];
            Int64 length = _reader.ReadInt64();
            Int64 receivedBytes = 0;
            int count;

            List<byte> file = new List<byte>();

            _writer.Write(MessageDictionary.OK);
            while (receivedBytes < length && (count = _reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                file.AddRange(buffer);
                receivedBytes += count;
            }

            return file.ToArray();
        }

        public void SendFile(Stream data)
        {
            _writer.Write(data.Length);
            if (_reader.Read() == MessageDictionary.OK)
            {
                data.Position = 0;
                data.Seek(0, SeekOrigin.Begin);
                var buffer = new byte[1024 * 8];
                int count;
                while ((count = data.Read(buffer, 0, buffer.Length)) > 0)
                {
                    if (count != 8192)
                    {
                        
                    }
                    _writer.Write(buffer, 0, count);
                }
            }
        }
        #endregion
    }

    public class DownloadClient
    {
        #region Variables

        private MainWindow _parent;

        private Thread _connectionThread;
        private TcpClient _client;
        private NetworkStream _stream;
        private BinaryReader _reader;
        private BinaryWriter _writer;
        private readonly string _server = Properties.Settings.Default.ServerIP;
        private int _port = Properties.Settings.Default.ServerPort;
        private bool _isConnected = false;
        private string _userId;

        public event EventHandler connectionSuccess;
        public event EventHandler connectionFailed;
        public event EventHandler<FileDownloadedEventArgs> FileDownloaded;
        #endregion

        #region Properties
        public bool IsBusy { get; set; }

        public List<FileModel> DownloadQueue { get; set; } = new List<FileModel>();

        #endregion;

        #region Constructors
        public DownloadClient(string userId, MainWindow parent)
        {
            _parent = parent;
            _userId = userId;
            Connect();
        }
        #endregion

        #region Methods
        public void Connect()
        {
            if (_connectionThread == null)
            {
                _connectionThread = new Thread(Setup);
                _connectionThread.IsBackground = true;
                _connectionThread.SetApartmentState(ApartmentState.STA);
                _connectionThread.Start();
            }
        }

        private void Setup()
        {
            try
            {
                _client = new TcpClient(_server, _port);
                _stream = _client.GetStream();

                _reader = new BinaryReader(_stream);
                _writer = new BinaryWriter(_stream);

                int result = _reader.ReadByte();
                if (result == MessageDictionary.Hello)
                {
                    _writer.Write(MessageDictionary.Hello);
                    _writer.Write(MessageDictionary.ImDownloadClient);
                    _writer.Write(_userId);

                    if (_reader.Read() == MessageDictionary.OK)
                    {
                        _isConnected = true;

                        connectionSuccess?.Invoke(this, EventArgs.Empty);

                        CheckQueue();
                    }
                }
                else
                {
                    _isConnected = false;
                    connectionFailed?.Invoke(this, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                connectionFailed?.Invoke(this, EventArgs.Empty);
            }
        }

        private void CheckQueue()
        {
            try
            {
                while (_isConnected)
                {
                    if (DownloadQueue.Count > 0)
                    {
                        FileModel downloadedFile = DownloadQueue.First();
                        if (!downloadedFile.Downloaded)
                            DownloadFile(downloadedFile);
                        else
                            FileDownloaded?.Invoke(this, new FileDownloadedEventArgs() { File = downloadedFile });
                        DownloadQueue.Remove(downloadedFile);
                    }
                }
            }
            catch (Exception ex)
            {
                _parent.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                    ErrorHelper.LogError(ex);
                    Logout();
                });
            }
        }

        private void Logout()
        {
            _parent.Dispatcher.Invoke(() =>
            {
                CustomMessageBox messageBox =
                    new CustomMessageBox(
                        "Wystąpił problem podczas połączenia z serwerem. Aplikacja zostanie zrestartowana.",
                        MessageBoxButton.OK);

                messageBox.OKButtonClicked += (s, ea) =>
                {
                    _parent.Darkened = false;
                    _parent.mainCanvas.Children.Remove(messageBox);
                    _parent.Logout();
                };

                _parent.Darkened = true;
                _parent.mainCanvas.Children.Add(messageBox);
            });
        }

        public void DownloadFile(FileModel file)
        {
            try
            {
                IsBusy = true;
                _writer.Write(MessageDictionary.DownloadFile);
                if (_reader.ReadByte() == MessageDictionary.OK)
                {
                    _writer.Write(file.Id);

                    byte[] fileData = ReceiveFile();

                    file.Data = fileData;

                    file.Downloaded = true;

                    FileDownloaded?.Invoke(this, new FileDownloadedEventArgs() {File = file});
                }
                IsBusy = false;
            }
            catch (Exception ex)
            {
                _parent.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show(ex.Message + Environment.NewLine + ex.StackTrace);
                    ErrorHelper.LogError(ex);
                    Logout();
                });
            }
        }

        public byte[] ReceiveFile()
        {
            byte[] buffer = new byte[1024 * 8];
            Int64 length = _reader.ReadInt64();
            Int64 receivedBytes = 0;
            int count;

            List<byte> file = new List<byte>();

            _writer.Write(MessageDictionary.OK);
            while (receivedBytes < length && (count = _reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                file.AddRange(buffer.Take(count));
                receivedBytes += count;
            }

            return file.ToArray();
        }
        #endregion
    }

    public class FileUploadedEventArgs
    {
        public ConversationMessageModel Message { get; set; }
        public FileModel File { get; set; }
    }

    public class FileDownloadedEventArgs
    {
        public FileModel File { get; set; }
    }
}
