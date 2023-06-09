﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FuzzySharp;
using PhotoNotes.Extensions;
using PhotoNotes.Models;
using PhotoNotes.Services;
using PhotoNotes.Views;
using System.Collections;
using System.Collections.ObjectModel;

namespace PhotoNotes.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private static readonly ObservableCollection<FileItem> EmptyFileCollection = new(Enumerable.Empty<FileItem>());
        private static readonly ObservableCollection<FolderItem> EmptyFolderCollection = new(Enumerable.Empty<FolderItem>());
        private readonly IPhotoManagement photoManagement;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(CurrTitle))]
        [NotifyPropertyChangedFor(nameof(ShowHomeButton))]
        [NotifyPropertyChangedFor(nameof(IsOnHomeScreen))]
        private string? currFolder = null;

        public MainPageViewModel(IPhotoManagement photoManagement)
        {
            this.photoManagement = photoManagement;
            (Folders, Files) = (photoManagement.MainFolder.Folders, photoManagement.MainFolder.Files);

            Files.CollectionChanged += (_, e) => OnPropertyChanged(nameof(HasFiles));
            Folders.CollectionChanged += (_, e) => OnPropertyChanged(nameof(HasFolders));

            var callBack = new CollectionSynchronizationCallback(static (IEnumerable collection, object context, Action accessMethod, bool writeAccess) =>
            {
                lock (context)
                {
                    accessMethod?.Invoke();
                }
            });

            //BindingBase.EnableCollectionSynchronization(Files, null, callBack);
            //BindingBase.EnableCollectionSynchronization(Folders, null, callBack);
            //BindingBase.EnableCollectionSynchronization(EmptyFolderCollection, null, callBack);
            //BindingBase.EnableCollectionSynchronization(EmptyFileCollection, null, callBack);
        }

        //Search Bar Bug Workaround

        #region Search Bar Bug Workaround

        public double ScreenWidth => DeviceDisplay.Current.MainDisplayInfo.Width / DeviceDisplay.Current.MainDisplayInfo.Density;

        #endregion Search Bar Bug Workaround

        public string CurrTitle => CurrFolder ?? "Files";
        public ObservableCollection<FileItem> Files { get; set; }
        public ObservableCollection<FolderItem> Folders { get; set; }
        public bool HasFiles => Files.Any();

        public bool HasFolders => Folders.Any();
        public bool IsOnHomeScreen => !ShowHomeButton;
        public bool ShowHomeButton => CurrFolder is not null;

        [RelayCommand]
        public void BackToHome()
        {
            Folders = photoManagement.MainFolder.Folders;
            Files = photoManagement.MainFolder.Files;
            EmptyFileCollection.Clear();
            EmptyFolderCollection.Clear();
            CurrFolder = null;
            OnPropertyChanged(nameof(Files));
            OnPropertyChanged(nameof(HasFolders));
            OnPropertyChanged(nameof(HasFiles));
        }

        [RelayCommand]
        public void BackToMain()
        {
            CurrFolder = null;
            Folders = photoManagement.MainFolder.Folders;
            OnPropertyChanged(nameof(Folders));
            OnPropertyChanged(nameof(HasFolders));
            Files = photoManagement.MainFolder.Files;
            OnPropertyChanged(nameof(Files));
            OnPropertyChanged(nameof(HasFiles));
        }

        [RelayCommand]
        public void DeleteFileItem(string name)
        {
            if (photoManagement.MainFolder.Files.Any(x => x.CurrPath == name))
            {
                photoManagement.DeleteFile(null, name);
            }
            else
            {
                photoManagement.DeleteFile(photoManagement.MainFolder.Folders.First(x => x.Files.Any(y => y.CurrPath == name)).ShortName, name);
            }
            //Indicated if user is searching for an item or not
            if (!ReferenceEquals(Files, photoManagement.MainFolder.Files))
            {
                Files.RemoveAll(x => x.CurrPath == name);
                MainThread.BeginInvokeOnMainThread(async () => await Search());
            }
            
        }

        [RelayCommand]
        public async Task DeleteFolderItem(string name)
        {
            var folder = Folders.First(x => x.CurrPath == name);
            if (folder.NumFiles > 0)
            {
                if (!await Shell.Current.DisplayAlert("Warning", $"Are you sure you want to delete the nested {folder.NumFiles} photo(s) inside this folder?", "Yes", "No"))
                {
                    return;
                }
            }

            photoManagement.DeleteFolder(name);
        }

        [RelayCommand]
        public async Task GoToSettings() => await Shell.Current.GoToAsync(nameof(SettingsView), true);

        [RelayCommand]
        public void OnTextChanged(bool isEmpty)
        {
            if (isEmpty)
            {
                BackToMain();
            }
        }

        [RelayCommand(AllowConcurrentExecutions = true)]
        public async Task Search()
        {
            if (string.IsNullOrWhiteSpace(SearchText)) { return; }
            EmptyFolderCollection.Clear();
            EmptyFileCollection.Clear();
            Folders = EmptyFolderCollection;
            Files = EmptyFileCollection;
            if (Preferences.Default.Get(PreferencesService.FuzzyStringMatchKey, true))
            {
                var fuzzyStringMatchingThreshold = double.Parse(Preferences.Default.Get(PreferencesService.FuzzyStringMatchThresholdKey, "90.0"));
                var t1 = Task.Run(() =>
                {
                    Folders.AddRange(photoManagement.MainFolder.Folders.Where(x => Fuzz.PartialRatio(x.ShortName, SearchText) > fuzzyStringMatchingThreshold || x.Files.Any(y => Fuzz.PartialRatio(y.ShortName, SearchText) > fuzzyStringMatchingThreshold)));
                });
                var t2 = Task.Run(() =>
                {
                    Files.AddRange(photoManagement.MainFolder.Files.Where(x => Fuzz.PartialRatio(x.ShortName, SearchText) > fuzzyStringMatchingThreshold));
                    photoManagement.MainFolder.Folders.SelectMany(x => x.Files).Where(x => Fuzz.PartialRatio(x.ShortName, SearchText) > fuzzyStringMatchingThreshold).ForEach(Files.Add);
                });

                await Task.WhenAll(t1, t2);
            }
            else
            {
                var t1 = Task.Run(() =>
                {
                    Folders.AddRange(photoManagement.MainFolder.Folders.Where(x => x.ShortName.Contains(SearchText) || x.Files.Any(y => y.ShortName.Contains(SearchText))));
                });
                var t2 = Task.Run(() =>
                {
                    Files.AddRange(photoManagement.MainFolder.Files.Where(x => x.ShortName.Contains(SearchText)));
                    photoManagement.MainFolder.Folders.SelectMany(x => x.Files).Where(x => x.ShortName.Contains(SearchText)).ForEach(Files.Add);
                });

                await Task.WhenAll(t1, t2);
            }

            OnPropertyChanged(nameof(Folders));
            OnPropertyChanged(nameof(HasFolders));
            OnPropertyChanged(nameof(Files));
            OnPropertyChanged(nameof(HasFiles));
        }

        [RelayCommand]
        public async Task SelectFile(string name)
        {
            //TODO
            await Shell.Current.GoToAsync($"secret/{nameof(PhotoView)}?{nameof(PhotoViewModel.PhotoSource)}={name}");
        }

        [RelayCommand]
        public async Task SelectFolder(string name)
        {
            var folder = Folders.Single(x => x.CurrPath == name);
            await Shell.Current.GoToAsync($"{nameof(FolderView)}", new Dictionary<string, object>
            {
                { nameof(FolderViewModel.Folder), folder }
            });
            /*CurrFolder = folder.ShortName;

            Files = folder.Files;
            Folders = EmptyFolderCollection;

            OnPropertyChanged(nameof(HasFolders));
            OnPropertyChanged(nameof(Files));*/
        }
    }
}