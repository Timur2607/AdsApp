using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace AdsApp
{
    public partial class MainWindow : Window
    {
        private readonly bumEntities1 _context = new bumEntities1();
        private Users _currentUser;

        public MainWindow()
        {
            InitializeComponent();
            LoadFilters();
            LoadAds();
        }

        private void LoadFilters()
        {
            try
            {
                CityComboBox.ItemsSource = _context.Cities.ToList();
                CategoryComboBox.ItemsSource = _context.Categories.ToList();
                TypeComboBox.ItemsSource = _context.AdTypes.ToList();
                StatusComboBox.ItemsSource = _context.AdStatuses.ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при загрузке фильтров:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void LoadAds()
        {
            try
            {
                var query = _context.Ads
                    .Include(a => a.Cities)
                    .Include(a => a.Categories)
                    .Include(a => a.AdTypes)
                    .Include(a => a.AdStatuses)
                    .AsQueryable();

                string keyword = SearchTextBox.Text?.Trim();
                if (!string.IsNullOrEmpty(keyword))
                {
                    query = query.Where(a =>
                        a.Title.Contains(keyword) ||
                        a.Description.Contains(keyword));
                }

                if (CityComboBox.SelectedValue is int cityId)
                    query = query.Where(a => a.CityId == cityId);

                if (CategoryComboBox.SelectedValue is int catId)
                    query = query.Where(a => a.CategoryId == catId);

                if (TypeComboBox.SelectedValue is int typeId)
                    query = query.Where(a => a.AdTypeId == typeId);

                if (StatusComboBox.SelectedValue is int statusId)
                    query = query.Where(a => a.StatusId == statusId);
                else
                {
                    // по умолчанию только "Активно"
                    int activeStatusId = _context.AdStatuses
                        .First(s => s.Name == "Активно").StatusId;
                    query = query.Where(a => a.StatusId == activeStatusId);
                }

                var list = query.ToList();

                // заглушка для картинок
                foreach (var ad in list)
                {
                    if (string.IsNullOrEmpty(ad.ImagePath))
                        ad.ImagePath = "/Resources/no-image.png";
                }

                AdsListView.ItemsSource = list;

            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при загрузке объявлений:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ApplyFilters_Click(object sender, RoutedEventArgs e)
        {
            LoadAds();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            if (loginWindow.ShowDialog() == true)
            {
                _currentUser = loginWindow.CurrentUser;
                MessageBox.Show(
                    $"Вы вошли как: {_currentUser.Login}",
                    "Успешная авторизация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                LoginButton.Visibility = Visibility.Collapsed;
                MyAdsButton.Visibility = Visibility.Visible;
            }
        }

        private void MyAdsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_currentUser == null)
            {
                MessageBox.Show(
                    "Сначала выполните вход.",
                    "Информация",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var myAdsWindow = new MyAdsWindow(_currentUser);
            myAdsWindow.ShowDialog();
            // после закрытия можно обновить общую ленту
            LoadAds();
        }

        private void ReportButton_Click(object sender, RoutedEventArgs e)
        {
            var reportWindow = new ReportWindow();
            reportWindow.ShowDialog();
        }
    }
}
