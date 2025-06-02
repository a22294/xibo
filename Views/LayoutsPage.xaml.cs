using Microsoft.Maui;
using Microsoft.Maui.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xibo_CMVV.Models;
using Xibo_CMVV.Services;

namespace Xibo_CMVV.Views
{
    public partial class LayoutsPage : ContentPage
    {
        private readonly XiboApiService _api = new();
        private readonly ObservableCollection<XiboLayout> _layoutList = new();
        private XiboLayout _selectedLayout;
        private string _selectedImagePath;

        public LayoutsPage()
        {
            InitializeComponent();
            layoutsList.ItemsSource = _layoutList;

            // Garantir que carrega ao abrir a página
            Appearing += (s, e) => LoadLayouts();
        }

        private async void LoadLayouts()
        {
            try
            {
                var layouts = await _api.GetLayoutsAsync();
                _layoutList.Clear();

                if (layouts != null && layouts.Any())
                {
                    foreach (var layout in layouts)
                    {
                        layout.assignedDisplayText = "Nenhum display atribuído"; // Apenas visual
                        _layoutList.Add(layout);
                    }
                }
                else
                {
                    await DisplayAlert("Aviso", "Nenhum layout encontrado.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao carregar layouts: {ex.Message}", "OK");
            }
        }

        private async void OnPickImageClicked(object sender, EventArgs e)
        {
            var fileResult = await FilePicker.PickAsync(new PickOptions { PickerTitle = "Escolher uma imagem" });
            if (fileResult != null)
            {
                _selectedImagePath = fileResult.FullPath;
                selectedImageLabel.Text = $"Imagem selecionada: {fileResult.FileName}";
            }
            else
            {
                selectedImageLabel.Text = "Nenhuma imagem selecionada";
            }
        }

        private async void OnCreateLayoutClicked(object sender, EventArgs e)
        {
            activityIndicator.IsVisible = true;
            try
            {
                var layout = new XiboLayout
                {
                    layout = layoutNameEntry.Text,
                    description = layoutDescriptionEntry.Text,
                    status = "Draft"
                };

                var created = await _api.CreateLayoutAsync(layout);
                if (created != null)
                {
                    if (!string.IsNullOrEmpty(_selectedImagePath))
                        await _api.UploadImageToLayoutAsync(created.layoutId, _selectedImagePath);

                    await DisplayAlert("Sucesso", "Layout criado com sucesso.", "OK");
                    LoadLayouts();
                }
                else
                {
                    await DisplayAlert("Erro", "Falha ao criar layout.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao criar layout: {ex.Message}", "OK");
            }
            finally
            {
                activityIndicator.IsVisible = false;
            }
        }

        private void OnLayoutSelected(object sender, SelectionChangedEventArgs e)
        {
            _selectedLayout = e.CurrentSelection.FirstOrDefault() as XiboLayout;
            if (_selectedLayout != null)
            {
                layoutNameEntry.Text = _selectedLayout.layout;
                layoutDescriptionEntry.Text = _selectedLayout.description;
            }
        }

        private async void OnUpdateLayoutClicked(object sender, EventArgs e)
        {
            if (_selectedLayout == null)
            {
                await DisplayAlert("Aviso", "Seleciona um layout primeiro.", "OK");
                return;
            }

            try
            {
                var updatedLayout = new XiboLayout
                {
                    layout = layoutNameEntry.Text,
                    description = layoutDescriptionEntry.Text,
                    status = "Published"
                };

                var result = await _api.UpdateLayoutAsync(_selectedLayout.layoutId, updatedLayout);
                if (result != null)
                {
                    await DisplayAlert("Sucesso", "Layout atualizado.", "OK");
                    LoadLayouts();
                }
                else
                {
                    await DisplayAlert("Erro", "Falha ao atualizar layout.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao atualizar layout: {ex.Message}", "OK");
            }
        }

        private async void OnDeleteLayoutClicked(object sender, EventArgs e)
        {
            if (_selectedLayout == null)
            {
                await DisplayAlert("Aviso", "Seleciona um layout primeiro.", "OK");
                return;
            }

            bool confirm = await DisplayAlert("Confirmar", "Eliminar este layout?", "Sim", "Não");
            if (!confirm) return;

            try
            {
                var success = await _api.DeleteLayoutAsync(_selectedLayout.layoutId);
                if (success)
                {
                    await DisplayAlert("Sucesso", "Layout eliminado.", "OK");
                    LoadLayouts();
                }
                else
                {
                    await DisplayAlert("Erro", "Falha ao eliminar layout.", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Erro", $"Erro ao eliminar layout: {ex.Message}", "OK");
            }
        }

        private async void OnAssignToDisplayClicked(object sender, EventArgs e)
        {
            if (sender is Button button && int.TryParse(button.CommandParameter.ToString(), out int layoutId))
            {
                var displays = await _api.GetDisplaysAsync();
                var displayOptions = string.Join("\n", displays.Select(d => $"{d.displayId} - {d.display}"));

                var input = await DisplayPromptAsync(
                    "Atribuir a Display",
                    $"Escolha o ID do display para atribuir o layout {layoutId}:\n{displayOptions}",
                    placeholder: "Introduza apenas o ID");

                if (int.TryParse(input, out int displayId))
                {
                    var confirm = await DisplayAlert(
                        "Confirmar",
                        $"Tens a certeza que queres atribuir o layout {layoutId} ao display {displayId}?",
                        "Sim", "Não");

                    if (confirm)
                    {
                        var success = await _api.AssignLayoutToDisplayAsync(displayId, layoutId);
                        if (success)
                        {
                            await DisplayAlert("Sucesso", $"Layout {layoutId} atribuído ao display {displayId}.", "OK");
                            LoadLayouts();
                        }
                        else
                        {
                            await DisplayAlert("Erro", "Não foi possível atribuir o layout. Verifica a API.", "OK");
                        }
                    }
                }
                else
                {
                    await DisplayAlert("Erro", "ID de display inválido.", "OK");
                }
            }
        }
    }
}
