using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ImapX;
using Licencjat_new.Server;

namespace Licencjat_new.CustomClasses
{
    public class FileModel
    {
        #region Variables

        private string _name;
        private byte[] _data;

        public event EventHandler DataChanged;
        #endregion

        #region Properties
        public string Id { get; set; }

        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                DataChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        public ContentType ContentType { get; set; }
        public long Size { get; set; }
        public string ConversationId { get; set; }
        public DateTime DateAdded { get; set; }
        public ImageSource Icon { get; set; }

        public byte[] Data
        {
            get { return _data; }
            set
            {
                _data = value;
                if (Data != null)
                    Downloaded = true;
            }
        }

        public bool Downloaded { get; set; }
        #endregion

        #region Constructors

        public FileModel(string id, string name, ContentType contentType, long size, DateTime dateAdded, byte[] data)
        {
            Id = id;
            Name = name;
            ContentType = contentType;
            Size = size;
            DateAdded = dateAdded;
            Data = data;
            Downloaded = true;
        }

        public FileModel(string id, string name, ContentType contentType, long size, DateTime dateAdded)
        {
            Id = id;
            Name = name;
            ContentType = contentType;
            Size = size;
            DateAdded = dateAdded;
        }

        public FileModel(string name, ContentType contentType, long size, DateTime dateAdded, byte[] data)
        {
            Name = name;
            ContentType = contentType;
            Size = size;
            DateAdded = dateAdded;
            Data = data;
        }

        public FileModel(Attachment attachment, DateTime dateAdded)
        {
            Name = attachment.FileName;
            ContentType = attachment.ContentType;
            Size = attachment.FileSize;
            DateAdded = dateAdded;
            Data = attachment.FileData;
        }

        public FileModel(string fileLocation)
        {
            if (File.Exists(fileLocation))
            {
                Data = File.ReadAllBytes(fileLocation);
                Downloaded = true;

                Icon = FileHelper.GetFileIcon(Path.GetExtension(fileLocation).Substring(1));
                Name = Path.GetFileName(fileLocation);
                ContentType = new ContentType(MimeTypes.MimeTypeMap.GetMimeType(Path.GetExtension(fileLocation)));
                Size = Data.Length;
                DateAdded = DateTime.Now;
            }
        }
        #endregion

        #region Methods

        #endregion
    }
}
