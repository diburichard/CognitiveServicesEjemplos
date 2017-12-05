using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Microsoft.ProjectOxford.Emotion.Contract;
using Microsoft.ProjectOxford.Face;
using Plugin.Media;
using Plugin.Media.Abstractions;

using Microsoft.ProjectOxford.Common.Contract;

namespace Xamarin.Cognitive.BingSpeech.Sample
{
    [XamlCompilation(XamlCompilationOptions.Skip)]
    public partial class APIVision : ContentPage
    {


        public APIVision()
        {
            InitializeComponent();
        }


        private async void UploadPictureButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (!CrossMedia.Current.IsPickPhotoSupported)
                {
                    await DisplayAlert("No upload", "Picking a photo is not supported.", "OK");
                    return;
                }
                var file = await CrossMedia.Current.PickPhotoAsync();
                if (file == null)
                    return;
                this.Indicator1.IsVisible = true;
                this.Indicator1.IsRunning = true;
                Image1.Source = ImageSource.FromStream(() => file.GetStream());
                FaceEmotionDetection theData = await DetectFaceAndEmotionsAsync(file);
                this.BindingContext = theData;
                this.Indicator1.IsRunning = false;
                this.Indicator1.IsVisible = false;
            }
            catch(Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }

        }


        private async void TakePictureButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                await CrossMedia.Current.Initialize();
                if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.
                  IsTakePhotoSupported)
                {
                    await DisplayAlert("No Camera", "No camera available.", "OK");
                    return;
                }
                var file = await CrossMedia.Current.TakePhotoAsync(new StoreCameraMediaOptions
                {
                    SaveToAlbum = true,
                    Name = "test.jpg"
                });
                if (file == null)
                    return;
                this.Indicator1.IsVisible = true;
                this.Indicator1.IsRunning = true;
                Image1.Source = ImageSource.FromStream(() => file.GetStream());
                FaceEmotionDetection theData = await DetectFaceAndEmotionsAsync(file);
                this.BindingContext = theData;
                this.Indicator1.IsRunning = false;
                this.Indicator1.IsVisible = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }

        }


        private async Task<FaceEmotionDetection> DetectFaceAndEmotionsAsync(MediaFile inputFile)
        {
            try
            {
                IFaceServiceClient faceServiceClient = new FaceServiceClient("a795a1dfd1d4495c9fc338ec4794d841", "https://westcentralus.api.cognitive.microsoft.com/face/v1.0");


                var requiredFaceAttributes = new FaceAttributeType[] {
                                                  FaceAttributeType.Age,
                                                  FaceAttributeType.Gender,
                                                  FaceAttributeType.Smile,
                                                  FaceAttributeType.FacialHair,
                                                  FaceAttributeType.HeadPose,
                                                  FaceAttributeType.Glasses,
                                                  FaceAttributeType.Emotion
                                                  };
                var faces = await faceServiceClient.DetectAsync(inputFile.GetStream(),
                  false, false, requiredFaceAttributes);
                var faceAttributes = faces[0]?.FaceAttributes;

                FaceEmotionDetection faceEmotionDetection = new FaceEmotionDetection();
                faceEmotionDetection.Age = faceAttributes.Age;
                string emocion = obtenerEmocion(faceAttributes.Emotion);
                faceEmotionDetection.Emotion = emocion;// faceEmotion.FirstOrDefault().Key;
                faceEmotionDetection.Glasses = faceAttributes.Glasses.ToString();
                faceEmotionDetection.Smile = faceAttributes.Smile;
                faceEmotionDetection.Gender = faceAttributes.Gender;
                faceEmotionDetection.Moustache = faceAttributes.FacialHair.Moustache;
                faceEmotionDetection.Beard = faceAttributes.FacialHair.Beard;

                return faceEmotionDetection;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
                return null;
            }
        }

        private string obtenerEmocion(EmotionScores emotion)
        {
            string[] emocion = { "Feliz", "Triste", "Neutral", "Sorpendido", "Enojado", "Contento", "Disgustado", "Miedoso" };
            float[] valores = { emotion.Happiness, emotion.Sadness, emotion.Neutral, emotion.Surprise, emotion.Anger, emotion.Contempt, emotion.Disgust, emotion.Fear };
            float maxValue = valores.Max();
            int indice = valores.ToList().IndexOf(maxValue);
            return emocion[indice];
        }
    }

    public class FaceEmotionDetection
    {
        public string Emotion { get; set; }
        public double Smile { get; set; }
        public string Glasses { get; set; }
        public string Gender { get; set; }
        public double Age { get; set; }
        public double Beard { get; set; }
        public double Moustache { get; set; }
    }



}