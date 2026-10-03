using DemkaIva;
using DemkaIva.models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DemkaIva
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool _isAdmin;
        private ServiceEditWindow _editWindow;

        public event PropertyChangedEventHandler PropertyChanged;

        public Visibility AdminVisibility =>
            _isAdmin ? Visibility.Visible : Visibility.Collapsed;

        private void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            if (TxtSearch == null || CmbFilter == null || CmbSort == null ||
                LvServices == null || TxtCount == null)
                return;

            var list = AppData.Context.Service.ToList();
            int total = list.Count;

            // Поиск по названию и описанию
            string query = (TxtSearch.Text ?? "").Trim().ToLower();
            if (!string.IsNullOrEmpty(query))
            {
                list = list.Where(s =>
                    (s.Title != null && s.Title.ToLower().Contains(query)) ||
                    (s.Description != null && s.Description.ToLower().Contains(query)))
                    .ToList();
            }

            // Фильтр по скидке [min, max)
            switch (CmbFilter.SelectedIndex)
            {
                case 1:
                    list = list.Where(s => { double d = s.Discount ?? 0; return d >= 0 && d < 0.05; }).ToList();
                    break;
                case 2:
                    list = list.Where(s => { double d = s.Discount ?? 0; return d >= 0.05 && d < 0.15; }).ToList();
                    break;
                case 3:
                    list = list.Where(s => { double d = s.Discount ?? 0; return d >= 0.15 && d < 0.30; }).ToList();
                    break;
                case 4:
                    list = list.Where(s => { double d = s.Discount ?? 0; return d >= 0.30 && d < 0.70; }).ToList();
                    break;
                case 5:
                    list = list.Where(s => { double d = s.Discount ?? 0; return d >= 0.70 && d <= 1.00; }).ToList();
                    break;
            }

            // Сортировка по цене (по итоговой стоимости)
            if (CmbSort.SelectedIndex == 1)
                list = list.OrderBy(s => s.DiscountedCost).ToList();
            else if (CmbSort.SelectedIndex == 2)
                list = list.OrderByDescending(s => s.DiscountedCost).ToList();

            LvServices.ItemsSource = list;
            TxtCount.Text = $"Отображено: {list.Count} из {total}";
        }

        private void FilterChanged(object sender, EventArgs e) => LoadData();

        private void PbAdmin_PasswordChanged(object sender, RoutedEventArgs e)
        {
            _isAdmin = PbAdmin.Password == "0000";
            OnPropertyChanged(nameof(AdminVisibility));
            LoadData();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAdmin) return;

            if (_editWindow != null && _editWindow.IsVisible)
            {
                _editWindow.Activate();
                return;
            }

            _editWindow = new ServiceEditWindow(null);
            _editWindow.Closed += (s, args) => _editWindow = null;

            if (_editWindow.ShowDialog() == true)
                LoadData();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAdmin) return;

            if (_editWindow != null && _editWindow.IsVisible)
            {
                _editWindow.Activate();
                return;
            }

            var service = (sender as Button)?.DataContext as Service;
            if (service == null) return;

            _editWindow = new ServiceEditWindow(service);
            _editWindow.Closed += (s, args) => _editWindow = null;

            if (_editWindow.ShowDialog() == true)
                LoadData();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAdmin) return;

            var service = (sender as Button)?.DataContext as Service;
            if (service == null) return;

            // Проверяем наличие записей
            if (AppData.Context.ClientService.Any(cs => cs.ServiceID == service.ID))
            {
                MessageBox.Show(
                    "Нельзя удалить услугу, так как на неё уже есть записи клиентов.",
                    "Удаление запрещено",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Удалить услугу «{service.Title}»?\nДополнительные фотографии также будут удалены.",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            // Удаляем связанные фото
            var photos = AppData.Context.ServicePhoto
                .Where(p => p.ServiceID == service.ID)
                .ToList();
            foreach (var p in photos)
                AppData.Context.ServicePhoto.Remove(p);

            AppData.Context.Service.Remove(service);
            AppData.Context.SaveChanges();
            LoadData();
        }

        private void BtnSignUp_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAdmin) return;

            var service = (sender as Button)?.DataContext as Service;
            if (service == null) return;

            var win = new SignUpWindow(service);
            win.ShowDialog();
        }

        private void BtnNearest_Click(object sender, RoutedEventArgs e)
        {
            if (!_isAdmin) return;
            new AppointmentsWindow().ShowDialog();
        }
    }
}