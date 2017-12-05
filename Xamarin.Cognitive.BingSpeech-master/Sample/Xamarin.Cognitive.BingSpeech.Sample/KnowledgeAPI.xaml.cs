using Recommendations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin.Cognitive.BingSpeech.Sample
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class KnowledgeAPI : ContentPage
    {



       private static ObservableCollection<Knowledge.ModeloVista> recomendaciones = new ObservableCollection<Knowledge.ModeloVista>();

        public KnowledgeAPI()
        {
            InitializeComponent();
            recomendaciones.Clear();
            ModelosView.ItemsSource = recomendaciones;
            ModelosView.ItemSelected += (sender, e) => {
                var u = (Knowledge.ModeloVista)e.SelectedItem;

                string modelId = "a2c79a7b-2824-4968-b7af-39561ce4fe8a";
                long buildId = 1673873;
                myEntry.Text = u.DisplayName;

                GetRecommendationsSingleRequest(modelId, buildId, u.Codigo);
            };
        }



        public void GetRecommendationsSingleRequest(string modelId, long buildId, string itemIds)
        {
            updateUI(true);
            string AccountKey = "564eabe9165948af88bf705a56dcd42e"; // <---  Set to your API key here.
            string BaseUri = "https://westus.api.cognitive.microsoft.com/recommendations/v4.0";           
            RecommendationsApiWrapper recommender = new RecommendationsApiWrapper(AccountKey, BaseUri);
            
            var itemSets = recommender.GetRecommendations(modelId, buildId, itemIds, 6);

            recomendaciones.Clear();
            var listView = new ListView();

            listView.ItemsSource = recomendaciones;

            if (itemSets.RecommendedItemSetInfo != null)
            {
                foreach (RecommendedItemSetInfo recoSet in itemSets.RecommendedItemSetInfo)
                {
                    foreach (var item in recoSet.Items)
                    {
                        recomendaciones.Add(new Knowledge.ModeloVista() { DisplayName = item.Name, Codigo = item.Id });
                    }
                }
            }
            else
            {
            }
            updateUI(false);
        }

        private async void Empezar_Busqueda(object sender, EventArgs e)
        {
            string modelName = "MyNewModel";
            string modelId = "a2c79a7b-2824-4968-b7af-39561ce4fe8a";
            long buildId = 1673873;
            string itemIds = myEntry.Text; //"5C5-00025";
            GetRecommendationsSingleRequest(modelId, buildId, itemIds);
        }

        public void updateUI(bool spinnerEnabled = false)
        {
            spinnerContent.IsVisible = spinnerEnabled;
            spinner.IsRunning = spinnerEnabled;
        }

    }
}