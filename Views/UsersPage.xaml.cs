using Microsoft.Maui.Controls;
using Xibo_CMVV.Services;
using Xibo_CMVV.Models;

namespace Xibo_CMVV.Views
{
    public partial class UsersPage : ContentPage
    {
        private readonly XiboApiService _api = new();
        private int? editingUserId = null;

        public UsersPage()
        {
            InitializeComponent();
            Loaded += UsersPage_Loaded;
        }

        private async void UsersPage_Loaded(object sender, EventArgs e)
        {
            await LoadUsersAsync();
        }

        private async Task LoadUsersAsync()
        {
            statusLabel.TextColor = Colors.Green;
            statusLabel.Text = "A carregar utilizadores...";

            if (!await _api.AuthenticateAsync())
            {
                statusLabel.TextColor = Colors.Red;
                statusLabel.Text = "Falha na autenticação.";
                return;
            }

            var users = await _api.GetUsersAsync();
            usersList.ItemsSource = users;
            statusLabel.Text = $"Total: {users.Count} utilizadores";
        }

        private async void OnSubmitClicked(object sender, EventArgs e)
        {
            var username = usernameEntry.Text?.Trim();
            var email = emailEntry.Text?.Trim();
            var password = passwordEntry.Text;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                statusLabel.TextColor = Colors.Red;
                statusLabel.Text = "Preencha todos os campos.";
                return;
            }

            bool success;
            if (editingUserId.HasValue)
            {
                var updatedUser = new XiboUser
                {
                    userId = editingUserId.Value,
                    userName = username,
                    email = email,
                    userType = "1"
                };

                success = await _api.UpdateUserAsync(updatedUser);
                statusLabel.Text = success ? "Utilizador atualizado com sucesso!" : "Erro ao atualizar utilizador.";
            }
            else
            {
                var newUser = new XiboUser
                {
                    userName = username,
                    email = email,
                    password = password,
                    userType = "User"
                };

                success = await _api.CreateUserAsync(newUser) != null;
                statusLabel.Text = success ? "Utilizador criado com sucesso!" : "Erro ao criar utilizador.";
            }

            statusLabel.TextColor = success ? Colors.Green : Colors.Red;

            if (success)
            {
                usernameEntry.Text = emailEntry.Text = passwordEntry.Text = "";
                submitButton.Text = "Criar Utilizador";
                editingUserId = null;
                await LoadUsersAsync();
            }
        }

        private void OnEditClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is XiboUser user)
            {
                editingUserId = user.userId;
                usernameEntry.Text = user.userName;
                emailEntry.Text = user.email;
                passwordEntry.Text = "";
                submitButton.Text = "Guardar Alterações";
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is int userId)
            {
                bool confirm = await DisplayAlert("Confirmar", "Eliminar este utilizador?", "Sim", "Não");
                if (!confirm) return;

                bool deleted = await _api.DeleteUserAsync(userId);
                statusLabel.TextColor = deleted ? Colors.Green : Colors.Red;
                statusLabel.Text = deleted ? "Utilizador eliminado." : "Erro ao eliminar utilizador.";

                if (deleted)
                    await LoadUsersAsync();
            }
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            usernameEntry.Text = emailEntry.Text = passwordEntry.Text = "";
            submitButton.Text = "Criar Utilizador";
            editingUserId = null;
            statusLabel.Text = "";
        }

        private async void OnRefreshClicked(object sender, EventArgs e)
        {
            await LoadUsersAsync();
        }
    }
}
