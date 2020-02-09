using Newtonsoft.Json;
using Organilog.Constants;
using Organilog.Models.Response;
using SQLite;
using System;
using System.ComponentModel;
using System.IO;
using Xamarin.Forms;
using TinyMVVM;

namespace Organilog.Models
{
    [Table("Media")]
    public class Media : BaseModel
    {
        [JsonProperty("mAppId")]
        [PrimaryKey]
        public Guid Id { get; set; }

        [JsonProperty("mId")]
        public int ServerId { get; set; }

        [JsonProperty("mUserId")]
        public int UserId { get; set; }

        [JsonProperty("mAccountId")]
        public int AccountId { get; set; }

        [JsonProperty("mCode")]
        public int Code { get; set; }

        [JsonProperty("mFilePath")]
        public string FilePath { get; set; }

        [JsonProperty("mFileName")]
        public string FileName { get; set; }

        [JsonProperty("mFileSize")]
        public int FileSize { get; set; }

        [JsonProperty("mFileMime")]
        public string FileMime { get; set; }

        [JsonProperty("mYear")]
        public string Year { get; set; }

        [JsonProperty("mMonth")]
        public string Month { get; set; }

        [JsonProperty("mType")]
        public string Type { get; set; } //only for invoice 

        private string fileData;

        [JsonProperty("mFileData")]
        public string FileData { get => fileData; set => SetProperty(ref fileData, value, nameof(ImageSource), ImageUri, nameof(ImageDisplay), nameof(ImageDisplayDetail)); }


        [Ignore]
        [JsonIgnore]
        public ImageSource ImageSource => string.IsNullOrWhiteSpace(FileData) ? null : ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(FileData)));

        [Ignore]
        [JsonIgnore]
        public string ImageUri => ApiURI.URL_GET_MEDIA(Settings.CurrentAccount, Settings.CurrentUser.FkAccountId, AccountId, Year, Month, FileName);

        [Ignore]
        [JsonIgnore]
        public ImageSource ImageDisplay
        {
            get
            {
                if ((FileMime!=null && FileMime.ToLower().EndsWith("pdf", StringComparison.Ordinal))|| FileName != null && FileName.ToLower().EndsWith("pdf", StringComparison.Ordinal))
                {
                    return ImageSource.FromFile("pdf_icon.png");
                }
                if ((FileMime != null && FileMime.ToLower().EndsWith("msword", StringComparison.Ordinal)) || (FileName != null && ( FileName.ToLower().EndsWith("doc", StringComparison.Ordinal) || FileName.ToLower().EndsWith("docx", StringComparison.Ordinal))))
                {
                    return ImageSource.FromFile("msword.png");
                }

                return ImageSource ?? ImageSource.FromUri(new Uri(ImageUri));
            }
        }

        [Ignore]
        [JsonIgnore]
        public ImageSource ImageDisplayDetail
        {
            get
            {
                if (ImageSource != null)
                    return ImageSource;
                if (FileMime != null && FileMime.ToLower().EndsWith("pdf", StringComparison.Ordinal))
                {
                    return null;
                }

                return new Uri(ImageUri);
            }
        }
        //{
        //    get
        //    {
        //        if (FileMime.ToLower().EndsWith("pdf", StringComparison.Ordinal))
        //        {
        //            return ImageSource.FromFile("pdf_icon.png");
        //        }
        //        if (FileMime.ToLower().EndsWith("msword", StringComparison.Ordinal))
        //        {
        //            return ImageSource.FromFile("msword.png");
        //        }

        //        return ImageSource ?? ImageSource.FromUri(new Uri(ImageUri));
        //    }
        //}

        [Ignore]
        public bool isImage
        {
            get { 
                 if (FileMime!=null && FileMime.ToLower().EndsWith("pdf", StringComparison.Ordinal))
                    {
                        return false;
                    }
                    if (FileName != null && FileName.ToLower().EndsWith("pdf", StringComparison.Ordinal))
                                return false;
                        

                return true;
            }

        }
        [Ignore]
        public bool isPDF
        {
            get
            {
                if (FileMime!=null && FileMime.ToLower().EndsWith("pdf", StringComparison.Ordinal))
                {
                    return true;
                }
                if (FileName!=null && FileName.ToLower().EndsWith("pdf", StringComparison.Ordinal))
                {
                    return true;
                }

                return false;
               
            }

        }



        [Ignore]
        [JsonIgnore]
        public UrlWebViewSource ImageDisplayWeb => new UrlWebViewSource { Url = ImageUri };
      


        [JsonProperty("mImageHeight")]
        public int ImageHeight { get; set; }

        [JsonProperty("mImageWidth")]
        public int ImageWidth { get; set; }

        [JsonProperty("mLegend")]
        public string Legend { get; set; }

        [JsonProperty("mComment")]
        public string Comment { get; set; }

        [JsonProperty("mActif")]
        public int IsActif { get; set; }

        [JsonProperty("mSynchronizationDate")]
        public DateTime? SynchronizationDate { get; set; }

        [JsonProperty("mCreatedOn")]
        public DateTime AddDate { get; set; }

        [JsonProperty("mModifDate")]
        public DateTime EditDate { get; set; }

        [JsonProperty("mLastViewDate")]
        public DateTime? LastViewDate { get; set; }
        
        public bool IsToSync { get; set; }

        public Media()
        {
            PropertyChanged += Media_PropertyChanged;
        }

        private void Media_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.Equals(nameof(FileData)))
            {
                OnPropertyChanged(nameof(ImageSource));
            }
        }

        public Media(MediaResponse response)
        {
            if (!string.IsNullOrWhiteSpace(response.K) && Guid.TryParse(response.K, out Guid id))
                Id = id;
            else
                Id = Guid.NewGuid();
            AddDate = response.AddDate;
            ServerId = response.ServerId;
            AccountId = response.AccountId;
            Code = response.Code;
            FileName = response.FileName;
            Year = response.Year;
            Month = response.Month;
            FileSize = response.FileSize;
            FileMime = response.FileMime;
            ImageWidth = response.ImageWidth;
            ImageHeight = response.ImageHeight;
            Legend = response.Legend;
            IsActif = response.IsActif;
            EditDate = response.EditDate;
        }

        public void UpdateFromResponse(MediaResponse response)
        {
            Code = response.Code;
            FileName = response.FileName;
            Year = response.Year;
            Month = response.Month;
            FileSize = response.FileSize;
            FileMime = response.FileMime;
            ImageWidth = response.ImageWidth;
            ImageHeight = response.ImageHeight;
            Legend = response.Legend;
            IsActif = response.IsActif;
            EditDate = response.EditDate;
        }
    }
}