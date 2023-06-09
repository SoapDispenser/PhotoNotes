using CommunityToolkit.Maui.Core.Platform;
using PhotoNotes.Services;
using PhotoNotes.ViewModels;

namespace PhotoNotes.Views;

public partial class SavePhotoView : ContentPage
{
    public SavePhotoView(SavePhotoViewModel vm)
    {
        InitializeComponent();
        this.BindingContext = vm;
        Loaded += (_, _) => FileNameEntry.Focus();
        Loaded += (_, _) =>
        {
            if (Preferences.Default.Get(PreferencesService.SaveToFolderKey, false))
            {
                SaveToFolderToggleButton.PerformClick();
            }
        };
    }

    private async void SaveButton_Clicked(object sender, EventArgs e)
    {
        await FileNameEntry.HideKeyboardAsync(new CancellationToken());
    }
}