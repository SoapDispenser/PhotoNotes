﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PhotoNotes.ViewModels
{
    [QueryProperty(nameof(PhotoSource), nameof(PhotoSource))]
    public partial class PhotoViewModel: ObservableObject
    {
        public string ShortPhotoName => Path.GetFileNameWithoutExtension(PhotoSource);
        public string PhotoSource { get; set; }

        public ImageSource ImageStream => ImageSource.FromStream(() => File.OpenRead(PhotoSource));

        [RelayCommand]
        public async Task GoBack()
        {
            await Shell.Current.GoToAsync("../", true);
        }
    }
}
