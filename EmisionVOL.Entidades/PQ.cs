using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace EmisionVOL.Entidades
{
    /// <summary>
    /// Summary description for PQ
    /// </summary>
    public class PQ
    {
        public string Aerolinea { get; set; }
        public string Clase { get; set; }
        public string TotalTarifa { get; set; }
        public string FareCalculation { get; set; }
        public string tipoPasajero { get; set; }
        public string PorComicion { get; set; }

        public PQ() { }

    }
}