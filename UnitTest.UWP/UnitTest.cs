using System;
using System.Diagnostics;
using Photo_Retouch.UWP;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Windows.Storage;
using System.Threading.Tasks;

namespace UnitTest.UWP
{
    [TestClass]
    public class BasicTest
    {
        [TestMethod]
        public async Task ApplyFilter()
        {
            var pictureClass = new PictureClass();
            var testFile = await StorageFile.GetFileFromApplicationUriAsync(
                new Uri("ms-appx:///TestAssets/Sample_0250.JPG"));
            pictureClass.CurrentFile = new FileClass(testFile);
            await pictureClass.PictureDecodeAsync();
            pictureClass.ApplyFilter("Reverse", null);
            await pictureClass.PictureEncodeAsync();
            Assert.IsNotNull(pictureClass.CurrentFile.TempPixels);
            Assert.IsNotNull(pictureClass.CurrentFile.TempStorageFile);
        }
    }

    [TestClass]
    public class PerformanceTest
    {
        [TestMethod]
        public async Task GetTime_0250()
        {
            PictureClass pictureClass = new PictureClass();
            var sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            var testFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///TestAssets/Sample_0250.JPG"));
            pictureClass.CurrentFile = new FileClass(testFile);
            await pictureClass.PictureDecodeAsync();
            pictureClass.ApplyFilter("Reverse", null);
            await pictureClass.PictureEncodeAsync();
            sw.Stop();
            Trace.WriteLine("Time For All : "+(sw.ElapsedMilliseconds/1000));
            Assert.IsNotNull(pictureClass.CurrentFile.TempPixels);
            Assert.IsNotNull(pictureClass.CurrentFile.TempStorageFile);
        }

        [TestMethod]
        public async Task GetTime_1000()
        {
            PictureClass pictureClass = new PictureClass();
            var sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            var testFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///TestAssets/Sample_1000.JPG"));
            pictureClass.CurrentFile = new FileClass(testFile);
            await pictureClass.PictureDecodeAsync();
            pictureClass.ApplyFilter("Reverse", null);
            await pictureClass.PictureEncodeAsync();
            sw.Stop();
            Trace.WriteLine("Time For All : "+(sw.ElapsedMilliseconds/1000));
            Assert.IsNotNull(pictureClass.CurrentFile.TempPixels);
            Assert.IsNotNull(pictureClass.CurrentFile.TempStorageFile);
        }

        [TestMethod]
        public async Task GetTime_4000()
        {
            PictureClass pictureClass = new PictureClass();
            var sw = new Stopwatch();
            sw.Reset();
            sw.Start();
            var testFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///TestAssets/Sample_4000.JPG"));
            pictureClass.CurrentFile = new FileClass(testFile);
            await pictureClass.PictureDecodeAsync();
            pictureClass.ApplyFilter("Reverse", null);
            await pictureClass.PictureEncodeAsync();
            sw.Stop();
            Trace.WriteLine("Time For All : "+(sw.ElapsedMilliseconds/1000));
            Assert.IsNotNull(pictureClass.CurrentFile.TempPixels);
            Assert.IsNotNull(pictureClass.CurrentFile.TempStorageFile);
        }
    }
}
