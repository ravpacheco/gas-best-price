using System;
using System.Collections.Generic;
using System.Text;

namespace GasBestPrice.Model
{
    public class GasStation
    {
        public string Id { get; set; }
        public string Address { get; set; }
        public DateTimeOffset LastUpdated { get; set; }    
        public float GasolinePrice { get; set; }
        public float AlcoholPrice { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }
}
