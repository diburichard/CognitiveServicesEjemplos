using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xamarin.Cognitive.BingSpeech.Sample
{

    public class MasterDetailPageCognitiveMenuItem
    {
        public MasterDetailPageCognitiveMenuItem()
        {
            TargetType = typeof(MasterDetailPageCognitiveDetail);
        }
        public int Id { get; set; }
        public string Title { get; set; }

        public Type TargetType { get; set; }
    }
}