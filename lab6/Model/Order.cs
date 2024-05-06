using System;

namespace lab6.Model
{


    public class Order
    {
        public int IdOrder { get; set; }
        public int ProductId { get; set; }
        public int Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime FullfilledAt { get; set; }
    }
}