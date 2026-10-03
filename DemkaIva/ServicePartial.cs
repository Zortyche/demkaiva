using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace DemkaIva.models
{
    public partial class Service
    {
        public decimal DiscountedCost =>
            (Discount.HasValue && Discount.Value > 0)
                ? Cost * (decimal)(1 - Discount.Value)
                : Cost;

        public int DurationInMinutes => DurationInSeconds / 60;

        public Visibility HasDiscountVisibility =>
            (Discount.HasValue && Discount.Value > 0)
                ? Visibility.Visible
                : Visibility.Collapsed;

        public string DiscountText =>
            (Discount.HasValue && Discount.Value > 0)
                ? $"скидка {Discount.Value * 100:0}%"
                : "";

        public SolidColorBrush CardBackground
        {
            get
            {
                if (Discount.HasValue && Discount.Value > 0)
                    return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E8F5E9"));
                return Brushes.White;
            }
        }

        public string ImagePath
        {
            get
            {
                string fallback = GetFullPath("Images/beauty_logo.png");
                if (string.IsNullOrWhiteSpace(MainImagePath))
                    return fallback;

                string full = GetFullPath(MainImagePath);
                if (File.Exists(full))
                    return full;

                
                string byName = Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    "Images", "Services",
                    Path.GetFileName(MainImagePath));
                if (File.Exists(byName))
                    return byName;

                return fallback;
            }
        }

        public static string GetFullPath(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
                return null;

            if (Path.IsPathRooted(relativePath))
                return relativePath;

            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        }
    }
}