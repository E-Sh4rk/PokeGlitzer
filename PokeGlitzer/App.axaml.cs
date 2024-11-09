using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;

namespace PokeGlitzer
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            ResLoader.Initialize();
            Settings.Initialize();
            GlitzerSimulation.Initialize();
            PersonalInfo.Initialize();
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow(desktop.Args);
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
