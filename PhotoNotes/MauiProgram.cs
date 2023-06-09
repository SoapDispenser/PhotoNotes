﻿using Camera.MAUI;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using PhotoNotes.Services;
using PhotoNotes.ViewModels;
using PhotoNotes.Views;

namespace PhotoNotes;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();


        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<MainPageViewModel>();
        builder.Services.AddTransient<PhotoPage>();
        builder.Services.AddTransient<SavePhotoViewModel>();
        builder.Services.AddTransient<SavePhotoView>();
        builder.Services.AddTransient<PhotoViewModel>();
        builder.Services.AddTransient<PhotoView>();
        builder.Services.AddTransient<FolderViewModel>();
        builder.Services.AddTransient<FolderView>();
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<SettingsView>();
        builder.Services.AddSingleton<IPhotoManagement, PhotoManagement>();
        PreferencesService.Init();


        builder
            .UseMauiCameraView()
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("RobotoMono-Regular.ttf", "RobotoMonoRegular");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("materialdesignicons-webfont.ttf", "MaterialDesign");


            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
