using Microsoft.Maui.Controls;
using Xibo_CMVV.Views;

namespace Xibo_CMVV;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState activationState)
    {
        return new Window(new NavigationPage(new HomePage()));
    }
}
