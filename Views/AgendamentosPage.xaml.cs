using System;
using Microsoft.Maui.Controls;
using Xibo_CMVV.Models;
using Xibo_CMVV.Services;

namespace Xibo_CMVV.Views
{
    public partial class AgendamentosPage : ContentPage
    {
        private readonly XiboApiService _api;

        public AgendamentosPage()
        {
            InitializeComponent();
            _api = new XiboApiService();
            CarregarAgendamentos();
        }

        private async void CarregarAgendamentos()
        {
            try
            {
                bool auth = await _api.AuthenticateAsync();
                if (!auth)
                {
                    await DisplayAlert("Erro", "Falha na autenticação.", "OK");
                    return;
                }

                var agendamentos = await _api.GetAgendamentosAsync();
                agendamentosList.ItemsSource = agendamentos;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Falha ao carregar agendamentos: {ex.Message}", "OK");
            }
        }

        private async void OnCriarAgendamentoClicked(object sender, EventArgs e)
        {
            try
            {
                bool auth = await _api.AuthenticateAsync();
                if (!auth)
                {
                    await DisplayAlert("Erro", "Falha na autenticação.", "OK");
                    return;
                }

                var novoAgendamento = new Agendamento
                {
                    EventTypeId = 1,               // ajuste conforme necessário
                    EventName = tituloInput.Text,
                    FromDt = dataInput.Text,  // mapeia para "fromDt" no JSON
                    ToDt = dataInput.Text,  // mapeia para "toDt" no JSON
                    IsPriority = false,
                    DisplayGroupIds = "1",             // ajuste conforme necessário
                    LayoutId = 1,               // ajuste conforme necessário
                    DisplayOrder = 0
                };

                bool success = await _api.CriarAgendamentoAsync(novoAgendamento);

                if (success)
                {
                    mensagemErro.IsVisible = false;
                    await DisplayAlert("Sucesso", "Agendamento criado com sucesso!", "OK");
                    CarregarAgendamentos();
                }
                else
                {
                    mensagemErro.Text = "Erro ao criar agendamento.";
                    mensagemErro.IsVisible = true;
                }
            }
            catch (Exception ex)
            {
                mensagemErro.Text = $"Erro: {ex.Message}";
                mensagemErro.IsVisible = true;
            }
        }
    }
}