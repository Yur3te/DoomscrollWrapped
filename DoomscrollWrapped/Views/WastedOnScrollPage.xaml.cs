using DoomscrollWrapped.ViewModels;

namespace DoomscrollWrapped.Views
{
    public partial class WastedOnScrollPage : ContentPage
    {
        private readonly WastedOnScrollViewModel _viewModel;

        public WastedOnScrollPage(WastedOnScrollViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel.RefreshCommand.CanExecute(null))
            {
                _viewModel.RefreshCommand.Execute(null);
            }
        }
    }
}
