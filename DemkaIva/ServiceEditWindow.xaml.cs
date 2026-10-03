using DemkaIva.models;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace DemkaIva
{
    public partial class ServiceEditWindow : Window
    {
        private Service _service;
        private string _mainImageRelativePath;
        private readonly ObservableCollection<PhotoItem> _photos =
            new ObservableCollection<PhotoItem>();

        public ServiceEditWindow(Service service)
        {
            InitializeComponent();
            _service = service ?? new Service();
            LbPhotos.ItemsSource = _photos;

            if (_service.ID != 0)
            {
                TxtHeader.Text = "Редактирование услуги";
                TxtId.Text = _service.ID.ToString();
                TxtTitle.Text = _service.Title;
                TxtCost.Text = _service.Cost.ToString("0");
                TxtDuration.Text = (_service.DurationInSeconds / 60).ToString();
                TxtDiscount.Text = ((_service.Discount ?? 0) * 100).ToString("0.##");
                TxtDesc.Text = _service.Description ?? "";
                _mainImageRelativePath = _service.MainImagePath;
                UpdateMainImagePreview();

                var existingPhotos = AppData.Context.ServicePhoto
                    .Where(p => p.ServiceID == _service.ID)
                    .ToList();
                foreach (var p in existingPhotos)
                    _photos.Add(new PhotoItem { RelativePath = p.PhotoPath });
            }
            else
            {
                TxtHeader.Text = "Добавление услуги";
                TxtId.Text = "(автоматически)";
                TxtDiscount.Text = "0";
            }
        }

        private void TxtNumeric_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"[\d.,]");
        }

        private void UpdateMainImagePreview()
        {
            string full = Service.GetFullPath(_mainImageRelativePath);
            if (!string.IsNullOrEmpty(full) && File.Exists(full))
            {
                try
                {
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.UriSource = new Uri(full);
                    bmp.EndInit();
                    ImgMain.Source = bmp;
                    return;
                }
                catch { }
            }
            ImgMain.Source = null;
        }

        private void BtnSelectMainImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif"
            };

            if (dlg.ShowDialog() == true)
            {
                _mainImageRelativePath = CopyImageToProject(dlg.FileName);
                UpdateMainImagePreview();
            }
        }

        private void BtnDeleteMainImage_Click(object sender, RoutedEventArgs e)
        {
            _mainImageRelativePath = null;
            ImgMain.Source = null;
        }

        private void BtnAddPhotos_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Изображения|*.jpg;*.jpeg;*.png;*.bmp;*.gif",
                Multiselect = true
            };

            if (dlg.ShowDialog() == true)
            {
                foreach (var file in dlg.FileNames)
                {
                    string relative = CopyImageToProject(file);
                    _photos.Add(new PhotoItem { RelativePath = relative });
                }
            }
        }

        private void BtnDeletePhoto_Click(object sender, RoutedEventArgs e)
        {
            if (LbPhotos.SelectedItem is PhotoItem item)
                _photos.Remove(item);
        }

        private string CopyImageToProject(string sourcePath)
        {
            string folder = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Images", "Services");

            Directory.CreateDirectory(folder);

            string ext = Path.GetExtension(sourcePath);
            string fileName = Guid.NewGuid().ToString("N") + ext;
            string dest = Path.Combine(folder, fileName);

            File.Copy(sourcePath, dest, true);

            return Path.Combine("Images", "Services", fileName);
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtTitle.Text))
            {
                MessageBox.Show("Введите наименование услуги.",
                    "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTitle.Focus();
                return;
            }

            if (!decimal.TryParse(TxtCost.Text, out decimal cost) || cost < 0)
            {
                MessageBox.Show("Стоимость должна быть неотрицательным числом.",
                    "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtCost.Focus();
                return;
            }

            if (!int.TryParse(TxtDuration.Text, out int minutes) || minutes <= 0 || minutes > 240)
            {
                MessageBox.Show("Длительность должна быть от 1 до 240 минут (не более 4 часов).",
                    "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtDuration.Focus();
                return;
            }

            double discountPercent = 0;
            if (!string.IsNullOrWhiteSpace(TxtDiscount.Text))
            {
                if (!double.TryParse(TxtDiscount.Text, out discountPercent) ||
                    discountPercent < 0 || discountPercent > 100)
                {
                    MessageBox.Show("Скидка должна быть от 0 до 100%.",
                        "Ошибка ввода", MessageBoxButton.OK, MessageBoxImage.Warning);
                    TxtDiscount.Focus();
                    return;
                }
            }

            string title = TxtTitle.Text.Trim();
            if (AppData.Context.Service.Any(s =>
                s.Title.ToLower() == title.ToLower() && s.ID != _service.ID))
            {
                MessageBox.Show("Услуга с таким наименованием уже существует.",
                    "Дубликат", MessageBoxButton.OK, MessageBoxImage.Warning);
                TxtTitle.Focus();
                return;
            }

            _service.Title = title;
            _service.Cost = cost;
            _service.DurationInSeconds = minutes * 60;
            _service.Discount = discountPercent / 100.0;
            _service.MainImagePath = _mainImageRelativePath;
            _service.Description = string.IsNullOrWhiteSpace(TxtDesc.Text)
                ? null : TxtDesc.Text.Trim();

            if (_service.ID == 0)
                AppData.Context.AddService(_service);

            AppData.Context.SaveChanges();

            // Синхронизация дополнительных фото
            var existing = AppData.Context.ServicePhoto
                .Where(p => p.ServiceID == _service.ID)
                .ToList();

            foreach (var p in existing)
            {
                if (!_photos.Any(x => x.RelativePath == p.PhotoPath))
                    AppData.Context.ServicePhoto.Remove(p);
            }

            foreach (var item in _photos)
            {
                if (!existing.Any(p => p.PhotoPath == item.RelativePath))
                {
                    AppData.Context.AddServicePhoto(new ServicePhoto
                    {
                        ServiceID = _service.ID,
                        PhotoPath = item.RelativePath
                    });
                }
            }

            AppData.Context.SaveChanges();
            DialogResult = true;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }

    public class PhotoItem
    {
        public string RelativePath { get; set; }
        public string FullPath => Service.GetFullPath(RelativePath);
        public string ShortName => Path.GetFileName(RelativePath ?? "");
    }
}