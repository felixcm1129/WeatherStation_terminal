using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Threading;
using WeatherApp.Commands;
using WeatherApp.Models;

namespace WeatherApp.ViewModels
{
    public class TemperatureViewModel : BaseViewModel
    {
        private TemperatureModel currentTemp;
        private string city;

        public ITemperatureService TemperatureService { get; private set; }

        public DelegateCommand<string> GetTempCommand { get; set; }

        public TemperatureModel CurrentTemp
        {
            get => currentTemp;
            set
            {
                currentTemp = value;
                OnPropertyChanged();
                OnPropertyChanged("RawText");
            }
        }

        private ObservableCollection<TemperatureModel> temperatures;

        public ObservableCollection<TemperatureModel> Temperatures
        {
            get { return temperatures; }
            set
            {
                temperatures = value;
                OnPropertyChanged();
            }
        }

        public string City
        {
            get { return city; }
            set
            {
                city = value;

                if (TemperatureService != null)
                {
                    TemperatureService.SetLocation(City);
                }

                OnPropertyChanged();
            }
        }

        private string _rawText;

        public string RawText
        {
            get
            {
                return _rawText;
            }
            set
            {
                _rawText = value;
                OnPropertyChanged();
            }

        }

        public TemperatureViewModel()
        {
            Name = this.GetType().Name;
            Temperatures = new ObservableCollection<TemperatureModel>();

            GetTempCommand = new DelegateCommand<string>(GetTemp, CanGetTemp);
        }


        public bool CanGetTemp(string obj)
        {
            return TemperatureService != null;
        }

        public void GetTemp(string obj)
        {
            if (TemperatureService == null) throw new NullReferenceException();

            _ = GetTempAsync();
        }

        private async Task GetTempAsync()
        {
            CurrentTemp = await TemperatureService.GetTempAsync();

            if (CurrentTemp != null)
            {
                /// TODO 01 : Insérer la température à la position 0 de la collection
                /// Description détaillée :
                /// À chaque fois que l'on clique sur le bouton "Get Data". On veut 
                /// insérer la température à la position 0 de la collection.
                /// La température n'est insérée que si la date/heure ET la ville de la
                /// dernière température insérée dans la liste est différente
                /// que celle que l'on vient de récupérer.
                /// Utiliser la méthode Insert de la collection

                if(Temperatures.Count > 0)
                {
                    if (CurrentTemp.DateTime != Temperatures[0].DateTime && CurrentTemp.City != Temperatures[0].City)
                    {
                        Temperatures.Insert(0, CurrentTemp);
                    }
                }
                else Temperatures.Add(CurrentTemp);
                Debug.WriteLine(CurrentTemp);
            }
        }

        public double CelsiusInFahrenheit(double c)
        {
            return c * 9.0 / 5.0 + 32;
        }

        public double FahrenheitInCelsius(double f)
        {
            return (f - 32) * 5.0 / 9.0;
        }

        public void SetTemperatureService(ITemperatureService srv)
        {
            TemperatureService = srv;
        }
    }
}
