namespace AiNotetakerApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(MeetingDetailPage), typeof(MeetingDetailPage));
        }
    }
}
