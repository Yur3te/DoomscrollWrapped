using DoomscrollWrapped.ViewModels;

namespace DoomscrollWrapped.Views
{
    public partial class LeaderboardPage : ContentPage
    {
        private readonly LeaderboardViewModel _viewModel;

        public LeaderboardPage(LeaderboardViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (_viewModel.LoadCommand.CanExecute(null))
            {
                _viewModel.LoadCommand.Execute(null);
            }
        }
    }
}
