using System;

namespace RestApi.Domain
{
    public class Order
    {
        public DateTime OrderDate { get; set; }
        public string Status { get; set; }
    }
}