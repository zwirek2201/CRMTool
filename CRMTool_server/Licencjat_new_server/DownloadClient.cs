using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Licencjat_new_server
{
    public class DownloadClient
    {
        public Program program
        {
            get;
            set;
        }

        private TcpClient _client;
        private BinaryReader _reader;
        private BinaryWriter _writer;
        private Stream _stream;

        public delegate void UserLogStateChangedEventHandler(object sender);

        public DownloadClient(TcpClient client, string userId)
        {
            _client = client;
            Logger.Log("New connection");

            Thread thread = new Thread(ClientSetup);
            thread.Start();
        }

        private void ClientSetup()
        {
            _stream = _client.GetStream();

            _reader = new BinaryReader(_stream, Encoding.UTF8);
            _writer = new BinaryWriter(_stream, Encoding.UTF8);

            Receiver();
        }

        private void Receiver()
        {
            try
            {
                while (_client.Client.Connected)
                {
                    byte response = _reader.ReadByte();
                    if (response == MessageDictionary.DownloadFile)
                    {
                        Logger.Log("Downloading file");
                        _writer.Write(MessageDictionary.OK);
                        string fileId = _reader.ReadString();

                        byte[] fileData = DBApi.GetFileData(fileId);

                        SendFile(fileData);
                    }
                    else
                    {

                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        private byte[] ReceiveFile()
        {
            byte[] buffer = new byte[1024 * 8];
            Int64 length = _reader.ReadInt64();
            Int64 receivedBytes = 0;
            int count;

            List<byte> file = new List<byte>();

            _writer.Write(MessageDictionary.OK);
            while (receivedBytes < length && (count = _reader.Read(buffer, 0, buffer.Length)) > 0)
            {
                Logger.Log("DownloadingFile file (" + receivedBytes + "/" + length + ")");
                file.AddRange(buffer);
                receivedBytes += count;
            }

            Logger.Log("File downloaded");
            return file.ToArray();
        }

        private void SendFile(byte[] data)
        {
            try
            {
                int receivedBytes = 0;
                MemoryStream stream = new MemoryStream();
                stream.Write(data, 0, data.Length);

                _writer.Write((Int64) data.Length);
                if (_reader.Read() == MessageDictionary.OK)
                {
                    stream.Position = 0;
                    stream.Seek(0, SeekOrigin.Begin);
                    var buffer = new byte[1024*8];
                    int count;
                    while ((count = stream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        Logger.Log("Downloading file (" + receivedBytes + "/" + stream.Length + ")");
                        _writer.Write(buffer, 0, count);
                        receivedBytes += count;
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
