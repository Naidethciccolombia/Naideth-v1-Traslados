 
namespace Naideth.Traslados.Dominio.ConvercionMoneda
{  
    public sealed class Precio
    {
        public decimal Value { get; set; }
        public string Moneda { get; set; } 
        public bool Origen { get; set; } 

        private Precio(decimal value, string moneda, bool origen)
        {
            Value = value;
            Moneda = moneda;
            Origen = origen; 
        }
        public static Precio Crear(decimal value, string moneda,bool origen)
        {
            return new Precio(value, moneda, origen);
        } 
    }


}
