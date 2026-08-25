using Microsoft.Extensions.Logging;
using AiNotetakerApp.Data;
using AiNotetakerApp.ViewModels;

namespace AiNotetakerApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            // 1. Register Services - Register the Database Service as a Singleton
            builder.Services.AddSingleton<DatabaseService>();
            // 2. Register ViewModels
            builder.Services.AddSingleton<MainViewModel>();
            // 3. Register Pages
            builder.Services.AddSingleton<MainPage>();

            return builder.Build();
        }
    }
}
