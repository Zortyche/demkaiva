using DemkaIva.models;
using System;
using System.Collections.Generic;

namespace DemkaIva
{
    // Начальные данные, с которыми приложение запускается "из коробки"
    // на новом компьютере, без необходимости поднимать базу данных.
    public static class SeedData
    {
        public static AppDataStore CreateDefault()
        {
            var store = new AppDataStore();

            store.Gender.Add(new Gender { Code = "М", Name = "Мужской" });
            store.Gender.Add(new Gender { Code = "Ж", Name = "Женский" });

            store.Client.Add(new Client
            {
                ID = 1,
                FirstName = "Анна",
                LastName = "Иванова",
                Patronymic = "Сергеевна",
                RegistrationDate = DateTime.Now,
                Email = "anna.ivanova@example.com",
                Phone = "+7 900 111-22-33",
                GenderCode = "Ж"
            });
            store.Client.Add(new Client
            {
                ID = 2,
                FirstName = "Мария",
                LastName = "Петрова",
                Patronymic = "Алексеевна",
                RegistrationDate = DateTime.Now,
                Email = "maria.petrova@example.com",
                Phone = "+7 900 222-33-44",
                GenderCode = "Ж"
            });
            store.Client.Add(new Client
            {
                ID = 3,
                FirstName = "Дмитрий",
                LastName = "Смирнов",
                Patronymic = "Олегович",
                RegistrationDate = DateTime.Now,
                Email = "dmitry.smirnov@example.com",
                Phone = "+7 900 333-44-55",
                GenderCode = "М"
            });

            foreach (var s in DefaultServices())
                store.Service.Add(s);

            AddDemoAppointments(store);

            return store;
        }

        private static void AddDemoAppointments(AppDataStore store)
        {
            var now = DateTime.Now;

            store.ClientService.Add(new ClientService
            {
                ID = 1,
                ClientID = 1,
                ServiceID = store.Service[0].ID,
                StartTime = now.Date.AddHours(now.Hour + 1)
            });
            store.ClientService.Add(new ClientService
            {
                ID = 2,
                ClientID = 2,
                ServiceID = store.Service[4].ID,
                StartTime = now.Date.AddDays(1).AddHours(11)
            });
            store.ClientService.Add(new ClientService
            {
                ID = 3,
                ClientID = 3,
                ServiceID = store.Service[9].ID,
                StartTime = now.Date.AddDays(1).AddHours(15).AddMinutes(30)
            });
        }

        private static IEnumerable<Service> DefaultServices()
        {
            int id = 1;
            (string Title, decimal Cost, int Minutes, double? Discount, string Image)[] rows =
            {
                ("Гречишный массаж", 2500, 60, null, "Гречишный массаж.jpg"),
                ("Декорирование нарощенных ресниц стразами", 500, 20, null, "Декорирование нарощенных ресниц стразами.jpg"),
                ("Демакияж", 300, 15, null, "Демакияж.jpg"),
                ("Депиляция воском", 800, 30, null, "Депиляция воском.jpg"),
                ("Европейский маникюр", 1200, 60, null, "Европейский маникюр.jpg"),
                ("ИК-сауна", 1000, 30, null, "ИК-сауна.jpg"),
                ("Классический маникюр", 1000, 50, null, "Классический маникюр.png"),
                ("Комбинированный маникюр", 1300, 70, null, "Комбинированный маникюр.jpg"),
                ("Контурная пластика", 8000, 40, 0.10, "Контурная пластика.jpg"),
                ("Мезотерапия", 4500, 45, null, "Мезотерапия.jpg"),
                ("Моделирование ресниц", 2000, 90, null, "Моделирование ресниц.jpg"),
                ("Окрашивание бровей", 400, 20, null, "Окрашивание бровей.jpg"),
                ("Парафинотерапия", 900, 30, null, "Парафинотерапия.jpg"),
                ("Покрытие ногтей гель-лаком", 1500, 60, null, "Покрытие ногтей гель-лаком.jpg"),
                ("Полировка ногтей", 500, 20, null, "Полировка ногтей.jpg"),
                ("Прессотерапия", 1800, 45, null, "Прессотерапия.jpg"),
                ("Прокол ушей пистолетом", 1000, 15, null, "Прокол ушей пистолетом.png"),
                ("Наращивание ресниц", 2500, 120, null, "Ресницы.png"),
                ("Снятие ресниц", 500, 20, null, "Снятие ресниц.jpg"),
                ("Стоун-терапия", 2800, 60, 0.15, "Стоун-терапия.jpg"),
                ("Укрепление ногтей гелем", 1200, 40, null, "Укрепление ногтей гелем.jpg"),
                ("Шугаринг", 1000, 30, null, "Шугаринг.jpg"),
                ("Экспресс SPA", 3000, 60, 0.20, "Экспресс SPA.png"),
                ("Экспресс-педикюр", 1500, 45, null, "Экспресс-педикюр.jpg"),
                ("Пилинг лица", 1500, 30, null, "piling.jpg"),
                ("Солярий", 600, 15, null, "soliarij.jpg"),
                ("Дневной макияж", 1500, 45, null, "makeup.jpg"),
            };

            foreach (var r in rows)
            {
                yield return new Service
                {
                    ID = id++,
                    Title = r.Title,
                    Cost = r.Cost,
                    DurationInSeconds = r.Minutes * 60,
                    Discount = r.Discount,
                    MainImagePath = System.IO.Path.Combine("Images", r.Image)
                };
            }
        }
    }
}
