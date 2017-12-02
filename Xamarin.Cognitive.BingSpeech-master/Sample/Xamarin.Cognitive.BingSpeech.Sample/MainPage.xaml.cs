using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Plugin.AudioRecorder;
using Xamarin.Forms;

namespace Xamarin.Cognitive.BingSpeech.Sample
{
	public partial class MainPage : ContentPage
	{
		AudioRecorderService recorder;
		BingSpeechApiClient bingSpeechClient;
		OutputMode outputMode;

		public MainPage ()
		{
			InitializeComponent ();

            //setting these in XAML doesn't seem to be working
            //RecognitionModePicker.SelectedIndex = 0;
            //OutputModePicker.SelectedIndex = 0;
            //ProfanityModePicker.SelectedIndex = 0;

            recorder = new AudioRecorderService
			{
				StopRecordingOnSilence = true,
				StopRecordingAfterTimeout = true,
				TotalAudioTimeout = TimeSpan.FromSeconds (15) //Bing speech REST API has 15 sec max
			};

			bingSpeechClient = new BingSpeechApiClient (Keys.BingSpeech.SubscriptionKey);

			//go fetch an auth token up front - this should decrease latecy on the first call. 
			//	Otherwise, this would be called automatically the first time I use the speech client
			Task.Run (() => bingSpeechClient.Authenticate ());
		}


		async void Record_Clicked (object sender, EventArgs e)
		{
			await RecordAudio ();
		}


		void updateUI (bool buttonEnabled, bool spinnerEnabled = false)
		{
			updateUI (buttonEnabled, null, spinnerEnabled);
		}


		void updateUI (bool buttonEnabled, string buttonText, bool spinnerEnabled = false)
		{
			RecordButton.IsEnabled = buttonEnabled;

			if (buttonText != null)
			{
				RecordButton.Text = buttonText;
			}

			spinnerContent.IsVisible = spinnerEnabled;
			spinner.IsRunning = spinnerEnabled;
		}


		async Task RecordAudio ()
		{
			try
			{
				if (!recorder.IsRecording) 
				{
					updateUI (false);

					var audioRecordTask = await recorder.StartRecording ();
					updateUI (true, "Detener");

					var recognitionMode = (RecognitionMode) Enum.Parse (typeof (RecognitionMode), 0.ToString());
					var profanityMode = (ProfanityMode) Enum.Parse (typeof (ProfanityMode), 0.ToString ());
					outputMode = (OutputMode) Enum.Parse (typeof (OutputMode), 0.ToString ());


					bingSpeechClient.RecognitionMode = recognitionMode;
					bingSpeechClient.ProfanityMode = profanityMode;


                    var audioFile = await audioRecordTask;

						updateUI (true, "Grabar", true);
						if (audioFile != null)
						{
							var resultText = await SpeechToText (audioFile);
							ResultsLabel.Text = resultText ?? "No hay Resultados!";
						}

						updateUI (true, false);
				
				}
				else //Stop button clicked
				{
					updateUI (false, true);

					//stop the recording...
					await recorder.StopRecording ();
				}
			}
			catch (Exception ex)
			{
				//blow up the app!
				throw ex;
			}
		}


		//Hook up this altrernate handler to try out the event-based API

		async Task RecordAudioAlternate ()
		{
			if (!recorder.IsRecording)
			{
				recorder.AudioInputReceived -= Recorder_AudioInputReceived;
				recorder.AudioInputReceived += Recorder_AudioInputReceived;

				updateUI (false);

				await recorder.StartRecording ();

				updateUI (true, "Detener");

				var recognitionMode = (RecognitionMode) Enum.Parse (typeof (RecognitionMode), 0.ToString ());
				var profanityMode = (ProfanityMode) Enum.Parse (typeof (ProfanityMode), 0.ToString ());
				outputMode = (OutputMode) Enum.Parse (typeof (OutputMode), 0.ToString ());

				//set the selected recognition mode & profanity mode
				bingSpeechClient.RecognitionMode = recognitionMode;
				bingSpeechClient.ProfanityMode = profanityMode;


			}
			else //Stop button clicked
			{
				updateUI (false, true);

				//stop the recording... recorded audio will be used in the Recorder_AudioInputReceived handler below
				await recorder.StopRecording ();
			}
		}


		async void Recorder_AudioInputReceived (object sender, string audioFile)
		{
			Device.BeginInvokeOnMainThread (() => updateUI (false, "Grabar", true));

				string resultText = null;

				if (audioFile != null)
				{
					resultText = await SpeechToText (audioFile);
				}

				Device.BeginInvokeOnMainThread (() =>
				{
					ResultsLabel.Text = resultText ?? "No Results!";
					updateUI (true, false);
				});

		}


        async Task<string> SpeechToText(string audioFile)
        {
            try
            {
                switch (outputMode)
                {
                    case OutputMode.Simple:
                        var simpleResult = await bingSpeechClient.SpeechToTextSimple(audioFile);

                        return ProcessResult(simpleResult);
                    case OutputMode.Detailed:
                        var detailedResult = await bingSpeechClient.SpeechToTextDetailed(audioFile);

                        return ProcessResult(detailedResult);
                }

                return null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                throw;
            }
        }


        async Task<string> SpeechToText (Task audioRecordTask)
		{
			try
			{
				using (var stream = recorder.GetAudioFileStream ())
				{
					switch (outputMode)
					{
						case OutputMode.Simple:
							var simpleResult = await bingSpeechClient.SpeechToTextSimple (stream, recorder.AudioStreamDetails.SampleRate, audioRecordTask);

							return ProcessResult (simpleResult);
						case OutputMode.Detailed:
							var detailedResult = await bingSpeechClient.SpeechToTextDetailed (stream, recorder.AudioStreamDetails.SampleRate, audioRecordTask);

							return ProcessResult (detailedResult);
					}
				}

				return null;
			}
			catch (Exception ex)
			{
				Debug.WriteLine (ex);
				throw;
			}
		}


		string ProcessResult (RecognitionSpeechResult speechResult)
		{
			string resultText = null;

			if (speechResult != null)
			{
				resultText = $"Recognition Status: {speechResult.RecognitionStatus}\r\n" +
					$"DisplayText: {speechResult.DisplayText}\r\n" +
					$"Offset: {speechResult.Offset}\r\n" +
					$"Duration: {speechResult.Duration}";
			}

			Debug.WriteLine (resultText);

			return resultText;
		}


		string ProcessResult (RecognitionResult recognitionResult)
		{
			string resultText = null;

			if (recognitionResult != null && recognitionResult.Results.Any ())
			{
				resultText = $"Recognition Status: {recognitionResult.RecognitionStatus}\r\n" +
					$"Offset: {recognitionResult.Offset}\r\n" +
					$"Duration: {recognitionResult.Duration}\r\n";

				var speechResult = recognitionResult.Results.First ();

				resultText += $"--::First Result::--\r\n" +
					$"Confidence: {speechResult.Confidence}\r\n" +
					$"Lexical: {speechResult.Lexical}\r\n" +
					$"Display: {speechResult.Display}\r\n" +
					$"ITN: {speechResult.ITN}\r\n" +
					$"Masked ITN: {speechResult.MaskedITN}";
			}

			Debug.WriteLine (resultText);

			return resultText;
		}
	}
}