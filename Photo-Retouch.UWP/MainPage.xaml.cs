using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.Core;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.StartScreen;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Photo_Retouch.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private PictureClass pictureClass = new PictureClass();
        private DataClass dataClass = new DataClass()
        {
            FileView_IsExpand = true,
            FolderView_IsExpand = true,
            IsFirstUse = SystemInformation.IsFirstRun,
        };

        public MainPage()
        {
            InitializeComponent();

            AppCenter.Start("92aeca04-08ed-439a-b41c-617595109945", typeof(Analytics));

            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            UpdateTitleBarLayout(coreTitleBar);
            Window.Current.SetTitleBar(AppTitleBar);

            coreTitleBar.LayoutMetricsChanged += CoreTitleBar_LayoutMetricsChanged;
            coreTitleBar.IsVisibleChanged += CoreTitleBar_IsVisibleChanged;

            //GetMRU();

            File_ListView.ItemsSource = pictureClass.FileList;
            FileExpand_Collapse();
            FolderExpand_Collapse();

            if (dataClass.IsFirstUse)
            {
                FirstStart();
            }
        }

        private async void FirstStart()
        {
            var jumpList = await JumpList.LoadCurrentAsync();
            jumpList.SystemGroupKind = JumpListSystemGroupKind.Recent;
        }

        private async void GetMRU()
        {
            var mru = StorageApplicationPermissions.MostRecentlyUsedList;
            
            foreach (AccessListEntry entry in mru.Entries)
            {
                var mruToken = entry.Token;
                var mruMetadata = entry.Metadata;
                var item = await mru.GetItemAsync(mruToken);
                if (item.IsOfType(StorageItemTypes.File))
                {
                    var file = (StorageFile)item;
                }
                else if (item.IsOfType(StorageItemTypes.Folder))
                {
                    var folder = (StorageFolder)item;
                }
            }
        }

        private void CoreTitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            UpdateTitleBarLayout(sender);
        }

        private void UpdateTitleBarLayout(CoreApplicationViewTitleBar coreTitleBar)
        {
            LeftPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayLeftInset);
            AppMenuBar.Margin = new Thickness(coreTitleBar.SystemOverlayLeftInset + 44, 0, 0, 0);
            RightPaddingColumn.Width = new GridLength(coreTitleBar.SystemOverlayRightInset);
            AppStackPanel.Margin = new Thickness(0, 0, coreTitleBar.SystemOverlayRightInset, 0);
            AppTitleBar.Height = coreTitleBar.Height;
            AppMenuBar.Height = coreTitleBar.Height;
            AppStackPanel.Height = coreTitleBar.Height;
        }

        private void CoreTitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args)
        {
            if (sender.IsVisible)
            {
                AppTitleBar.Visibility = Visibility.Visible;
            }
            else
            {
                AppTitleBar.Visibility = Visibility.Visible;
            }
        }

        private async void OpenPicture_Menu_Click(object sender, RoutedEventArgs e)
        {
            var mru = StorageApplicationPermissions.MostRecentlyUsedList;
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");

            var files = await picker.PickMultipleFilesAsync();
            if (files.Count > 0)
            {
                foreach (StorageFile file in files)
                {
                    if (file.IsAvailable)
                    {
                        var fileClass = new FileClass(file);
                        pictureClass.FileList.Add(fileClass);
                        string mruToken = mru.Add(file);
                    }
                }
                FileExpand_Expand();
            }
            else
            {
                ShowDialog("Error", "Can't open picture(s).");
            }
        }

        private void Exit_Menu_Click(object sender, RoutedEventArgs e)
        {
            Windows.UI.Xaml.Application.Current.Exit();
        }


        private async void ShowDialog(string title, string content)
        {
            var dialog = new ContentDialog()
            {
                Title = title,
                Content = content,
            };
            await dialog.ShowAsync();
        }

        private void FullScreen_Menu_Click(object sender, RoutedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                view.ExitFullScreenMode();
            }
            else
            {
                view.TryEnterFullScreenMode();
            }
        }

        private async void OpenFolder_Menu_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker
            {
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                ViewMode = PickerViewMode.Thumbnail
            };
            folderPicker.FileTypeFilter.Add("*");

            StorageFolder folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                var folderClass = new FolderClass(folder);

                var mru = StorageApplicationPermissions.MostRecentlyUsedList;
                string mruToken = mru.Add(folder);
            }
            else
            {
                ShowDialog("Error", "Can't open folder.");
            }
        }

        private async void File_ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            FileClass clickedItem = (FileClass)e.ClickedItem;
            pictureClass.CurrentFile = clickedItem;
            var file = await StorageFile.GetFileFromPathAsync(pictureClass.CurrentFile.FilePath);
            IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
            // Set the image source to the selected bitmap
            BitmapImage bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(fileStream);
            Left_Image.Source = bitmapImage;
            //Set LeftImage as Max View
            //LeftGrid_Width.Width = new GridLength(LeftGrid_Row.ActualHeight * (bitmapImage.PixelWidth / bitmapImage.PixelHeight));
            fileStream.Dispose();
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var view = ApplicationView.GetForCurrentView();
            if (view.IsFullScreenMode)
            {
                FullScreen_Menu.IsChecked = true;
            }
            else
            {
                FullScreen_Menu.IsChecked = false;
            }
        }

        private void FileExpand_Button_Click(object sender, RoutedEventArgs e)
        {
            FileExpand_Change();
        }

        private void FileExpand_Change()
        {
            if (dataClass.FileView_IsExpand)
            {
                dataClass.FileView_IsExpand = false;
                FileExpand_FontIcon.Glyph = "\uE70E";
                File_ListView.Visibility = Visibility.Collapsed;
                FileExpand_Button.Background = (SolidColorBrush)Windows.UI.Xaml.Application.Current.Resources["Color_Dark1"];
            }
            else
            {
                dataClass.FileView_IsExpand = true;
                FileExpand_FontIcon.Glyph = "\uE70D";
                File_ListView.Visibility = Visibility.Visible;
                FileExpand_Button.Background = (SolidColorBrush)Windows.UI.Xaml.Application.Current.Resources["Color_Light1"];
            }
        }

        private bool FileExpand_Expand()
        {
            if (!dataClass.FileView_IsExpand)
            {
                dataClass.FileView_IsExpand = true;
                FileExpand_FontIcon.Glyph = "\uE70D";
                File_ListView.Visibility = Visibility.Visible;
                FileExpand_Button.Background = (SolidColorBrush)Windows.UI.Xaml.Application.Current.Resources["Color_Light1"];
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool FileExpand_Collapse()
        {
            if (dataClass.FileView_IsExpand)
            {
                dataClass.FileView_IsExpand = false;
                FileExpand_FontIcon.Glyph = "\uE70E";
                File_ListView.Visibility = Visibility.Collapsed;
                FileExpand_Button.Background = (SolidColorBrush)Windows.UI.Xaml.Application.Current.Resources["Color_Dark1"];
                return true;
            }
            else
            {
                return false;
            }
        }

        private void FolderExpand_Button_Click(object sender, RoutedEventArgs e)
        {
            FolderExpand_Change();
        }

        private void FolderExpand_Change()
        {
            if (dataClass.FolderView_IsExpand)
            {
                dataClass.FolderView_IsExpand = false;
                FolderExpand_FontIcon.Glyph = "\uE70E";
                Folder_ListView.Visibility = Visibility.Collapsed;
                FolderExpand_Button.Background = (SolidColorBrush)Windows.UI.Xaml.Application.Current.Resources["Color_Dark1"];
            }
            else
            {
                dataClass.FolderView_IsExpand = true;
                FolderExpand_FontIcon.Glyph = "\uE70D";
                Folder_ListView.Visibility = Visibility.Visible;
                FolderExpand_Button.Background = (SolidColorBrush)Windows.UI.Xaml.Application.Current.Resources["Color_Light1"];
            }
        }

        private bool FolderExpand_Expand()
        {
            if (!dataClass.FolderView_IsExpand)
            {
                dataClass.FolderView_IsExpand = true;
                FolderExpand_FontIcon.Glyph = "\uE70D";
                Folder_ListView.Visibility = Visibility.Visible;
                FolderExpand_Button.Background = (SolidColorBrush)Windows.UI.Xaml.Application.Current.Resources["Color_Light1"];
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool FolderExpand_Collapse()
        {
            if (dataClass.FolderView_IsExpand)
            {
                dataClass.FolderView_IsExpand = false;
                FolderExpand_FontIcon.Glyph = "\uE70E";
                Folder_ListView.Visibility = Visibility.Collapsed;
                FolderExpand_Button.Background = (SolidColorBrush)Windows.UI.Xaml.Application.Current.Resources["Color_Dark1"];
                return true;
            }
            else
            {
                return false;
            }
        }

        private void FileExclude_CommandsFlyout_Click(object sender, RoutedEventArgs e)
        {

        }

        private void FileDelete_CommandsFlyout_Click(object sender, RoutedEventArgs e)
        {

        }

        private void CloseAllPictures_Click(object sender, RoutedEventArgs e)
        {
            pictureClass.FileList.Clear();
            pictureClass.CurrentFile = null;
            FileExpand_Collapse();
        }

        private async void Test_Button_Click(object sender, RoutedEventArgs e)
        {
            await pictureClass.PictureDecodeAsync();
            pictureClass.ApplyFilter("Reverse", null);
            await pictureClass.PictureEncodeAsync();

            IRandomAccessStream fileStream = await pictureClass.CurrentFile.TempStorageFile.OpenAsync(FileAccessMode.Read);
            BitmapImage bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(fileStream);
            Right_Image.Source = bitmapImage;
        }

        private void KeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            switch (sender.Modifiers)
            {
                case VirtualKeyModifiers.Control:
                    switch (sender.Key)
                    {
                        case VirtualKey.Q:
                            QS_ASB.Focus(FocusState.Keyboard);
                            break;
                    }
                    break;
                case VirtualKeyModifiers.None:
                    switch (sender.Key)
                    {
                        case VirtualKey.Escape:
                            //TODO
                            break;
                    }
                    break;
            }
        }

        private async void StoreReview_Menu_Click(object sender, RoutedEventArgs e)
        {
            await SystemInformation.LaunchStoreForReviewAsync();
        }
    }
}
