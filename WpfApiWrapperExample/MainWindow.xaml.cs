using HHDev.Core.Enums;
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
        private HistoryApiClient _apiClient;
        private ApiSetupModel _selectedSetup;
        private ApiCustomPropertyDefinitionModel _currentSetupDefinition;

        public string CurrentStatus { get; set; } = "";
        public bool UIEnabled { get; set; } = true;
        public bool RememberApiKey { get; set; } = true;

        public ICommand AddSetupCommand { get; set; }
        public ICommand DeleteSetupCommand { get; set; }
        public ICommand AddAttachedFileToSetupCommand { get; set; }
        public ICommand DownloadAttachedFileFromSetupCommand { get; set; }
        public ICommand DeleteAttachedFileFromSetupCommand { get; set; }
        public ICommand SaveChangesInCurrentSetupCommand { get; set; }
        public ICommand ShowSetupHistoryCommand { get; set; }
        public ICommand ConnectCommand { get; set; }


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

            // The API client is created when the user enters an API key and clicks "Connect" (see Connect()).
            // If a key was saved last time, it is filled in and used automatically on start-up.
            Loaded += MainWindow_Loaded;

            ConnectCommand = new DelegateCommand(Connect);
            AddSetupCommand = new DelegateCommand(AddSetup);
            DeleteSetupCommand = new DelegateCommand(DeleteSetup);
            AddAttachedFileToSetupCommand = new DelegateCommand(AddAttachedFileToSetup);
            DownloadAttachedFileFromSetupCommand = new DelegateCommand(DownloadAttachedFileFromSetup);
            DeleteAttachedFileFromSetupCommand = new DelegateCommand(DeleteAttachedFileFromSetup);
            SaveChangesInCurrentSetupCommand = new DelegateCommand(SaveChangesInCurrentSetup);
            ShowSetupHistoryCommand = new DelegateCommand(ShowSetupHistory);

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

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var savedApiKey = ApiKeyStore.Load();

            if (string.IsNullOrEmpty(savedApiKey) == false)
            {
                ApiKeyBox.Password = savedApiKey;
                Connect();
            }
        }

        private void ApiKeyBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Connect();
            }
        }

        private async void Connect()
        {
            var apiKey = ApiKeyBox.Password?.Trim();

            if (string.IsNullOrEmpty(apiKey))
            {
                MessageBox.Show("Please enter an API key.");
                return;
            }

            ResetLoadedData();

            var authManager = new AuthenticationManager(eAuthenticationMode.ApiKey, apiKey);

            if (_apiClient == null)
            {
                // HistoryApiClient is an ApiClientV2 that also exposes the /{Entity}/History/{id} endpoints
                _apiClient = new HistoryApiClient(authManager);
            }
            else
            {
                // Reuse the existing client (and its HttpClient) with the new key
                _apiClient.UpdateAuthenticationManager(authManager);
            }

            if (RememberApiKey == false)
            {
                ApiKeyStore.Delete();
            }

            var connected = await LoadAccountsAsync();

            if (connected && RememberApiKey)
            {
                ApiKeyStore.Save(apiKey);
            }
        }

        private void ResetLoadedData()
        {
            SelectedSetupFull = null;
            SelectedSetup = null;
            SelectedEventCar = null;
            SelectedEvent = null;
            SelectedChampionship = null;
            SelectedAccount = null;

            Setups.Clear();
            EventCars.Clear();
            Events.Clear();
            Championships.Clear();
            Accounts.Clear();

            _currentSetupDefinition = null;
        }

        /// <returns>True when the API accepted the key.</returns>
        private async Task<bool> LoadAccountsAsync()
        {
            SetStatus("Loading accounts");
            var accounts = await _apiClient.GetAllAccountsAsync();
            ClearStatus();

            if (accounts.Success == false)
            {
                MessageBox.Show($"Error retrieving accounts (check the API key): {DescribeFailure(accounts)}");
                return false;
            }

            if (accounts.Value.Count == 0)
            {
                MessageBox.Show("Connected, but no accounts are available for this API key.");
                return true;
            }

            foreach (var account in accounts.Value)
            {
                Accounts.Add(account);
            }

            SelectedAccount = Accounts.FirstOrDefault();
            return true;
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
            var championships = await _apiClient.GetAllChampionshipsAsync(accountId, new ApiGetOptions()
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
                MessageBox.Show($"Error retrieving championships for account {accountName}: {DescribeFailure(championships)}");
                return;
            }

            foreach (var championship in championships.Value)
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

            Task<ApiResult<ApiCustomPropertyDefinitionModel>> definitionTask = null;

            if (_currentSetupDefinition == null || _currentSetupDefinition.Id != setupDefinitonId)
            {
                definitionTask = _apiClient.GetDefinitionByIdAsync(SelectedAccount.Id, setupDefinitonId, new ApiGetOptions()
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

            var setupsTask = _apiClient.GetAllSetupsForEventCarAsync(acconutId, eventId, carId, new ApiGetOptions()
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
                MessageBox.Show($"Error retrieving setups for event {SelectedEvent.Name} - car {((ApiCarModel)SelectedEventCar.Parameters["Car"]).Number}: {DescribeFailure(setups)}");
                return;
            }

            if (definitionTask != null)
            {
                var definitions = definitionTask.Result;
                if (definitions.Success == false)
                {
                    MessageBox.Show($"Error retrieving setup definition for event {SelectedEvent.Name} - car {((ApiCarModel)SelectedEventCar.Parameters["Car"]).Number}: {DescribeFailure(definitions)}");
                    return;
                }

                _currentSetupDefinition = definitions.Value;
            }

            foreach (var setup in setups.Value)
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
            var setup = await _apiClient.GetSetupByIdAsync(SelectedAccount.Id, SelectedSetup.Id, new ApiGetOptions()
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
                MessageBox.Show($"Error retrieving setup {SelectedSetup.Name}: {DescribeFailure(setup)}");
                return;
            }

            LoadSetup(setup.Value);           
        }

        private static string DescribeFailure(ApiResult result)
        {
            var statusCode = result.StatusCode.HasValue ? $" ({(int)result.StatusCode})" : string.Empty;

            return string.IsNullOrWhiteSpace(result.Message)
                ? $"{result.Status}{statusCode}"
                : $"{result.Status}{statusCode} - {result.Message}";
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
            var addResult = await _apiClient.AddSetupAsync(SelectedAccount.Id, SelectedEvent.Id, GetSelectedCarId(), new CreateModel()
            {
                CopyFromLast = true,                
            });
            ClearStatus();

            if (addResult.Success == false)
            {
                MessageBox.Show($"Error creating setup: {DescribeFailure(addResult)}");
                return;
            }

            await GetSetupsForCurrentEventCar(false);

            _selectedSetup = Setups.First(x => x.Id == addResult.Value.Id);
            OnPropertyChanged(nameof(SelectedSetup));

            LoadSetup(addResult.Value);
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
            var result = await _apiClient.DeleteSetupAsync(SelectedAccount.Id, SelectedSetup.Id);
            ClearStatus();

            if (result.Success == false)
            {
                MessageBox.Show($"Setup could not be deleted: {DescribeFailure(result)}");
                return;
            }

            await GetSetupsForCurrentEventCar(true);
        }

        private async void ShowSetupHistory()
        {
            if (SelectedAccount == null || SelectedSetup == null)
            {
                return;
            }

            var setupName = SelectedSetup.Name;

            SetStatus("Loading setup history");
            var history = await _apiClient.GetSetupHistoryAsync(SelectedAccount.Id, SelectedSetup.Id);
            ClearStatus();

            if (history.Success == false)
            {
                MessageBox.Show($"Error retrieving history for setup {setupName}: {DescribeFailure(history)}");
                return;
            }

            if (history.Value == null || history.Value.Count == 0)
            {
                MessageBox.Show($"No history found for setup {setupName}");
                return;
            }

            var historyWindow = new SetupHistoryWindow(setupName, history.Value)
            {
                Owner = this,
            };
            historyWindow.Show();
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
            var uploadResult = await _apiClient.AddAttachmentToSetupAsync(SelectedAccount.Id, SelectedSetup.Id, new ApiPrepareUploadModel()
            {
                AutoDownloadMode = eAutoDownloadMode.No,
                CustomPropertyAttachedFileName = null,
                FileName = Path.GetFileName(filePath),                
            }, filePath);            

            if (uploadResult.Value.AddAttachmentStatus == eAddAttachmentStatus.FailedToAdd)
            {
                ClearStatus();
                MessageBox.Show($"Failed to add attachement: {DescribeFailure(uploadResult)}");
                return;
            }

            while (uploadResult.Value.AddAttachmentStatus == eAddAttachmentStatus.FailedToUpload ||
                   uploadResult.Value.AddAttachmentStatus == eAddAttachmentStatus.FailedToUpdateServerStatus)
            {
                SetStatus($"Attachment failed to upload for reason {uploadResult.Value.AddAttachmentStatus}.  Retrying...");
                uploadResult = await _apiClient.RetrySetupAttachmentAsync(SelectedAccount.Id, uploadResult.Value);
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
            var attachedFileInfos = await _apiClient.GetSetupAttachmentsByIdAsync(SelectedAccount.Id, SelectedSetup.Id, new[] { SelectedSetupFull.SelectedAttachedFile.Id });
            ClearStatus();

            if (attachedFileInfos.Success == false)
            {
                MessageBox.Show($"There was an error download the attached files: {DescribeFailure(attachedFileInfos)}");
                return;
            }

            var url = attachedFileInfos.Value[0].SignedURL;

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
            var result = await _apiClient.DeleteSetupAttachmentAsync(SelectedAccount.Id, SelectedSetup.Id, SelectedSetupFull.SelectedAttachedFile.Id);
            ClearStatus();

            if (result.Success == false)
            {
                MessageBox.Show($"Could not delete attached file: {DescribeFailure(result)}");
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
            var result = await _apiClient.UpdateSetupAsync(SelectedAccount.Id, SelectedSetup.Id, new UpdateModel()
            {
                ParameterUpdates = parameterUpdates,
            });
            ClearStatus();

            if (result.Success == false)
            {
                MessageBox.Show($"Could not save changes to setup {SelectedSetup.Name}: {DescribeFailure(result)}");
                return;
            }

            await GetSetupsForCurrentEventCar(false);

            SelectedSetup = Setups.FirstOrDefault(x => x.Id == selectedSetupId) ?? Setups.FirstOrDefault();            
        }
    }
}