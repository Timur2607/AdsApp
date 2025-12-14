using Microsoft.Win32;
using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace AdsApp
{
    public partial class EditAdWindow : Window
    {
        private readonly bumEntities1 _context = new bumEntities1();
        private readonly Users _currentUser;
        private Ads _ad;
        private int _oldStatusId;
        private string _imagePath;

        // Конструктор для добавления
        public EditAdWindow(Users currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            LoadLookups();

            _ad = new Ads();
            _oldStatusId = 0; // раньше статуса не было
            SetDefaultImage();
        }

        // Конструктор для редактирования
        public EditAdWindow(Users currentUser, int adId)
        {
            InitializeComponent();
            _currentUser = currentUser;

            LoadLookups();

            _ad = _context.Ads
                .Include(a => a.Cities)
                .Include(a => a.Categories)
                .Include(a => a.AdStatuses)
                .First(a => a.AdId == adId);

            _oldStatusId = _ad.StatusId;
            BindAdToForm();
        }

        private void LoadLookups()
        {
            CityComboBox.ItemsSource = _context.Cities.ToList();
            CategoryComboBox.ItemsSource = _context.Categories.ToList();
            TypeComboBox.ItemsSource = _context.AdTypes.ToList();
            StatusComboBox.ItemsSource = _context.AdStatuses.ToList();
        }

        private void BindAdToForm()
        {
            TitleTextBox.Text = _ad.Title;
            DescriptionTextBox.Text = _ad.Description;
            PriceTextBox.Text = _ad.Price?.ToString();
            CityComboBox.SelectedValue = _ad.CityId;
            CategoryComboBox.SelectedValue = _ad.CategoryId;
            TypeComboBox.SelectedValue = _ad.AdTypeId;
            StatusComboBox.SelectedValue = _ad.StatusId;

            if (!string.IsNullOrEmpty(_ad.ImagePath) && File.Exists(_ad.ImagePath))
            {
                AdImage.Source = new BitmapImage(new Uri(_ad.ImagePath));
                _imagePath = _ad.ImagePath;
            }
            else
            {
                SetDefaultImage();
            }
        }

        private void SetDefaultImage()
        {
            AdImage.Source = new BitmapImage(
                new Uri("pack://application:,,,/Resources/no-image.png"));
        }   

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp"
            };

            if (dlg.ShowDialog() == true)
            {
                _imagePath = dlg.FileName;
                AdImage.Source = new BitmapImage(new Uri(_imagePath));
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(TitleTextBox.Text))
                {
                    MessageBox.Show(
                        "Заполните заголовок объявления.",
                        "Предупреждение",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                _ad.Title = TitleTextBox.Text.Trim();
                _ad.Description = DescriptionTextBox.Text.Trim();

                _ad.Price = decimal.TryParse(PriceTextBox.Text, out var price)
                    ? price
                    : (decimal?)null;

                _ad.CityId = (int)CityComboBox.SelectedValue;
                _ad.CategoryId = (int)CategoryComboBox.SelectedValue;
                _ad.AdTypeId = (int)TypeComboBox.SelectedValue;
                _ad.StatusId = (int)StatusComboBox.SelectedValue;
                _ad.UserId = _currentUser.UserId;

                if (!string.IsNullOrEmpty(_imagePath))
                    _ad.ImagePath = _imagePath;

                // Логика прибыли при переводе в "Завершено"
                var statusFinished = _context.AdStatuses
                    .First(s => s.Name == "Завершено");
                bool wasFinished = _oldStatusId == statusFinished.StatusId;
                bool nowFinished = _ad.StatusId == statusFinished.StatusId;

                if (!wasFinished && nowFinished)
                {
                    var profitWindow = new ProfitInputWindow();
                    if (profitWindow.ShowDialog() == true)
                    {
                        if (!int.TryParse(profitWindow.ProfitText, out int profit) || profit < 0)
                        {
                            MessageBox.Show(
                                "Сумма прибыли должна быть целым неотрицательным числом.",
                                "Ошибка ввода",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
                            return;
                        }

                        _ad.ProfitAmount = profit;
                        _ad.ClosedAt = DateTime.Now;
                    }
                    else
                    {
                        MessageBox.Show(
                            "Завершение объявления отменено.",
                            "Информация",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        return;
                    }
                }

                if (_ad.AdId == 0)
                {
                    _ad.CreatedAt = DateTime.Now;
                    _context.Ads.Add(_ad);
                }

                _context.SaveChanges();
                DialogResult = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при сохранении объявления:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
