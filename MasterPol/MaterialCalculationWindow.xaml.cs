using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace MasterPol
{
    public partial class MaterialCalculationWindow : Window
    {
        private MasterPolEntities db = new MasterPolEntities();
        private MainWindow _mainWindow;

        public MaterialCalculationWindow(MainWindow mainWindow)
        {
            InitializeComponent();
            _mainWindow = mainWindow;

            List<ТипПродукции> product = db.ТипПродукции.ToList();
            ProductTypeComboBox.ItemsSource = product;
            List<ТипМатериала> material = db.ТипМатериала.ToList();
            MaterialTypeComboBox.ItemsSource = material;
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ProductTypeComboBox.SelectedItem == null || MaterialTypeComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Выберите тип продукции и материала");
                    return;
                }

                int productTypeId = Convert.ToInt32(ProductTypeComboBox.SelectedValue);
                int materialTypeId = Convert.ToInt32(MaterialTypeComboBox.SelectedValue);

                if (!int.TryParse(RequiredCountBox.Text, out int required) || required < 0)
                {
                    MessageBox.Show("Требуемое количество должно быть целым неотрицательным числом");
                    return;
                }

                if (!int.TryParse(StockCountBox.Text, out int stock) || stock < 0)
                {
                    MessageBox.Show("Количество на складе должно быть целым неотрицательным числом");
                    return;
                }

                if (!double.TryParse(Param1Box.Text.Replace('.', ','), out double p1) || p1 <= 0)
                {
                    MessageBox.Show("Параметр 1 должен быть положительным числом");
                    return;
                }

                if (!double.TryParse(Param2Box.Text.Replace('.', ','), out double p2) || p2 <= 0)
                {
                    MessageBox.Show("Параметр 2 должен быть положительным числом");
                    return;
                }

                int result = _mainWindow.CalculateRequiredMaterial(
                    productTypeId, materialTypeId, required, stock, p1, p2);

                if (result == -1)
                {
                    ResultText.Text = "Ошибка: неверные данные или типы не найдены";
                    ResultText.Foreground = Brushes.Red;
                }
                else
                {
                    ResultText.Text = $"Результат: {result} единиц материала";
                    ResultText.Foreground = Brushes.Green;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}