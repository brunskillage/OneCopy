using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using OneCopy2017;
using OneCopy2017.TinyIoc;

namespace OneCopy.MsTests
{
    public class TestSetup
    {
        public static readonly string RootDir = "c:\\temp\\DupeTestRoot";
        public static readonly string FirstDir = RootDir + "\\FirstDir";
        public static readonly string NestedDir = RootDir + "\\ThirdDir\\Nested";
        public static readonly string Dupe1SourceFullPath = RootDir + "\\FileF.bin";

        public static TinyIoCContainer RegisterServices()
        {
            var container = TinyIoCContainer.Current;

            var typesToRegister =
                AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a => a.GetTypes())
                    .Where(t => t.FullName.StartsWith("OneCopy2017") && t.FullName.EndsWith("Service"))
                    .ToList();

            foreach (var type in typesToRegister)
                container.Register(type).AsSingleton();

            container.Register<App>().AsSingleton();

            return container;
        }

        public static void ClearTestFiles()
        {
            if (Directory.Exists(RootDir))
            {
                Directory.EnumerateFiles(RootDir).ToList().ForEach(File.Delete);
                Directory.EnumerateDirectories(RootDir).ToList().ForEach(d => Directory.Delete(d, true));
            }
        }

        public static void CreateSimulatedRealLifeDirectoryIfNotExists()
        {
            if (!Directory.Exists(RootDir))
            {
                SaveGraphicText("FileA", RootDir + "\\FileA.jpg");  
                SaveText("FileB", RootDir + "\\FileB.txt");
                SaveText("FileC", FirstDir + "\\FileC.txt");
                SaveGraphicText("FileD", NestedDir + "\\FileD.jpg");
                SaveRandomBlobOfLength(1, NestedDir + "\\FileE.bin");
                SaveRandomBlobOfLength(1, RootDir + "\\FileF.bin");
            }
        }

        public static void SaveText(string text, string fullName)
        {
            new FileInfo(fullName).Directory.Create();
            Console.WriteLine("Creating file at " + fullName);
            File.WriteAllText(fullName, text);
        }

        public static void SaveRandomBlobOfLength(int sizeInMb, string fileName)
        {
            byte[] data = new byte[sizeInMb * 1024 * 1024];
            Random rng = new Random();
            rng.NextBytes(data);
            File.WriteAllBytes(fileName, data);
        }

        public static void SaveGraphicText(string text, string fullName)
        {
            var width = 100;
            var height = 100;
            using (var bmp = new Bitmap(width, height))
            using (var gfx = Graphics.FromImage(bmp))
            {
                gfx.DrawString(text, new Font("Tahoma", 20),
                    Brushes.Red, new PointF(10, 10));

                gfx.DrawRectangle(new Pen(Color.White), 50, 50, 20, 30);
                Console.WriteLine("Creating file at " + fullName);
                new FileInfo(fullName).Directory.Create();
                bmp.Save(fullName, ImageFormat.Jpeg);
            }
        }

        public static void SaveRandomImage(string text, string fullName)
        {
            var width = 100;
            var height = 100;
            using (var bmp = new Bitmap(width, height))
            {
                var r = new Random();

                for (var x = 0; x < width; x++)
                    for (var y = 0; y < height; y++)
                    {
                        var num = r.Next(0, 256);
                        bmp.SetPixel(x, y, Color.FromArgb(255, num, num, num));
                    }
                new FileInfo(fullName).Directory.Create();
                Console.WriteLine("Creating file at " + fullName);
                bmp.Save(fullName, ImageFormat.Jpeg);
            }
        }
    }
}