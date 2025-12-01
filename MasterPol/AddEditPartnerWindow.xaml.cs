using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace MasterPol
{
    public partial class AddEditPartnerWindow : Window
    {
        private MasterPolEntities db = new MasterPolEntities();
        private int? _partnerId;

        // Свойства для привязки
        public int PartnerType { get; set; }
        public string PartnerName { get; set; } = "";
        public string Director { get; set; } = "";
        public string Address { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Email { get; set; } = "";
        public double? Rating { get; set; } = 0;

        public AddEditPartnerWindow()
        {
            InitializeComponent();
            InitializeForm();
        }

        public AddEditPartnerWindow(int partnerId) : this()
        {
            _partnerId = partnerId;
            LoadPartnerData(partnerId);
        }

        private void InitializeForm()
        {
            DataContext = this;
            TypeComboBox.ItemsSource = new Dictionary<int, string>
            {
                { 1, "ООО" },
                { 2, "ПАО" }
            };
            PartnerType = 1;
        }

        private void LoadPartnerData(int partnerId)
        {
            var partner = db.Партнер.FirstOrDefault(p => p.КодПартнера == partnerId);
            if (partner == null)
            {
                MessageBox.Show("Партнер не найден");
                return;
            }

            PartnerType = partner.ТипПартнера;
            PartnerName = partner.НаименованиеПартнера;
            Director = partner.Директор;
            Address = partner.ЮрАдрес;
            Phone = partner.Телефон;
            Email = partner.Почта;
            Rating = partner.Рейтинг;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(PartnerName))
            {
                MessageBox.Show("Укажите наименование компании");
                return;
            }

            if (!ValidateRating())
                return;

            try
            {
                if (_partnerId.HasValue)
                {
                    // Редактирование
                    var partner = db.Партнер.First(p => p.КодПартнера == _partnerId.Value);
                    UpdatePartnerEntity(partner);
                }
                else
                {
                    // Добавление
                    var newPartner = new Партнер();
                    UpdatePartnerEntity(newPartner);
                    db.Партнер.Add(newPartner);
                }

                db.SaveChanges();
                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void UpdatePartnerEntity(Партнер partner)
        {
            partner.ТипПартнера = PartnerType;
            partner.НаименованиеПартнера = PartnerName;
            partner.Директор = Director;
            partner.ЮрАдрес = Address;
            partner.Телефон = Phone;
            partner.Почта = Email;
            partner.Рейтинг = Rating ?? 0;
        }

        private bool ValidateRating()
        {
            if (string.IsNullOrWhiteSpace(RatingTextBox.Text))
            {
                Rating = 0;
                return true;
            }

            if (double.TryParse(RatingTextBox.Text.Replace('.', ','), out double r))
            {
                if (r >= 0 && r <= 10)
                {
                    Rating = r;
                    return true;
                }
                else
                {
                    MessageBox.Show("Рейтинг должен быть от 0 до 10");
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Некорректный формат рейтинга");
                return false;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}