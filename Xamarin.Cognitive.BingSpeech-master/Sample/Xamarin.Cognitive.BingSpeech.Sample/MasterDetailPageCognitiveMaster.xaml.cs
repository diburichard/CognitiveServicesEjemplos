using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xamarin.Cognitive.BingSpeech.Sample
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MasterDetailPageCognitiveMaster : ContentPage
    {
        public ListView ListView;

        public MasterDetailPageCognitiveMaster()
        {
            InitializeComponent();

            BindingContext = new MasterDetailPageCognitiveMasterViewModel();
            ListView = MenuItemsListView;
        }

        class MasterDetailPageCognitiveMasterViewModel : INotifyPropertyChanged
        {
            public ObservableCollection<MasterDetailPageCognitiveMenuItem> MenuItems { get; set; }

            public MasterDetailPageCognitiveMasterViewModel()
            {
                MenuItems = new ObservableCollection<MasterDetailPageCognitiveMenuItem>(new[]
                {
                    new MasterDetailPageCognitiveMenuItem { Id = 0, Title = "Speech API", TargetType = typeof(MainPage) },
                    new MasterDetailPageCognitiveMenuItem { Id = 1, Title = "Vision API" },
                    new MasterDetailPageCognitiveMenuItem { Id = 2, Title = "Knowledge API", TargetType=typeof(KnowledgeAPI) },
                    new MasterDetailPageCognitiveMenuItem { Id = 3, Title = "Language API", TargetType=typeof(LanguageAPI) },
                    new MasterDetailPageCognitiveMenuItem { Id = 4, Title = "Search API" },
                });
            }

            #region INotifyPropertyChanged Implementation
            public event PropertyChangedEventHandler PropertyChanged;
            void OnPropertyChanged([CallerMemberName] string propertyName = "")
            {
                if (PropertyChanged == null)
                    return;

                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            #endregion
        }
    }
}