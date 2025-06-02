using Microsoft.Maui.Controls;
using Xibo_CMVV.Views;

namespace Xibo_CMVV.Views
{
    public partial class HomePage : ContentPage
    {
        public HomePage()
        {
            InitializeComponent();
        }

        private async void OnUsersClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UsersPage());
        }

        private async void OnDisplaysClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new DisplaysPage());
        }

        private async void OnUploadClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new UploadPage());
        }

        private async void OnAgendamentosClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new AgendamentosPage());
        }

        private async void OnLayoutsClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LayoutsPage());
        }

        private async void OnLibraryClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LibraryPage());
        }
    }
}
