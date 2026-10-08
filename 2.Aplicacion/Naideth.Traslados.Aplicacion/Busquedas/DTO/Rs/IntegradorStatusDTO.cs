namespace Naideth.Traslados.Aplicacion.Vuelo.DTO.Rs
{
    public class IntegradorStatusDTO
    {
        public string Nombre { get; set; }
        public string Estado { get; set; } // Pendiente, EnProceso, Completada, Error
        public double? TiempoRespuesta { get; set; } // En segundos
        public int CantidadDisponibilidades { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public string? MensajeError { get; set; }

        public IntegradorStatusDTO(string nombre, DateTime fechaInicio)
        {
            Nombre = nombre;
            Estado = "EnProceso";
            FechaInicio = fechaInicio;
            CantidadDisponibilidades = 0;
        }

        public void MarcarCompletada(int cantidadDisponibilidades)
        {
            Estado = "Completada";
            FechaFin = DateTime.UtcNow;
            TiempoRespuesta = Math.Round((FechaFin.Value - FechaInicio).TotalSeconds, 2);
            CantidadDisponibilidades = cantidadDisponibilidades;
        }

        public void MarcarError(string mensajeError, int cantidadDisponibilidades)
        {
            Estado = "Error";
            FechaFin = DateTime.UtcNow;
            TiempoRespuesta = Math.Round((FechaFin.Value - FechaInicio).TotalSeconds, 2);
            MensajeError = mensajeError;
            CantidadDisponibilidades = cantidadDisponibilidades;
        }
    }
}
