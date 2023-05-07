﻿using Camera.MAUI;
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
        builder.Services.AddTransient<PhotoPage>();
		builder.Services.AddTransient<SavePhotoViewModel>();
        builder.Services.AddTransient<SavePhotoView>();
        builder.Services.AddSingleton<IPhotoManagement, PhotoManagement>();


		builder
            .UseMauiCameraView()
            .UseMauiApp<App>()
			
			.ConfigureFonts(fonts =>
			{
                fonts.AddFont("RobotoMono-Regular.ttf", "RobotoMonoRegular");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                
            });

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
