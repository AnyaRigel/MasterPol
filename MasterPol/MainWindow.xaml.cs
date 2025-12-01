using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Data.Entity;

namespace MasterPol
{
    public partial class MainWindow : Window
    {
        private MasterPolEntities db = new MasterPolEntities();

        public class PartnerRequestViewModel
        {
            public int PartnerId { get; set; }
            public string PartnerNameWithType { get; set; }
            public string PartnerAddress { get; set; }
            public string PartnerPhone { get; set; }
            public double? PartnerRating { get; set; }
            public decimal TotalCost { get; set; }
            public List<ПродуктыПартнера> Products { get; set; }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadRequests();
        }

        private void LoadRequests()
        {
            try
            {
                // Загруска всех записей из ПродуктыПартнера с связанными данными
                var partnerProducts = db.ПродуктыПартнера
                    .Include(pp => pp.Продукция1)
                    .Include(pp => pp.Партнер)
                    .ToList();

                var grouped = partnerProducts
                    .GroupBy(pp => pp.НаименованиеПартнера)
                    .Select(g =>
                    {
                        var partner = g.First().Партнер;
                        var totalCost = g.Sum(pp =>
                        {
                            var minPrice = pp.Продукция1.МинСтоимостьДляПартнера ?? 0f;
                            return (decimal)(pp.Количество * minPrice);
                        });

                        // Округление и защита от отрицательных значений
                        totalCost = Math.Max(0m, Math.Round(totalCost, 2));

                        return new PartnerRequestViewModel
                        {
                            PartnerId = partner.КодПартнера,
                            PartnerNameWithType = (partner.ТипПартнера == 1 ? "ЗАО" : "ОАО") + " | " + partner.НаименованиеПартнера,
                            PartnerAddress = partner.ЮрАдрес,
                            PartnerPhone = partner.Телефон,
                            PartnerRating = partner.Рейтинг,
                            TotalCost = totalCost,
                            Products = g.ToList()
                        };
                    })
                    .OrderByDescending(r => r.TotalCost)
                    .ToList();

                requestsList.ItemsSource = grouped;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заявок: {ex.Message}");
            }
        }

        private void AddRequestButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new AddEditPartnerWindow();
            if (editWindow.ShowDialog() == true)
            {
                LoadRequests(); 
            }
        }

        private void requestsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (requestsList.SelectedItem is PartnerRequestViewModel request)
            {
                var editWindow = new AddEditPartnerWindow(request.PartnerId);
                if (editWindow.ShowDialog() == true)
                {
                    LoadRequests();
                }
            }
        }

        //метод расчета
        public int CalculateRequiredMaterial(
        int productTypeId,
        int materialTypeId,
        int requiredProductCount,
        int stockProductCount,
        double param1,
        double param2)
    {
        // Валидация входных параметров
        if (param1 <= 0 || param2 <= 0 ||
            requiredProductCount < 0 || stockProductCount < 0)
        {
            return -1;
        }

        // Загрузка данных из БД
        var productType = db.ТипПродукции.FirstOrDefault(p => p.КодТипаПродукции == productTypeId);
        var materialType = db.ТипМатериала.FirstOrDefault(m => m.КодТипаМатриала == materialTypeId);

        if (productType == null || materialType == null)
            return -1;

        double? coefficient = productType.КоэффициентТипаПродукции;
        double? wastePercent = materialType.ПроцентБракаМатериала;

        if (coefficient <= 0 || wastePercent < 0 || wastePercent >= 100)
            return -1;

        // Количество продукции, которое нужно произвести
        int toProduce = Math.Max(0, requiredProductCount - stockProductCount);
        if (toProduce == 0)
            return 0;

        double? materialPerUnit = param1 * param2 * coefficient;

        double? wasteRate = wastePercent / 100.0;
        double? totalMaterialWithWaste = (toProduce * materialPerUnit) / (1 - wasteRate);

        long result = (long)Math.Ceiling((decimal)totalMaterialWithWaste);

        // Проверка на переполнение
        if (result > int.MaxValue || result < 0)
            return -1;

        return (int)result;
    }

        private void Calculations_Click(object sender, RoutedEventArgs e)
        {
            var calcWindow = new MaterialCalculationWindow(this);
            calcWindow.Show();

        }
    }
}
