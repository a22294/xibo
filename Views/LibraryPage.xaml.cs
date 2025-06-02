using System.Text.Json;
using Microsoft.Maui.Controls;
using Xibo_CMVV.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System;
using System.Collections.Generic;

namespace Xibo_CMVV.Views
{
    public partial class LibraryPage : ContentPage
    {
        private readonly XiboApiService _api = new();
        private ObservableCollection<MediaItem> _mediaList = new();

        public LibraryPage()
        {
            InitializeComponent();
            libraryList.ItemsSource = _mediaList;
        }

        private async void OnListLibraryClicked(object sender, EventArgs e)
        {
            statusLabel.Text = "🔄 A carregar biblioteca...";
            statusLabel.IsVisible = true;

            var mediaResponse = await _api.GetLibraryAsync();
            if (mediaResponse != null)
            {
                _mediaList.Clear();
                foreach (var item in mediaResponse)
                {
                    var mediaItem = new MediaItem
                    {
                        mediaId = item.GetProperty("mediaId").GetInt32(),
                        name = item.GetProperty("name").GetString(),
                        fileName = item.GetProperty("fileName").GetString(),
                        duration = item.GetProperty("duration").GetString(),
                        type = item.GetProperty("type").GetString(),
                        lastModifiedDate = item.GetProperty("lastModifiedDate").GetString()
                    };
                    _mediaList.Add(mediaItem);
                }
                statusLabel.Text = $"✅ {_mediaList.Count} items carregados.";
            }
            else
            {
                statusLabel.Text = "❌ Erro ao carregar a biblioteca.";
            }
        }

        private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
        {
            var searchText = e.NewTextValue.ToLower();
            var filteredList = _mediaList.Where(m => m.name.ToLower().Contains(searchText)).ToList();
            libraryList.ItemsSource = filteredList;
        }

        private async void OnDeleteMediaClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter != null && int.TryParse(button.CommandParameter.ToString(), out int mediaId))
            {
                var confirm = await DisplayAlert("Confirmar", $"Apagar ficheiro com ID {mediaId}?", "Sim", "Não");
                if (confirm)
                {
                    var success = await _api.DeleteMediaAsync(mediaId);
                    await DisplayAlert("Resultado", success ? "✅ Removido com sucesso" : "❌ Erro ao remover", "OK");

                    if (success)
                    {
                        var itemToRemove = _mediaList.FirstOrDefault(m => m.mediaId == mediaId);
                        if (itemToRemove != null)
                            _mediaList.Remove(itemToRemove);
                    }
                }
            }
        }

        private void OnDetailsClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter != null && int.TryParse(button.CommandParameter.ToString(), out int mediaId))
            {
                var media = _mediaList.FirstOrDefault(m => m.mediaId == mediaId);
                if (media != null)
                {
                    DisplayAlert("Detalhes do Media",
                        $"🆔 ID: {media.mediaId}\n" +
                        $"📛 Nome: {media.name}\n" +
                        $"📁 Ficheiro: {media.fileName}\n" +
                        $"⏱ Duração: {media.duration}\n" +
                        $"📦 Tipo: {media.type}\n" +
                        $"🗓 Modificado: {media.lastModifiedDate}",
                        "OK");
                }
            }
        }

        private async void OnExportCsvClicked(object sender, EventArgs e)
        {
            var csvLines = new List<string>
            {
                "mediaId,name,fileName,duration,type,lastModifiedDate"
            };

            foreach (var media in _mediaList)
            {
                var line = $"{media.mediaId},{media.name},{media.fileName},{media.duration},{media.type},{media.lastModifiedDate}";
                csvLines.Add(line);
            }

            var csvContent = string.Join(Environment.NewLine, csvLines);
            var fileName = $"MediaLibrary_{DateTime.Now:yyyyMMddHHmmss}.csv";
            var filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            File.WriteAllText(filePath, csvContent);

            await DisplayAlert("Exportação", $"✅ CSV exportado para:\n{filePath}", "OK");
        }
    }

    public class MediaItem
    {
        public int mediaId { get; set; }
        public string name { get; set; }
        public string fileName { get; set; }
        public string duration { get; set; }
        public string type { get; set; }
        public string lastModifiedDate { get; set; }
    }
}