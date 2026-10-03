using DemkaIva.models;
using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DemkaIva
{
    public partial class SignUpWindow : Window
    {
        private readonly Service _service;
        private readonly ClientService _existing;

        public SignUpWindow(Service service, ClientService existing = null)
        {
            InitializeComponent();
            _service = service;
            _existing = existing;

            TxtServiceTitle.Text = _service.Title;
            TxtDuration.Text = _service.DurationInMinutes + " мин";

            var clients = AppData.Context.Client.ToList()
                .Select(c => new
                {
                    c.ID,
                    FullName = ($"{c.LastName} {c.FirstName} {c.Patronymic}").Trim()
                })
                .OrderBy(c => c.FullName)
                .ToList();

            CmbClients.ItemsSource = clients;
            CmbClients.DisplayMemberPath = "FullName";
            CmbClients.SelectedValuePath = "ID";

            if (_existing != null)
            {
                TxtHeader.Text = "Редактирование записи";
                CmbClients.SelectedValue = _existing.ClientID;
                DpDate.SelectedDate = _existing.StartTime.Date;
                TxtTime.Text = _existing.StartTime.ToString("HH:mm");
            }
            else
            {
                DpDate.SelectedDate = DateTime.Today;
                TxtTime.Text = DateTime.Now.AddHours(1).ToString("HH:mm");
            }

            UpdateEndTime();
        }

        private void TxtTime_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\d:]");
        }

        private void TxtTime_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateEndTime();
        }

        private void DpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateEndTime();
        }

        private void UpdateEndTime()
        {
            if (TxtEndTime == null || TxtTime == null) return;

            if (TryParseTime(TxtTime.Text, out var time))
            {
                var start = (DpDate.SelectedDate ?? DateTime.Today).Date.Add(time);
                var end = start.AddSeconds(_service.DurationInSeconds);
                TxtEndTime.Text = end.ToString("HH:mm");
            }
            else
            {
                TxtEndTime.Text = "—";
            }
        }

        private static bool TryParseTime(string text, out TimeSpan time)
        {
            time = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(text)) return false;
            if (!TimeSpan.TryParseExact(text.Trim(), @"hh\:mm",
                CultureInfo.InvariantCulture, out time)) return false;
            if (time.Hours < 0 || time.Hours > 23) return false;
            if (time.Minutes < 0 || time.Minutes > 59) return false;
            return true;
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CmbClients.SelectedValue == null)
            {
                MessageBox.Show("Выберите клиента из списка.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DpDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Выберите дату оказания услуги.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!TryParseTime(TxtTime.Text, out var time))
            {
                MessageBox.Show("Введите корректное время в формате ЧЧ:ММ (например, 14:30).",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTime.Focus();
                return;
            }

            DateTime start = DpDate.SelectedDate.Value.Date.Add(time);

            if (_existing == null && start < DateTime.Now)
            {
                MessageBox.Show("Нельзя оформить запись на прошедшее время.",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (_existing == null)
            {
                var record = new ClientService
                {
                    ClientID = (int)CmbClients.SelectedValue,
                    ServiceID = _service.ID,
                    StartTime = start
                };
                AppData.Context.AddClientService(record);
            }
            else
            {
                _existing.ClientID = (int)CmbClients.SelectedValue;
                _existing.ServiceID = _service.ID;
                _existing.StartTime = start;
            }

            AppData.Context.SaveChanges();

            MessageBox.Show("Запись успешно сохранена.",
                "Успех", MessageBoxButton.OK, MessageBoxImage.Information);

            DialogResult = true;
        }
    }
}