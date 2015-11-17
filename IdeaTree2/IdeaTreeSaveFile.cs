using Microsoft.Practices.Prism.Mvvm;
using MonitoredUndo;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace IdeaTree2
{
    [ProtoContract(ImplicitFields = ImplicitFields.AllPublic, ImplicitFirstTag = 4)]
    public class IdeaTreeSaveFile : BindableBase, ISupportsUndo
    {
        [ProtoIgnore]
        public static string IdeaTreeSaveFileExt = ".itr";
        
        private bool changedSinceLastSave;
        [ProtoIgnore]
        public bool ChangedSinceLastSave
        {
            get { return changedSinceLastSave; }
            set { SetProperty(ref changedSinceLastSave, value); }
        }

        private bool changedSinceLastSaveExceptExpansion;
        [ProtoIgnore]
        public bool ChangedSinceLastSaveExceptExpansion
        {
            get { return changedSinceLastSaveExceptExpansion; }
            set { SetProperty(ref changedSinceLastSaveExceptExpansion, value); }
        }

        private string password;
        [ProtoIgnore]
        public string Password
        {
            get { return password; }
            set
            {
                SetProperty(ref password, value);
                ChangedSinceLastSave = true;
                ChangedSinceLastSaveExceptExpansion = true;
            }
        }
        
        [ProtoMember(1)]
        private byte[] encryptedPassword;

        [ProtoMember(2)]
        private byte[] blob;

        private string templatePath;
        public string TemplatePath
        {
            get { return templatePath; }
            set
            {
                SetProperty(ref templatePath, value);
                ChangedSinceLastSave = true;
                ChangedSinceLastSaveExceptExpansion = true;
            }
        }
        
        private IdeaTreeTemplate template;
        [ProtoIgnore]
        public IdeaTreeTemplate Template
        {
            get { return template; }
            set
            {
                SetProperty(ref template, value);
                ChangedSinceLastSave = true;
                ChangedSinceLastSaveExceptExpansion = true;
            }
        }
        
        [ProtoIgnore]
        public ObservableCollection<IdeaNote> Ideas { get; set; }
        [ProtoMember(3, OverwriteList = true)]
        private IList<IdeaNote> IdeaList
        {
            get { return Ideas; }
            set
            {
                Ideas = new ObservableCollection<IdeaNote>(value);
                Ideas.CollectionChanged += Ideas_CollectionChanged;
                foreach (IdeaNote item in Ideas)
                    item.PropertyChanged += Item_PropertyChanged;
            }
        }

        public IdeaTreeSaveFile()
        {
            Ideas = new ObservableCollection<IdeaNote>();
            Ideas.CollectionChanged += Ideas_CollectionChanged;
        }

        private void Ideas_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (IdeaNote item in e.NewItems)
                    item.PropertyChanged += Item_PropertyChanged;
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (IdeaNote item in e.OldItems)
                    item.PropertyChanged -= Item_PropertyChanged;
            }

            ChangedSinceLastSave = true;
            ChangedSinceLastSaveExceptExpansion = true;
            DefaultChangeFactory.Current.OnCollectionChanged(this, nameof(Ideas), Ideas, e);
        }

        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IdeaNote.IsSelected))
            {
                ChangedSinceLastSave = true;
                if (e.PropertyName != nameof(IdeaNote.IsExpanded))
                    ChangedSinceLastSaveExceptExpansion = true;
            }
        }

        public static IdeaTreeSaveFile FromFile(string path)
        {
            IdeaTreeSaveFile saveFile = null;
            
            using (Stream file = File.OpenRead(path))
            {
                using (var gzip = new GZipStream(file, CompressionMode.Decompress, true))
                {
                    saveFile = Serializer.Deserialize<IdeaTreeSaveFile>(gzip);
                }
            }

            return saveFile;
        }

        public void Save(string path)
        {
            using (Stream file = File.Create(path))
            {
                using (var gzip = new GZipStream(file, CompressionMode.Compress, true))
                {
                    Serializer.Serialize(gzip, this);
                }
            }

            // Clear internal flag for unsaved data.
            ChangedSinceLastSave = false;
            ChangedSinceLastSaveExceptExpansion = false;
        }

        [ProtoBeforeSerialization]
        private void BeforeSerialization()
        {
            // Encrypt plaintext password before saving.
            if (!string.IsNullOrEmpty(Password))
            {
                UnicodeEncoding byteConverter = new UnicodeEncoding();
                byte[] dataToEncrypt = byteConverter.GetBytes(Password);
                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                blob = rsa.ExportCspBlob(true);
                encryptedPassword = rsa.Encrypt(dataToEncrypt, false);
            }
        }

        [ProtoAfterDeserialization]
        private void AfterDeserialization()
        {
            foreach (var idea in Ideas) idea.RootSaveFile = this;

            if (encryptedPassword == null || encryptedPassword.Length == 0 || blob == null || blob.Length == 0) return;
            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            rsa.ImportCspBlob(blob);
            byte[] decryptedPassword = rsa.Decrypt(encryptedPassword, false);
            UnicodeEncoding byteConverter = new UnicodeEncoding();
            Password = byteConverter.GetString(decryptedPassword);
            encryptedPassword = null;
            blob = null;
        }

        public void AddIdea(IdeaNote idea)
        {
            if (idea.RootSaveFile != null) idea.RootSaveFile.Ideas.Remove(idea);
            if (idea.Parent != null)
            {
                idea.Parent.Ideas.Remove(idea);
                idea.Parent = null;
            }
            idea.RootSaveFile = this;
            Ideas.Add(idea);
        }

        public void InsertIdea(int index, IdeaNote idea)
        {
            if (idea.RootSaveFile != null) idea.RootSaveFile.Ideas.Remove(idea);
            if (idea.Parent != null)
            {
                idea.Parent.Ideas.Remove(idea);
                idea.Parent = null;
            }
            idea.RootSaveFile = this;
            Ideas.Insert(index, idea);
        }

        public object GetUndoRoot() => this;
    }
}
