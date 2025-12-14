using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace AdsApp
{
    public partial class MyAdsWindow : Window
    {
        private readonly bumEntities1 _context = new bumEntities1();
        private readonly Users _currentUser;

        public MyAdsWindow(Users currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            UserInfoText.Text = $"Пользователь: {_currentUser.Login}";
            LoadUserAds();
        }

        private void LoadUserAds()
        {
            try
            {
                var ads = _context.Ads
                    .Include(a => a.Cities)
                    .Include(a => a.Categories)
                    .Include(a => a.AdStatuses)
                    .Where(a => a.UserId == _currentUser.UserId)
                    .ToList();

                AdsDataGrid.ItemsSource = ads;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при загрузке ваших объявлений:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new EditAdWindow(_currentUser);
            if (editWindow.ShowDialog() == true)
                LoadUserAds();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedAd = AdsDataGrid.SelectedItem as Ads;
            if (selectedAd == null)
            {
                MessageBox.Show(
                    "Выберите объявление для редактирования.",
                    "Информация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var editWindow = new EditAdWindow(_currentUser, selectedAd.AdId);
            if (editWindow.ShowDialog() == true)
                LoadUserAds();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var ad = AdsDataGrid.SelectedItem as Ads;
            if (ad == null)
            {
                MessageBox.Show(
                    "Выберите объявление для удаления.",
                    "Информация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                "Вы действительно хотите удалить объявление?\nОперация необратима.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                _context.Ads.Remove(ad);
                _context.SaveChanges();
                LoadUserAds();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при удалении объявления:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
