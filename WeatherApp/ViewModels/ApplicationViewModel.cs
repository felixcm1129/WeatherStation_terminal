using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using WeatherApp.Commands;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.ViewModels
{
    public class ApplicationViewModel : BaseViewModel
    {
        #region Membres

        private BaseViewModel currentViewModel;
        private List<BaseViewModel> viewModels;
        private ObservableCollection<TemperatureModel> listTemperatures;
        private TemperatureViewModel tvm;
        private OpenWeatherService ows;
        private string filename;

        private VistaSaveFileDialog saveFileDialog;
        private VistaOpenFileDialog openFileDialog;

        #endregion

        #region Propriétés
        /// <summary>
        /// Model actuellement affiché
        /// </summary>
        public BaseViewModel CurrentViewModel
        {
            get { return currentViewModel; }
            set
            {
                currentViewModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// String contenant le nom du fichier
        /// </summary>
        public string Filename
        {
            get
            {
                return filename;
            }
            set
            {
                filename = value;
            }
        }

        /// <summary>
        /// Commande pour changer la page à afficher
        /// </summary>
        public DelegateCommand<string> ChangePageCommand { get; set; }

        /// <summary>
        /// TODO 02 : Ajouter ImportCommand
        /// </summary>
        public DelegateCommand<string> ImportCommand { get; set; }

        /// <summary>
        /// TODO 02 : Ajouter ExportCommand
        /// </summary>
        public DelegateCommand<string> ExportCommand { get; set; }
        /// <summary>
        /// TODO 13a : Ajouter ChangeLanguageCommand
        /// </summary>
        public DelegateCommand<string> ChangeLanguageCommand { get; set; }

        public List<BaseViewModel> ViewModels
        {
            get
            {
                if (viewModels == null)
                    viewModels = new List<BaseViewModel>();
                return viewModels;
            }
        }

        public ObservableCollection<TemperatureModel> ListTemperatures
        {
            get 
            {
                if (listTemperatures == null)
                    listTemperatures = new ObservableCollection<TemperatureModel>();
                return listTemperatures;
            }
            set 
            {
                listTemperatures = value;
            }
        }
        #endregion

        public ApplicationViewModel()
        {
            ChangePageCommand = new DelegateCommand<string>(ChangePage);

            /// TODO 06 : Instancier ExportCommand qui doit appeler la méthode Export
            /// Ne peut s'exécuter que la méthode CanExport retourne vrai
            ExportCommand = new DelegateCommand<string>(Export, CanExport);
            /// TODO 03 : Instancier ImportCommand qui doit appeler la méthode Import
            ImportCommand = new DelegateCommand<string>(Import);

            /// TODO 13b : Instancier ChangeLanguageCommand qui doit appeler la méthode ChangeLanguage
            ChangeLanguageCommand = new DelegateCommand<string>(ChangeLanguage);


            initViewModels();

            CurrentViewModel = ViewModels[0];

        }

        #region Méthodes
        void initViewModels()
        {
            /// TemperatureViewModel setup
            tvm = new TemperatureViewModel();

            string apiKey = "";

            if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "DEVELOPMENT")
            {
                apiKey = AppConfiguration.GetValue("OWApiKey");
            }

            if (string.IsNullOrEmpty(Properties.Settings.Default.apiKey) && apiKey == "")
            {
                tvm.RawText = "Aucune clé API, veuillez la configurer";
            }
            else
            {
                if (apiKey == "")
                    apiKey = Properties.Settings.Default.apiKey;

                ows = new OpenWeatherService(apiKey);
            }

            tvm.SetTemperatureService(ows);
            ViewModels.Add(tvm);

            var cvm = new ConfigurationViewModel();
            ViewModels.Add(cvm);
        }



        private void ChangePage(string pageName)
        {
            if (CurrentViewModel is ConfigurationViewModel)
            {
                ows.SetApiKey(Properties.Settings.Default.apiKey);

                var vm = (TemperatureViewModel)ViewModels.FirstOrDefault(x => x.Name == typeof(TemperatureViewModel).Name);
                if (vm.TemperatureService == null)
                    vm.SetTemperatureService(ows);
            }

            CurrentViewModel = ViewModels.FirstOrDefault(x => x.Name == pageName);
        }

        /// <summary>
        /// TODO 07 : Méthode CanExport ne retourne vrai que si la collection a du contenu
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CanExport(string obj)
        {
            if (tvm.Temperatures.Count > 0)
            {
                return true;
            }
            else return false;
        }

        /// <summary>
        /// Méthode qui exécute l'exportation
        /// </summary>
        /// <param name="obj"></param>
        private void Export(string obj)
        {

            if (saveFileDialog == null)
            {
                saveFileDialog = new VistaSaveFileDialog();
                saveFileDialog.Filter = "Json file|*.json|All files|*.*";
                saveFileDialog.DefaultExt = "json";
                saveFileDialog.FileName = "temperatures.json";
                if(saveFileDialog.ShowDialog() == true)
                {
                    Filename = saveFileDialog.FileName;
                }

                saveToFile();
            }

            /// TODO 08 : Code pour afficher la boîte de dialogue de sauvegarde
            /// Voir
            /// Solution : 14_pratique_examen
            /// Projet : demo_openFolderDialog
            /// ---
            /// Algo
            /// Si la réponse de la boîte de dialogue est vrai
            ///   Garder le nom du fichier dans Filename
            ///   Appeler la méthode saveToFile
            ///   

        }

        private void saveToFile()
        {
            /// TODO 09 : Code pour sauvegarder dans le fichier
            /// Voir 
            /// Solution : 14_pratique_examen
            /// Projet : serialization_object
            /// Méthode : serialize_array()
            /// 
            /// ---
            /// Algo
            /// Initilisation du StreamWriter
            /// Sérialiser la collection de températures
            /// Écrire dans le fichier
            /// Fermer le fichier   
            /// 

            using (var tw = new StreamWriter(Filename, false))
            {
                var temps = JsonConvert.SerializeObject(tvm.Temperatures, Formatting.Indented);
                tw.Write(temps);
                tw.Close();
            }

        }

        private void openFromFile()
        {

            /// TODO 05 : Code pour lire le contenu du fichier
            /// Voir
            /// Solution : 14_pratique_examen
            /// Projet : serialization_object
            /// Méthode : deserialize_from_file_to_object
            /// 
            /// ---
            /// Algo
            /// Initilisation du StreamReader
            /// Lire le contenu du fichier
            /// Désérialiser dans un liste de TemperatureModel
            /// Remplacer le contenu de la collection de Temperatures avec la nouvelle liste
            using (var sr = new StreamReader(Filename))
            {
                var fileContent = sr.ReadToEnd();

                ListTemperatures = JsonConvert.DeserializeObject<ObservableCollection<TemperatureModel>>(fileContent);

                tvm.Temperatures = ListTemperatures;
            }

        }

        private void Import(string obj)
        { 
                openFileDialog = new VistaOpenFileDialog();
                openFileDialog.Filter = "Json file|*.json|All files|*.*";
                openFileDialog.DefaultExt = "json";
                openFileDialog.ShowDialog();
                Filename = openFileDialog.FileName;
                openFromFile();
            

            /// TODO 04 : Commande d'importation : Code pour afficher la boîte de dialogue
            /// Voir
            /// Solution : 14_pratique_examen
            /// Projet : demo_openFolderDialog
            /// ---
            /// Algo
            /// Si la réponse de la boîte de dialogue est vraie
            ///   Garder le nom du fichier dans Filename
            ///   Appeler la méthode openFromFile

        }

        private void ChangeLanguage(string language)
        {

            Properties.Settings.Default.Language = language;
            Properties.Settings.Default.Save();

            if (MessageBox.Show(
                    Properties.Resources.msg_restart,
                    Properties.Resources.wn_warning,
                    MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                Restart();
        }

        void Restart()
        {
            var filename = Application.ResourceAssembly.Location;
            var newFile = Path.ChangeExtension(filename, ".exe");
            Process.Start(newFile);
            Application.Current.Shutdown();
        }

        #endregion
    }
}
