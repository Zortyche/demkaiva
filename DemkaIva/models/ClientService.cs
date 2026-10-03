using System;

namespace DemkaIva.models
{
    public class ClientService
    {
        public int ID { get; set; }
        public int ClientID { get; set; }
        public int ServiceID { get; set; }
        public DateTime StartTime { get; set; }
        public string Comment { get; set; }
    }
}
