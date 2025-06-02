using Microsoft.Maui.Controls;
using Xibo_CMVV.Services;
using System.IO;
using System.Threading.Tasks;

namespace Xibo_CMVV.Views
{
    public partial class UploadPage : ContentPage
    {
        private readonly XiboApiService _api = new();

        public UploadPage()
        {
            InitializeComponent();
        }

        private async void OnBrowseClicked(object sender, EventArgs e)
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "Selecionar um ficheiro"
            });

            if (result != null)
            {
                var fileInfo = new FileInfo(result.FullPath);
                filePathEntry.Text = result.FullPath;
                mediaNameEntry.Text = Path.GetFileName(result.FullPath);

                // Mostrar info de tamanho
                fileInfoLabel.Text = $"📄 {fileInfo.Name} | {(fileInfo.Length / 1024.0):F2} KB";
                fileInfoLabel.IsVisible = true;

                // Validar tipo imagem
                if (result.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                    result.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                    result.FileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase) ||
                    result.FileName.EndsWith(".gif", StringComparison.OrdinalIgnoreCase))
                {
                    previewImage.Source = ImageSource.FromFile(result.FullPath);
                    previewImage.IsVisible = true;
                    noPreviewLabel.IsVisible = false;
                }
                else
                {
                    previewImage.Source = null;
                    previewImage.IsVisible = false;
                    noPreviewLabel.IsVisible = true;
                }
            }
        }

        private async void OnUploadClicked(object sender, EventArgs e)
        {
            statusLabel.IsVisible = true;
            statusLabel.Text = "🔒 A autenticar...";

            bool auth = await _api.AuthenticateAsync();
            if (!auth)
            {
                statusLabel.Text = "❌ Falha na autenticação.";
                return;
            }

            var name = mediaNameEntry.Text;
            var desc = mediaDescEntry.Text;
            var path = filePathEntry.Text;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(path))
            {
                statusLabel.Text = "⚠️ Preencha nome e selecione um ficheiro.";
                return;
            }

            // Verificar tamanho máximo (exemplo: 10MB)
            var fileInfo = new FileInfo(path);
            if (fileInfo.Length > 10 * 1024 * 1024)
            {
                statusLabel.Text = "⚠️ O ficheiro excede o tamanho máximo de 10MB.";
                return;
            }

            statusLabel.Text = "🚀 A enviar ficheiro...";
            uploadProgressBar.IsVisible = true;
            uploadProgressBar.Progress = 0;

            // Simular progresso (porque o método API original não devolve progresso real)
            for (int i = 0; i <= 10; i++)
            {
                uploadProgressBar.Progress = i / 10.0;
                await Task.Delay(100);
            }

            bool success = await _api.UploadMediaAsync(name, path, desc);

            uploadProgressBar.IsVisible = false;
            statusLabel.Text = success ? "✅ Ficheiro enviado com sucesso!" : "❌ Erro ao enviar o ficheiro.";
        }
    }
}
