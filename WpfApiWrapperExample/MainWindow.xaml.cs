using HHDev.DataManagement.Api.Models;
using HHDev.DataManagement.ApiClientWrapper;
using HHDev.DataManagement.Client.Authentication;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfApiWrapperExample.Models;

namespace WpfApiWrapperExample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private const string API_KEY = "API KEY HERE";

        private ApiClient _apiClient;
        private ApiSetupModel _selectedSetup;
        private ApiCustomPropertyDefinitionModel _currentSetupDefinition;

        public string CurrentStatus { get; set; } = "";
        public bool UIEnabled { get; set; } = true;

        public ICommand AddSetupCommand { get; set; }
        public ICommand DeleteSetupCommand { get; set; }
        public ICommand AddAttachedFileToSetupCommand { get; set; }
        public ICommand DownloadAttachedFileFromSetupCommand { get; set; }
        public ICommand DeleteAttachedFileFromSetupCommand { get; set; }
        public ICommand SaveChangesInCurrentSetupCommand { get; set; }


        public ObservableCollection<ApiAccountInformationModel> Accounts { get; } = new ObservableCollection<ApiAccountInformationModel>();
        public ObservableCollection<ApiChampionshipModel> Championships { get; } = new ObservableCollection<ApiChampionshipModel>();
        public ObservableCollection<ApiEventModel> Events { get; } = new ObservableCollection<ApiEventModel>();
        public ObservableCollection<ApiEventCarModel> EventCars { get; } = new ObservableCollection<ApiEventCarModel>();
        public ObservableCollection<ApiSetupModel> Setups { get; } = new ObservableCollection<ApiSetupModel>();

        public ApiAccountInformationModel SelectedAccount { get; set; }                
        public ApiChampionshipModel SelectedChampionship { get; set; }
        public ApiEventModel SelectedEvent { get; set; }
        public ApiEventCarModel SelectedEventCar { get; set; }
        public ApiSetupModel SelectedSetup { get => _selectedSetup; set => _selectedSetup = value; }
        public EditSetupModel SelectedSetupFull { get; set; }      


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            var authManager = new AuthenticationManager(eAuthenticationMode.ApiKey, API_KEY);
            _apiClient = new ApiClient(authManager);

            Loaded += MainWindow_Loaded;

            AddSetupCommand = new DelegateCommand(AddSetup);
            DeleteSetupCommand = new DelegateCommand(DeleteSetup);
            AddAttachedFileToSetupCommand = new DelegateCommand(AddAttachedFileToSetup);
            DownloadAttachedFileFromSetupCommand = new DelegateCommand(DownloadAttachedFileFromSetup);
            DeleteAttachedFileFromSetupCommand = new DelegateCommand(DeleteAttachedFileFromSetup);
            SaveChangesInCurrentSetupCommand = new DelegateCommand(SaveChangesInCurrentSetup);

        }

        private void SetStatus(string message)
        {
            CurrentStatus = message;
            UIEnabled = false;
        }

        private void ClearStatus()
        {
            CurrentStatus = null;
            UIEnabled = true;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            SetStatus("Loading accounts");
            var accounts = await _apiClient.GetAllAccounts(new ApiGetOptions());
            ClearStatus();

            if (accounts.Success == false)
            {
                MessageBox.Show("Error retrieving accounts");
                return;
            }

            foreach (var account in accounts.ReturnValue)
            {
                Accounts.Add(account);
            }

            SelectedAccount = Accounts.FirstOrDefault();
        }

        private async void OnSelectedAccountChanged()
        {
            Championships.Clear();

            if (SelectedAccount == null)
            {
                return;
            }

            var accountId = SelectedAccount.Id;
            var accountName = SelectedAccount.Name;

            SetStatus("Loading championships");
            var championships = await _apiClient.GetAllChampionships(accountId, new ApiGetOptions()
            {
                ParametersToInclude = new List<string>()
                {
                    "Name",
                    "Events.Name",
                    "Events.Cars.Car.Number",
                    "Events.Cars.SetupDefinitionId",
                },
            });
            ClearStatus();

            if (championships.Success == false)
            {
                MessageBox.Show($"Error retrieving championships for account {accountName}");
                return;
            }

            foreach (var championship in championships.ReturnValue)
            {
                Championships.Add(championship);
            }

            SelectedChampionship = Championships.FirstOrDefault();
        }

        private void OnSelectedChampionshipChanged()
        {
            Events.Clear();

            if (SelectedChampionship == null)
            {
                return;
            }

            foreach (var @event in SelectedChampionship.Events)
            {
                Events.Add(@event);
            }

            SelectedEvent = Events.FirstOrDefault();
        }

        private void OnSelectedEventChanged()
        {
            EventCars.Clear();

            if (SelectedEvent == null)
            {
                return;
            }

            foreach (var eventCar in SelectedEvent.Cars)
            {
                EventCars.Add(eventCar);
            }

            SelectedEventCar = EventCars.FirstOrDefault();
        }

        private async void OnSelectedEventCarChanged()
        {
            await GetSetupsForCurrentEventCar(true);
        }

        private async void OnSelectedSetupChanged()
        {
            await LoadSelectedSetup();
        }

        private async Task GetSetupsForCurrentEventCar(bool selectDefaultSetup)
        {
            Setups.Clear();

            if (SelectedEvent == null || SelectedEventCar == null)
            {
                return;
            }

            var acconutId = SelectedAccount.Id;
            var eventId = SelectedEvent.Id;
            var carId = ((ApiCarModel)SelectedEventCar.Parameters["Car"]).Id;

            SetStatus("Loading setups");

            var setupDefinitonId = SelectedEventCar.Parameters["SetupDefinitionId"].ToString();

            var tasks = new List<Task>();

            Task<ApiGetResult<ApiCustomPropertyDefinitionModel>> definitionTask = null;

            if (_currentSetupDefinition == null || _currentSetupDefinition.Id != setupDefinitonId)
            {
                definitionTask = _apiClient.GetDefinitionById(SelectedAccount.Id, setupDefinitonId, new ApiGetOptions()
                {
                    ParametersToInclude = new List<string>()
                    {
                        "*",
                        "ParameterDefinitions.Name",
                        "ParameterDefinitions.PropertyType",
                        "PartParameterDefinitions.*",
                        "AssemblyParameterDefinitions.*",
                        "MathParameterDefinitions.*",
                    },
                });

                tasks.Add(definitionTask);
            }

            var setupsTask = _apiClient.GetAllSetupsForEventCar(acconutId, eventId, carId, new ApiGetOptions()
            {
                ParametersToInclude = new List<string>()
                {
                    "Name",
                },
            });
            tasks.Add(setupsTask);

            await Task.WhenAll(tasks);

            var setups = setupsTask.Result;                       

            ClearStatus();

            if (setups.Success == false)
            {
                MessageBox.Show($"Error retrieving setups for event {SelectedEvent.Name} - car {((ApiCarModel)SelectedEventCar.Parameters["Car"]).Number}");
                return;
            }

            if (definitionTask != null)
            {
                var definitions = definitionTask.Result;
                if (definitions.Success == false)
                {
                    MessageBox.Show($"Error retrieving setup definition for event {SelectedEvent.Name} - car {((ApiCarModel)SelectedEventCar.Parameters["Car"]).Number}");
                    return;
                }

                _currentSetupDefinition = definitions.ReturnValue;
            }

            foreach (var setup in setups.ReturnValue)
            {
                Setups.Add(setup);
            }

            if (selectDefaultSetup)
            {
                SelectedSetup = Setups.FirstOrDefault();
            }
        }

        private async Task LoadSelectedSetup()
        {
            SelectedSetupFull = null;

            if (SelectedSetup == null)
            {
                return;
            }

            SetStatus("Loading selected setup");
            var setup = await _apiClient.GetSetupById(SelectedAccount.Id, SelectedSetup.Id, new ApiGetOptions()
            {
                ParametersToInclude = new List<string>()
                {
                    "*",
                    "AttachedFiles.FileName",
                }
            });
            ClearStatus();

            if (setup.Success == false)
            {
                MessageBox.Show($"Error retrieving setup {SelectedSetup.Name}");
                return;
            }

            LoadSetup(setup.ReturnValue);           
        }

        private void LoadSetup(ApiSetupModel setup)
        {
            SelectedSetupFull = new EditSetupModel(setup, _currentSetupDefinition);           
        }

        private string GetSelectedCarId()
        {
            if (SelectedEventCar == null)
            {
                throw new NullReferenceException("SelectedEventCar is null");
            }

            return ((ApiCarModel)SelectedEventCar.Parameters["Car"]).Id;
        }

        private async void AddSetup()
        {
            if (SelectedAccount == null || SelectedEvent == null || SelectedEventCar == null)
            {
                return;
            }

            SetStatus("Adding setup");
            var addResult = await _apiClient.AddSetup(SelectedAccount.Id, SelectedEvent.Id, GetSelectedCarId(), new CreateModel()
            {
                CopyFromLast = true,                
            });
            ClearStatus();

            if (addResult.Success == false)
            {
                MessageBox.Show("Error creating setup");
                return;
            }

            await GetSetupsForCurrentEventCar(false);

            _selectedSetup = Setups.First(x => x.Id == addResult.ReturnValue.Id);
            OnPropertyChanged(nameof(SelectedSetup));

            LoadSetup(addResult.ReturnValue);
        }

        private async void DeleteSetup() 
        {
            if (SelectedAccount == null || SelectedSetup == null)
            {
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete the selected setup?", "Delete setup", MessageBoxButton.YesNoCancel) != MessageBoxResult.Yes)
            {
                return;
            }

            SetStatus("Deleting setup");
            var result = await _apiClient.DeleteSetup(SelectedAccount.Id, SelectedSetup.Id);
            ClearStatus();

            if (result == false)
            {
                MessageBox.Show("Setup could not be deleted");
                return;
            }

            await GetSetupsForCurrentEventCar(true);
        }

        private async void AddAttachedFileToSetup()
        {
            if (SelectedAccount == null || SelectedSetup == null)
            {
                return;
            }

            var openFileDialog = new OpenFileDialog();
            var openFileDialogResult = openFileDialog.ShowDialog();

            if (openFileDialogResult.HasValue == false || openFileDialogResult.Value == false)
            {
                return;
            }

            var filePath = openFileDialog.FileName;

            SetStatus("Uploading file");
            var uploadResult = await _apiClient.AddAttachmentToSetup(SelectedAccount.Id, SelectedSetup.Id, new ApiPrepareUploadModel()
            {
                AutoDownload =false,
                CustomPropertyAttachedFileName = null,
                FileName = Path.GetFileName(filePath),                
            }, filePath);            

            if (uploadResult.AddAttachmentStatus == eAddAttachmentStatus.FailedToAdd)
            {
                ClearStatus();
                MessageBox.Show("Failed to add attachement");
                return;
            }

            while (uploadResult.AddAttachmentStatus == eAddAttachmentStatus.FailedToUpload ||
                   uploadResult.AddAttachmentStatus == eAddAttachmentStatus.FailedToUpdateServerStatus)
            {
                SetStatus($"Attachment failed to upload for reason {uploadResult.AddAttachmentStatus}.  Retrying...");
                uploadResult = await _apiClient.RetrySetupAttachment(SelectedAccount.Id, uploadResult);
            }

            ClearStatus();

            await LoadSelectedSetup();
        }

        private async void DownloadAttachedFileFromSetup()
        {
            if (SelectedAccount == null || SelectedSetup == null || SelectedSetupFull?.SelectedAttachedFile == null)
            {
                return;
            }

            SetStatus("Getting attached file info");
            var attachedFileInfos = await _apiClient.GetSetupAttachmentsById(SelectedAccount.Id, SelectedSetup.Id, new[] { SelectedSetupFull.SelectedAttachedFile.Id });
            ClearStatus();

            if (attachedFileInfos.Success == false)
            {
                MessageBox.Show("There was an error download the attached files");
                return;
            }

            var url = attachedFileInfos.ReturnValue[0].SignedURL;

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = SelectedSetupFull.SelectedAttachedFile.Parameters["FileName"].ToString();
            var saveFileDialogResult = saveFileDialog.ShowDialog();

            if (saveFileDialogResult.HasValue == false || saveFileDialogResult.Value == false)
            {
                return;
            }

            SetStatus("Downloading file");
            using var client = new HttpClient();
            using var s = await client.GetStreamAsync(url);
            using var fs = new FileStream(saveFileDialog.FileName, FileMode.OpenOrCreate);
            await s.CopyToAsync(fs);
            ClearStatus();

            MessageBox.Show("File downloaded");
        }

        private async void DeleteAttachedFileFromSetup()
        {
            if (SelectedAccount == null || SelectedSetup == null || SelectedSetupFull?.SelectedAttachedFile == null)
            {
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete the selected attached file?", "Delete attached file", MessageBoxButton.YesNoCancel) != MessageBoxResult.Yes)
            {
                return;
            }

            SetStatus("Deleting attached file");
            var result = await _apiClient.DeleteSetupAttachment(SelectedAccount.Id, SelectedSetup.Id, SelectedSetupFull.SelectedAttachedFile.Id);
            ClearStatus();

            if (result == false)
            {
                MessageBox.Show("Could not delete attached file");
                return;
            }

            await LoadSelectedSetup();
        }

        private async void SaveChangesInCurrentSetup()
        {
            if (SelectedAccount == null || SelectedSetup == null || SelectedSetupFull == null)
            {
                return;
            }

            var parameterUpdates = new List<ParameterUpdateModel>();

            foreach (var item in SelectedSetupFull.SetupParameters)
            {
                if (item.IsDirty)
                {
                    parameterUpdates.Add(new ParameterUpdateModel(item.Name, item.GetValueString()));
                }
            }

            if (parameterUpdates.Count == 0)
            {
                MessageBox.Show("No changes detected");
                return;
            }

            var selectedSetupId = SelectedSetup.Id;

            SetStatus("Send updates to API");
            var result = await _apiClient.UpdateSetup(SelectedAccount.Id, SelectedSetup.Id, new UpdateModel()
            {
                ParameterUpdates = parameterUpdates,
            });
            ClearStatus();

            await GetSetupsForCurrentEventCar(false);

            SelectedSetup = Setups.FirstOrDefault(x => x.Id == selectedSetupId) ?? Setups.FirstOrDefault();            
        }
    }
}