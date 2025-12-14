using System;
using System.Data.Entity;
using System.Linq;
using System.Windows;

namespace AdsApp
{
    public partial class ReportWindow : Window
    {
        private readonly bumEntities1 _context = new bumEntities1();

        public ReportWindow()
        {
            InitializeComponent();
            // по умолчанию: текущий месяц
            FromDatePicker.SelectedDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            ToDatePicker.SelectedDate = DateTime.Today;
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (FromDatePicker.SelectedDate == null || ToDatePicker.SelectedDate == null)
            {
                MessageBox.Show(
                    "Выберите период отчёта.",
                    "Предупреждение",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            DateTime from = FromDatePicker.SelectedDate.Value.Date;
            DateTime to = ToDatePicker.SelectedDate.Value.Date.AddDays(1).AddTicks(-1);

            try
            {
                var finishedStatus = _context.AdStatuses
                    .First(s => s.Name == "Завершено");

                var list = _context.Ads
                    .Where(a => a.StatusId == finishedStatus.StatusId
                                && a.ClosedAt >= from
                                && a.ClosedAt <= to)
                    .ToList();

                FinishedAdsDataGrid.ItemsSource = list;

                int totalProfit = list
                    .Where(a => a.ProfitAmount.HasValue)
                    .Sum(a => a.ProfitAmount.Value);

                TotalProfitText.Text = totalProfit.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ошибка при формировании отчёта:\n" + ex.Message,
                    "Ошибка",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
