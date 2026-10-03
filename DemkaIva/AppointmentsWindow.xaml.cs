using DemkaIva.models;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace DemkaIva
{
    public partial class AppointmentsWindow : Window
    {
        private readonly DispatcherTimer _timer;

        public AppointmentsWindow()
        {
            InitializeComponent();
            LoadAppointments();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(30)
            };
            _timer.Tick += (s, e) => LoadAppointments();
            _timer.Start();

            Closed += (s, e) => _timer.Stop();
        }

        private void LoadAppointments()
        {
            var today = DateTime.Today;
            var dayAfterTomorrow = today.AddDays(2);

            var list = AppData.Context.ClientService
                .Where(cs => cs.StartTime >= today && cs.StartTime < dayAfterTomorrow)
                .OrderBy(cs => cs.StartTime)
                .ToList()
                .Select(cs =>
                {
                    var service = AppData.Context.Service.FirstOrDefault(s => s.ID == cs.ServiceID);
                    var client = AppData.Context.Client.FirstOrDefault(c => c.ID == cs.ClientID);

                    var remaining = cs.StartTime - DateTime.Now;
                    string text;
                    Brush brush = Brushes.Black;

                    if (remaining.TotalMinutes < 0)
                    {
                        text = "уже началось";
                        brush = Brushes.Red;
                    }
                    else if (remaining.TotalHours < 1)
                    {
                        text = $"{remaining.Minutes} мин";
                        brush = Brushes.Red;
                    }
                    else
                    {
                        int h = (int)remaining.TotalHours;
                        int m = remaining.Minutes;
                        text = $"{h} ч {m} мин";
                    }

                    return new AppointmentView
                    {
                        ID = cs.ID,
                        ServiceTitle = service?.Title ?? "—",
                        ClientName = ($"{client?.LastName} {client?.FirstName} {client?.Patronymic}").Trim(),
                        Email = client?.Email,
                        Phone = client?.Phone,
                        StartTime = cs.StartTime,
                        RemainingText = text,
                        RemainingBrush = brush,
                        Entity = cs,
                        Service = service
                    };
                })
                .ToList();

            LvAppointments.ItemsSource = list;
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (LvAppointments.SelectedItem is AppointmentView item)
            {
                var win = new SignUpWindow(item.Service, item.Entity);
                if (win.ShowDialog() == true)
                    LoadAppointments();
            }
            else
            {
                MessageBox.Show("Выберите запись для редактирования.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (LvAppointments.SelectedItem is AppointmentView item)
            {
                var result = MessageBox.Show(
                    $"Удалить запись клиента «{item.ClientName}» на услугу «{item.ServiceTitle}»?",
                    "Подтверждение удаления",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    AppData.Context.ClientService.Remove(item.Entity);
                    AppData.Context.SaveChanges();
                    LoadAppointments();
                }
            }
            else
            {
                MessageBox.Show("Выберите запись для удаления.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e) => LoadAppointments();

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();
    }

    public class AppointmentView
    {
        public int ID { get; set; }
        public string ServiceTitle { get; set; }
        public string ClientName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public DateTime StartTime { get; set; }
        public string RemainingText { get; set; }
        public Brush RemainingBrush { get; set; }
        public ClientService Entity { get; set; }
        public Service Service { get; set; }
    }
}