using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ArtNeuralNetwork
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public NeuralNetwork network;

        private int outputImageWidth = 300;
        private int outputImageHeight = 300;
        private int outputImageDimension = 3;

        private int maxInputSize = 20; //Input Layer size
        private int HiddenLayerAmount = 200; // Hidden Layer size 

        private bool isTraining = false;

        public MainWindow()
        {
            InitializeComponent();

            //RenameFolderImages("../../../TrainingData", "dragon0");
            //RemoveDuplicateImages(@"C:\Users\DaanS\Downloads\nl_pinterest_com");

            GetAIResultButton.IsEnabled = false;
            RetrainButton.IsEnabled = false;
            TrainButton.IsEnabled = false;
            LoadAIButton.IsEnabled = false;
            SaveAIButton.IsEnabled = false;

            Task.Run(async () =>
            {
                Dispatcher.Invoke(() => { progressLabel.Content = "Init the Neural Network"; });
                InitNeuralNetwork();
                Dispatcher.Invoke(() =>
                {
                    LoadAIButton.IsEnabled = true;
                    SaveAIButton.IsEnabled = true;
                    GetAIResultButton.IsEnabled = true;
                    RetrainButton.IsEnabled = true;
                    TrainButton.IsEnabled = true;
                    progressLabel.Content = "Network has been initialized";
                });

            });
        }

        public void InitNeuralNetwork()
        {
            //Input is 100 because 100 is the max amount of chars in the input text field. because the input amount needs to be static
            //sigmoid
            //tanh
            //relu
            //leakyrelu          

            //Should increase hidden layers for more variety (white result means too much)

            //NEED MORE DATA
            //MORRREEEE DATAAAA
            // 50 doenst show much
            // 200 as hidden layer actually gives good results. spread out. 7800 image data  
            // 600 hidden layers gives amazing results. 8900 image data
            // 1200 hidden layers seems to be kinda the same as 600. 10k images. 
            //---------- dragon images
            //1200 hidden layers is better then 600 but only if it has way more data more then 2000 images 
            //The lower the  hidden layers the faster good results. But more hidden layers is better for creativity but also requires lots more images 
            //300 do seem pretty good but vague
            //300-900 is for now a good range
            //NEED MORE DRAGON DATA AND SEE WHAT HAPPENS AT 300 WITH 4K IMAGES AND 1200 WITH 4K IMAGES 
            //1200 needs more data and it will be getting really good
            //Looks like the AI kinda posts images on each other with backpropegation 
            //Maybe extra layers will help with the complexity of the network and creativity. Also different kind of activations
            //More neurons in a layer increases quality? and more hidden layers increases creativity? options? 
            int[] layers = new int[10] { maxInputSize, HiddenLayerAmount, HiddenLayerAmount, HiddenLayerAmount, HiddenLayerAmount, HiddenLayerAmount, HiddenLayerAmount, HiddenLayerAmount, HiddenLayerAmount, outputImageWidth * outputImageHeight * outputImageDimension };

            string[] activation = new string[9] { "leakyrelu", "leakyrelu", "leakyrelu", "leakyrelu", "leakyrelu", "leakyrelu", "leakyrelu", "leakyrelu", "sigmoid" };

            //Weight, bias, learningRate
            //Latest Decent ratings: 0.8f 1f 0.01f
            this.network = new NeuralNetwork(layers, activation, 0.3f, 1f, 0.03f);
        }

        public void TrainAI()
        {

            var TrainingData = Directory.GetFiles("../../../TrainingData", "*.*", SearchOption.AllDirectories).ToList();
            Dispatcher.Invoke(() =>
            {
                totalCountLabel.Content = TrainingData.Count;
            });
            GiveAIPreviewUntilTrainingStops();
            Parallel.For(0, TrainingData.Count, i =>
            {
                var inputData = ConvertStringToInputData(ValidateStringInput(TrainingData[i]));

                try
                {
                    using (Bitmap image = (Bitmap)Bitmap.FromFile(TrainingData[i]))
                    {
                        float[] expectedImageDataArray = new float[outputImageWidth * outputImageHeight * outputImageDimension];
                        var counter1 = 0;
                        for (var y = 0; y < outputImageHeight; y++)
                        {
                            for (var x = 0; x < outputImageWidth; x++)
                            {
                                expectedImageDataArray[counter1] = image.GetPixel(x, y).R / 255.0000f;
                                expectedImageDataArray[counter1 + 1] = image.GetPixel(x, y).G / 255.0000f;
                                expectedImageDataArray[counter1 + 2] = image.GetPixel(x, y).B / 255.0000f;

                                counter1 += 3;
                            }
                        }
                        network.BackPropagate(inputData, expectedImageDataArray);
                        Dispatcher.Invoke(() => { CountLabel.Content = Convert.ToInt32(CountLabel.Content) + 1; });
                    }
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(() => { ErrorCountLabel.Content = Convert.ToInt32(ErrorCountLabel.Content) + 1; });
                }
            });

            //foreach (var data in TrainingData)
            //{
            //    var inputData = ConvertStringToInputData(data.Split("\\")[data.Split("\\").Length - 1].ToLower().Replace(".png", "").Replace(".jpg", ""));

            //    var image = ResizeImage(Bitmap.FromFile(data), outputImageWidth, outputImageHeight);
            //    float[] expectedImageDataArray = new float[outputImageWidth * outputImageHeight * outputImageDimension];
            //    var counter1 = 0;
            //    for (var y = 0; y < outputImageHeight; y++)
            //    {
            //        for (var x = 0; x < outputImageWidth; x++)
            //        {
            //            expectedImageDataArray[counter1] = image.GetPixel(x, y).R / 255.0000f;
            //            expectedImageDataArray[counter1 + 1] = image.GetPixel(x, y).G / 255.0000f;
            //            expectedImageDataArray[counter1 + 2] = image.GetPixel(x, y).B / 255.0000f;

            //            counter1 += 3;
            //        }
            //    }
            //    network.BackPropagate(inputData, expectedImageDataArray);
            //}
        }

        public Bitmap ProcessAIResults(string input)
        {
            progressLabel.Content = "Starting processing...";
            var inputText = ConvertStringToInputData(input);
            var result = network.FeedForward(inputText);

            var bitmap = new Bitmap(outputImageWidth, outputImageHeight);

            var counter2 = 0;
            for (var y = 0; y < outputImageHeight; y++)
            {
                for (var x = 0; x < outputImageWidth; x++)
                {
                    var r = Math.Round(result[counter2] * 255.0000f);
                    var g = Math.Round(result[counter2 + 1] * 255.0000f);
                    var b = Math.Round(result[counter2 + 2] * 255.0000f);

                    if (double.IsNaN(r)) r = 255;
                    if (double.IsNaN(g)) g = 255;
                    if (double.IsNaN(b)) b = 255;

                    if (r < 0) r = 0;
                    if (g < 0) g = 0;
                    if (b < 0) b = 0;

                    if (r > 255) r = 255;
                    if (g > 255) g = 255;
                    if (b > 255) b = 255;

                    bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb((int)r, (int)g, (int)b));
                    counter2 += 3;
                }
            }

            AIImagesResult.Source = BitmapToImageSource(bitmap);
            progressLabel.Content = "Finished results";
            return bitmap;
        }

        //Convert text to byte array to be inputed into the neural network input layer
        private float[] ConvertStringToInputData(string inputString)
        {
            var testString = inputString;
            var startLenght = testString.Length;
            for (var i = 0; i < maxInputSize - startLenght; i++) //Add extra spaces to make the array the max input size
            {
                testString += " ";
            }

            var testStringArray = Encoding.ASCII.GetBytes(testString); //Convert string to bytearray
            float[] inputArray = new float[maxInputSize];

            for (var i = 0; i < maxInputSize; i++) //Convert byte array to value between 0 and 1. where 0 is a space. Lower case only.
            {
                //Optimize input method

                var ascii = (testStringArray[i] - 96) / 26.0000f;
                var fixedByteData = ascii < 0 ? 0 : ascii;
                inputArray[i] = fixedByteData;
            }

            return inputArray;
        }

        //Resized Images
        private static Bitmap ResizeImage(System.Drawing.Image image, int width, int height)
        {
            var destRect = new System.Drawing.Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private BitmapImage BitmapToImageSource(System.Drawing.Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                memory.Position = 0;
                BitmapImage bitmapimage = new BitmapImage();
                bitmapimage.BeginInit();
                bitmapimage.StreamSource = memory;
                bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapimage.EndInit();
                return bitmapimage;
            }
        }

        private void RescaleAllImages(string folderPath, int width, int height)
        {
            var ImagesToRescalePaths = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories).ToList();
            foreach (var imageToRescalePath in ImagesToRescalePaths)
            {
                try
                {
                    using (var bitmap = Bitmap.FromFile(imageToRescalePath))
                    {
                        if (bitmap.Width == outputImageWidth && bitmap.Height == outputImageHeight) continue;
                        using (var image = ResizeImage(bitmap, width, height))
                        {
                            var imageToSave = (System.Drawing.Image)image.Clone();
                            image.Dispose();
                            bitmap.Dispose();
                            File.Delete(imageToRescalePath);
                            imageToSave.Save(imageToRescalePath);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void GetAIResultButton_Click(object sender, RoutedEventArgs e)
        {
            ProcessAIResults(InputTextbox.Text);
        }

        private void GiveAIPreviewUntilTrainingStops()
        {
            isTraining = true;
            Task.Run(async () =>
            {
                while (isTraining)
                {
                    Dispatcher.Invoke(() => { ProcessAIResults(InputTextbox.Text); });
                    await Task.Delay(20000);
                }
            });
        }


        private async Task<bool> DataNeedsRescaling(string folderPath)
        {
            var ImagesToRescalePaths = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories).ToList();
            foreach (var imageToRescalePath in ImagesToRescalePaths)
            {
                try
                {
                    using (var bitmap = System.Drawing.Image.FromFile(imageToRescalePath))
                    {
                        if (bitmap.Width != outputImageWidth || bitmap.Height != outputImageHeight) return true;
                    }
                }
                catch (Exception ex)
                {
                    File.Delete(imageToRescalePath);
                }
            }
            return false;
        }

        static string ValidateStringInput(string text)
        {
            var cleanInput = text.Split("\\")[text.Split("\\").Length - 1].ToLower().Replace(".jpeg", "").Replace(".png", "").Replace("_", " ").Replace("-", " ").Replace(".jpg", "") + " ";
            string output = Regex.Replace(cleanInput, @"\d{2,}", " ");
            output = Regex.Replace(output, @"\d", " ");
            return output;
        }

        private void RetrainButton_Click(object sender, RoutedEventArgs e)
        {
            progressLabel.Content = "Retraining AI...";
            GetAIResultButton.IsEnabled = false;
            RetrainButton.IsEnabled = false;
            Task.Run(() =>
            {
                TrainAI();
                Dispatcher.Invoke(() =>
                {
                    GetAIResultButton.IsEnabled = true;
                    RetrainButton.IsEnabled = true;
                    progressLabel.Content = "Finished Retraining";
                });
            });
        }

        private async void MutateButton_Click(object sender, RoutedEventArgs e)
        {
            var weight = (float)Convert.ToDouble(mutateWeightInput.Text);
            progressLabel.Content = "Mutating the network";
            var s = new Stopwatch();
            s.Start();
            await network.Mutate(1, weight);
            s.Stop();
            progressLabel.Content = $"Mutated the network in {s.ElapsedMilliseconds}ms";
        }

        private void LoadAIButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();

            //Open the Pop-Up Window to select the file 
            if (dlg.ShowDialog() == true)
            {
                new FileInfo(dlg.FileName);
                using (Stream s = dlg.OpenFile())
                {
                    network.Load(dlg.FileName);
                }
                progressLabel.Content = $"Loaded in AI {dlg.FileName}";
            }
        }

        private void SaveAIButton_Click(object sender, RoutedEventArgs e)
        {
            var path = "../../../AI_Saves/" + $"I_{maxInputSize}-L-N_{network.layers.Count()}-{HiddenLayerAmount}W-{network.weightMax}_B-{network.BiasMax}_L-{network.learningRate}_{DateTime.Now}.txt".Replace('/', '-').Replace(':', '-');
            network.Save(path);
            progressLabel.Content = $"Saved AI as {path}";
        }

        private void TrainButton_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(async () =>
            {
                Dispatcher.Invoke(() => { progressLabel.Content = "Checking if rescale is needed"; });
                if (await DataNeedsRescaling("../../../TrainingData"))
                {
                    Dispatcher.Invoke(() => { progressLabel.Content = "Rescaling is needed, rescaling trainingPath..."; });
                    RescaleAllImages("../../../TrainingData", outputImageWidth, outputImageHeight);
                }
                Dispatcher.Invoke(() => { progressLabel.Content = "Training the AI... please wait"; });
                TrainAI();

                Dispatcher.Invoke(() =>
                {
                    progressLabel.Content = "AI trained... Waiting for input";                    
                });
                isTraining = false;
            });
        }
        private void RenameFolderImages(string path, string name)
        {
            var ImagesToRescalePaths = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToList();
            var count = 0;
            foreach (var imageToRescalePath in ImagesToRescalePaths)
            {
                try
                {

                    File.Move(imageToRescalePath, $"{path}/{name}{count}.png");

                }
                catch (Exception ex)
                {

                }
                count++;
            }
        }

        //One time only function
        private void RemoveDuplicateImages(string path)
        {
            var ImagesToRescalePaths = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories).ToList();
            var list = new List<string>();
            foreach (var imageToRescalePath in ImagesToRescalePaths)
            {
                var stringToCompare = imageToRescalePath.Replace(" (1).jpg", "").Replace(" (2).jpg", "").Replace(" (3).jpg", "").Replace(".jpg", "");
                if (list.Contains(stringToCompare)) File.Delete(imageToRescalePath);
                else list.Add(stringToCompare);      
            }
        }

        private void RecordVideo(int durationInFrames)
        {
            var TrainingData = Directory.GetFiles("../../../TrainingData", "*.*", SearchOption.AllDirectories).ToList();
            for (var i = 0; i < durationInFrames; i++)
            {

                var inputData = ConvertStringToInputData(ValidateStringInput(InputTextbox.Text));

                try
                {
                    using (Bitmap image = (Bitmap)Bitmap.FromFile(TrainingData[new Random().Next(0, TrainingData.Count - 1)]))
                    {
                        float[] expectedImageDataArray = new float[outputImageWidth * outputImageHeight * outputImageDimension];
                        var counter1 = 0;
                        for (var y = 0; y < outputImageHeight; y++)
                        {
                            for (var x = 0; x < outputImageWidth; x++)
                            {
                                expectedImageDataArray[counter1] = image.GetPixel(x, y).R / 255.0000f;
                                expectedImageDataArray[counter1 + 1] = image.GetPixel(x, y).G / 255.0000f;
                                expectedImageDataArray[counter1 + 2] = image.GetPixel(x, y).B / 255.0000f;

                                counter1 += 3;
                            }
                        }
                        network.BackPropagate(inputData, expectedImageDataArray);
                        var processedImage = ProcessAIResults(InputTextbox.Text);
                        processedImage.Save($"../../../RecordingTemp/frame{i}.png");
                    }
                }
                catch (Exception ex)
                {
                }

            }
            //Backpropegate random images from the training folder 
            //After each backpropegate, the endresult should be copied and stored in a list 
            //Store all frames into a folder. maybe even create an mp4 file directly from it. Or gif? maybe a looped gif? 
        }

        private void RecordFramesButton_Click(object sender, RoutedEventArgs e)
        {
            RecordVideo(60);
        }
    }
}
