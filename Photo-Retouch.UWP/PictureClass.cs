using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Controls;

namespace Photo_Retouch.UWP
{
    public class Pixel
    {
        public int r;
        public int g;
        public int b;
        public int a;
    }

    public partial class PictureClass
    {
        public Dictionary<IStorageItem, MenuFlyoutItem> MRUDic = new Dictionary<IStorageItem, MenuFlyoutItem>();
        public ObservableCollection<FileClass> FileList = new ObservableCollection<FileClass>();
        public FileClass CurrentFile = null;
        public ObservableCollection<FolderClass> FolderList = new ObservableCollection<FolderClass>();

        public async Task<bool> PictureDecodeAsync()
        {
            var file = await StorageFile.GetFileFromPathAsync(CurrentFile.FilePath);
            var stream = await file.OpenAsync(FileAccessMode.Read);
            var decoder = await BitmapDecoder.CreateAsync(stream);
            var data = await decoder.GetPixelDataAsync();
            CurrentFile.PixelFormat = decoder.BitmapPixelFormat;
            var bytes = data.DetachPixelData();
            CurrentFile.Pixels = new Pixel[decoder.PixelHeight, decoder.PixelWidth];

            int pixelHeight = (int)decoder.PixelHeight;
            int pixelWidth = (int)decoder.PixelWidth;
            int temp;
            for (int i = 0; i < pixelHeight; i++)
            {
                for (int j = 0; j < pixelWidth; j++)
                {
                    temp = (i * pixelWidth + j) * 4;
                    CurrentFile.Pixels[i, j] = new Pixel()
                    {
                        b = bytes[temp + 0],
                        g = bytes[temp + 1],
                        r = bytes[temp + 2],
                        a = bytes[temp + 3],
                    };
                }
            }
            return true;
        }

        public async Task<bool> PictureEncodeAsync()
        {
            var rand = new Random();
            StorageFolder storageFolder = ApplicationData.Current.TemporaryFolder;
            CurrentFile.TempStorageFile = await storageFolder.CreateFileAsync
                (CurrentFile.FileName + rand.Next(100000, 1000000).ToString());

            var bytes = new byte[CurrentFile.TempPixels.Length * 4];
            uint row = (uint)CurrentFile.TempPixels.GetLength(0);
            uint col = (uint)CurrentFile.TempPixels.GetLength(1);
            var Pixels = CurrentFile.TempPixels;

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    bytes[(i * col + j) * 4 + 0] = (byte)Pixels[i, j].b;
                    bytes[(i * col + j) * 4 + 1] = (byte)Pixels[i, j].g;
                    bytes[(i * col + j) * 4 + 2] = (byte)Pixels[i, j].r;
                    bytes[(i * col + j) * 4 + 3] = (byte)Pixels[i, j].a;
                }
            }

            using (var storageStream = await CurrentFile.TempStorageFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.BmpEncoderId, storageStream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                col, row, CurrentFile.DPIX, CurrentFile.DPIY, bytes);
            }
            return true;
        }

        public void ApplyFilter(string name, Dictionary<string, string> keyValues)
        {
            var InArr = CurrentFile.Pixels;
            var OutArr = CurrentFile.Pixels;

            int row = CurrentFile.Pixels.GetLength(0);
            int col = CurrentFile.Pixels.GetLength(1);

            switch (name)
            {
                case "Reverse":
                    OutArr = ReverseFilter(InArr, OutArr, row, col);
                    break;

            }
            CurrentFile.TempPixels = OutArr;
        }

        public byte[] PixelToByte(Pixel[] pixels)
        {
            var bytes = new byte[pixels.Length * 4];
            int i = 0;
            foreach (var pixel in pixels)
            {
                bytes[i] = (byte)pixels[i].b;
                i += 1;
                bytes[i] = (byte)pixels[i].g;
                i += 1;
                bytes[i] = (byte)pixels[i].r;
                i += 1;
                bytes[i] = (byte)pixels[i].a;
                i += 1;
            }
            return bytes;
        }
    }

    public class FileClass
    {
        public StorageFile TempStorageFile { get; set; }
        public StorageItemThumbnail Thumbnail { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public string FilePath { get; set; }
        public double DPIX { get; set; }
        public double DPIY { get; set; }
        public BitmapPixelFormat PixelFormat { get; set; }
        public dynamic Pixels { get; set; }
        public dynamic TempPixels { get; set; }

        public FileClass()
        {
            
        }

        public FileClass(StorageFile file)
        {
            Task task = GetThumnail(file);
            FileName = file.DisplayName;
            FileExtension = file.FileType;
            FilePath = file.Path;

        }

        private async Task GetThumnail(StorageFile file)
        {
            Thumbnail = await file.GetThumbnailAsync( ThumbnailMode.PicturesView);
        }
    }

    public class FolderClass
    {
        public string FolderName { get; set; }
        public ObservableCollection<FolderClass> Folders {get;set;}
        public ObservableCollection<FileClass> Files {get;set;}

        public FolderClass(StorageFolder folder)
        {

        }
    }
}
