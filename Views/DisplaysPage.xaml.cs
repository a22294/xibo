using Microsoft.Maui.Controls;
using Xibo_CMVV.Services;

namespace Xibo_CMVV.Views
{
    public partial class DisplaysPage : ContentPage
    {
        private readonly XiboApiService _api = new();

        public DisplaysPage()
        {
            InitializeComponent();
            LoadDisplays();
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadDisplays();
        }

        private async Task LoadDisplays()
        {
            bool auth = await _api.AuthenticateAsync();
            if (!auth)
            {
                await DisplayAlert("Erro", "Falha na autenticação com o Xibo.", "OK");
                return;
            }

            var displays = await _api.GetDisplaysAsync();
            DisplaysList.ItemsSource = displays;
        }
    }
}
