using DemkaIva.models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DemkaIva
{
    // Самодостаточное хранилище данных на основе XML-файла.
    // Не требует установки или настройки какой-либо базы данных:
    // файл создаётся автоматически рядом с приложением при первом запуске.
    [XmlRoot("DemkaIvaData")]
    public class AppDataStore
    {
        private static readonly string FilePath = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory, "Data", "appdata.xml");

        public List<Gender> Gender { get; set; } = new List<Gender>();
        public List<Client> Client { get; set; } = new List<Client>();
        public List<Service> Service { get; set; } = new List<Service>();
        public List<ServicePhoto> ServicePhoto { get; set; } = new List<ServicePhoto>();
        public List<ClientService> ClientService { get; set; } = new List<ClientService>();

        public void AddService(Service service)
        {
            service.ID = NextId(Service, s => s.ID);
            Service.Add(service);
        }

        public void AddServicePhoto(ServicePhoto photo)
        {
            photo.ID = NextId(ServicePhoto, p => p.ID);
            ServicePhoto.Add(photo);
        }

        public void AddClientService(ClientService clientService)
        {
            clientService.ID = NextId(ClientService, cs => cs.ID);
            ClientService.Add(clientService);
        }

        private static int NextId<T>(List<T> items, Func<T, int> idSelector) =>
            items.Count == 0 ? 1 : items.Max(idSelector) + 1;

        public void SaveChanges()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            var serializer = new XmlSerializer(typeof(AppDataStore));
            using (var writer = new StreamWriter(FilePath))
                serializer.Serialize(writer, this);
        }

        public static AppDataStore Load()
        {
            if (File.Exists(FilePath))
            {
                try
                {
                    var serializer = new XmlSerializer(typeof(AppDataStore));
                    using (var reader = new StreamReader(FilePath))
                        return (AppDataStore)serializer.Deserialize(reader);
                }
                catch
                {
                    // Файл повреждён — создаём данные заново.
                }
            }

            var store = SeedData.CreateDefault();
            store.SaveChanges();
            return store;
        }
    }

    public static class AppData
    {
        public static AppDataStore Context { get; } = AppDataStore.Load();
    }
}
