namespace DemkaIva.models
{
    public partial class Service
    {
        public int ID { get; set; }
        public string Title { get; set; }
        public decimal Cost { get; set; }
        public int DurationInSeconds { get; set; }
        public string Description { get; set; }
        public double? Discount { get; set; }
        public string MainImagePath { get; set; }
    }
}
