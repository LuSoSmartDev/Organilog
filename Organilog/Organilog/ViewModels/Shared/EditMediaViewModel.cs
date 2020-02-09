using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using Organilog.Constants;
using Organilog.Models;
using Syncfusion.SfImageEditor.XForms;
using TinyMVVM;


using Xamarin.Forms;
using Xamarin.Forms.Extensions;
using XamStorage;

namespace Organilog.ViewModels.Shared
{
    public class EditMediaViewModel : TinyViewModel
    {
        public ICommand CancelCommand { get; private set; }

        public SfImageEditor sf;

        

        private MediaLink mediaLink;
        public MediaLink MediaLink { get => mediaLink; set => SetProperty(ref mediaLink, value); }

        public ImageSource Image  { get; private set;}

        public string textlable { get; set; }

        public EditMediaViewModel()
        {
            CancelCommand = new AwaitCommand(Cancel);

            //mediaLink.Media.FilePath;
        }
        public override  void InitAsync(NavigationParameters parameters)
        {
            base.InitAsync(parameters);


            MediaLink = parameters?.GetValue<MediaLink>(ContentKey.SELECTED_MEDIA)?.DeepCopy();

            
           
            //if(MediaLink.Media.FileData != null)
            //{
            //    await CreateRawFile("temp11.png", Convert.FromBase64String(MediaLink.Media.FileData));
            //}

        }

     
      
        private async void Cancel(object sender, TaskCompletionSource<bool> tcs)
        {
            await CoreMethods.PopViewModel(modal: IsModal);

            tcs.TrySetResult(true);
        }

        public void Editor_ImageSaving(object sender, ImageSavingEventArgs args)
        {
            var stream = args.Stream;
        }

        public IFolder GetRootFolder()
        {
            return FileSystem.Current.LocalStorage;
        }

        public async Task CreateRawFile(string filename, Stream fileContentStream, string folderLocation = null)
        {
            IFolder folder = GetRootFolder();
            if (!string.IsNullOrWhiteSpace(folderLocation))
            {
                // create a folder, if one does not exist already
                folder = await folder.CreateFolderAsync(folderLocation, CreationCollisionOption.OpenIfExists);
            }
            IFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            var fileContent = GetBytesFromStream(fileContentStream);

            using (var memoryStreamHandler = new MemoryStream())
            {
                await memoryStreamHandler.WriteAsync(fileContent, 0, fileContent.Length);

                using (var fileStreamHandler = await file.OpenAsync(XamStorage.FileAccess.ReadAndWrite))
                {
                    memoryStreamHandler.Position = 0;
                    await memoryStreamHandler.CopyToAsync(fileStreamHandler);
                }
            }
        }

        public static byte[] GetBytesFromStream(Stream fileContentStream)
        {
            using (var memoryStreamHandler = new MemoryStream())
            {
                fileContentStream.CopyTo(memoryStreamHandler);
                return memoryStreamHandler.ToArray();
            }
        }

        public async Task CreateRawFile(string filename, byte[] fileContent, string folderLocation = null)
        {
            IFolder folder = GetRootFolder();
            if (!string.IsNullOrWhiteSpace(folderLocation))
            {
                // create a folder, if one does not exist already
                folder = await folder.CreateFolderAsync(folderLocation, CreationCollisionOption.OpenIfExists);
            }
            IFile file = await folder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);

            using (var memoryStreamHandler = new MemoryStream())
            {
                await memoryStreamHandler.WriteAsync(fileContent, 0, fileContent.Length);

                using (var fileStreamHandler = await file.OpenAsync(XamStorage.FileAccess.ReadAndWrite))
                {
                    memoryStreamHandler.Position = 0;
                    await memoryStreamHandler.CopyToAsync(fileStreamHandler);
                }
            }
            Image = ImageSource.FromFile(file.Path);
        }
    }
}
