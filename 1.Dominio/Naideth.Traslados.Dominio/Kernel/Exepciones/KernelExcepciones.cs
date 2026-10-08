using Microsoft.AspNetCore.Http;
using Naideth.Traslados.Dominio.Kernel.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Naideth.Traslados.Dominio.Kernel.Exepciones
{ 
    #region IdNoValido
    public sealed class IdNoValidoExcepcion : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Error";
        public string Mensaje => "Identificador no encontrada";

    }
    #endregion IdNoValido

    #region CampoInvalido
    public sealed class CampoInvalidoExcepcion : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Alerta";
        public string Mensaje { get; set;}

        public CampoInvalidoExcepcion(string campo)
        {
            Mensaje = $"Campo {campo} no válido";
        }
    }

    #endregion CampoInvalido

    #region RegistroExiste
    public sealed class RegistroExisteExcepcion : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Alerta"; 
        public string Mensaje { get; set; }

        public RegistroExisteExcepcion(string registro)
        {
            Mensaje = $"Ya existe {registro} con este nombre";
        }
    }
    #endregion RegistroExiste

    #region RegistroNoEncontrado
    public sealed class NoEncontradoExcepcion : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Alerta";
        public string Mensaje => "Registro no encontrado";

    }
    #endregion RegistroNoEncontrado

    #region CampoInvalido
    public sealed class ItemNoCreadoExcepcion : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Alerta";
        public string Mensaje { get; set; }

        public ItemNoCreadoExcepcion(string nombreitem)
        {
            Mensaje = $"Item {nombreitem} no válido";
        }
    }

    #endregion CampoInvalido
    #region NoTienePermisoParaAccion 
    public sealed class PermisoNoEncontradoExcepcion : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
    public string Titulo => "Alerta";
    public string Mensaje => "No tienes permiso para esta accion";

}

    #endregion NoTienePermisoParaAccion

    #region ErrorObteniendoRecurso 
    public sealed class ErrorObteniendoRecursoExcepcion : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Alerta";
        public string Mensaje => "Error Obteniendo recurso";

    }

    #endregion NoTienePermisoParaAccion 
    #region ErrorCiudadNoEncontrada
    public sealed class ErrorCiudadNoEncontrada : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Alerta";
        public string Mensaje => "Error Ciudad No Encontrada";

    }
    #endregion ErrorCiudadNoEncontrada
    #region CredencialesGdsNoDisponibles
    public sealed class CredencialesNoDisponibles : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Alerta";
        public string Mensaje => "Credenciales Gds No Disponibles";

    }
    #endregion CredencialesGdsNoDisponibles
    #region TenantsGdsNoDisponibles
    public sealed class TenantsNoDisponibles : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Alerta";
        public string Mensaje => "Tu Cuenta no tiene Tenans Asociado por favor verificar";

    }
    #endregion TenantsGdsNoDisponibles
    #region ListaHotelesNoDisponibles
    public sealed class ListaHotelesNoDisponibles : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Alerta";
        public string Mensaje => "Lista Hoteles No Disponibles";

    }
    #endregion ListaHotelesNoDisponibles
    #region DatosNoEncontrada
    public sealed class DatosNoEncontrada : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Alerta";
        public string Mensaje => "Datos No Encontrados";

    }
    #endregion DatosNoEncontrada

    #region HotelNoEncontradoEnBusqueda
    public sealed class HotelNoEncontradoEnBusqueda : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Alerta";
        public string Mensaje => "Hotel No Encontrado En Busqueda";

    }
    #endregion HotelNoEncontradoEnBusqueda

    #region HotelNoEncontradoEnPreReserva
    public sealed class HotelNoEncontradoEnPreReserva : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Alerta";
        public string Mensaje => "Hotel No Encontrado En PreReserva";

    }
    #endregion HotelNoEncontradoEnPreReserva

    #region CacheBusquedaNoEncontrada
    public sealed class CacheBusquedaNoEncontrada : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Alerta";
        public string Mensaje => "Cache Busqueda No Encontrado";

    }
    #endregion CacheBusquedaNoEncontrada

    #region ExepcionExternaMensaje
    public sealed class ExepcionExternaMensaje : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Alerta";
        public string Mensaje { get; set; } 

        public ExepcionExternaMensaje(string msm)
        {
            Mensaje = $"Alerta {msm}"; 
        }
    }
    public sealed class ExepcionExternaMensajeBook : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Error";
        public string Mensaje { get; set; }

        public ExepcionExternaMensajeBook(string msm)
        {
            if (!msm.Contains(", No Confirma"))
            {
                Mensaje = $"{msm}, No Confirma";
            }
            else
            {
                Mensaje = msm;
            }
        }
    }

    #endregion ExepcionExternaMensaje
    #region ExepcionErrorLeerDataReserva
    public sealed class ExepcionErrorLeerDataReserva : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Error";
        public string Mensaje { get; set; }

        public ExepcionErrorLeerDataReserva(string msm)
        {
            Mensaje = $"Error al leer la data de proveedor, reserva generada en proveedor, Localizador: {msm}";
        }

    }

    #endregion ExepcionErrorLeerDataReserva
    #region ExepcionValidacionPrevioReserva
    public sealed class ExepcionValidacionPrevioReserva : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Error"; 
        public string Mensaje { get; set; }

        public ExepcionValidacionPrevioReserva(string msm)
        {
            Mensaje = $"Error, referencia a reserva ya se encuentra en proveedor, Localizador: {msm}";
        }

    }

    #endregion ExepcionValidacionPrevioReserva
    #region ExepcionGeneracionReserva
    public sealed class ExepcionGeneracionReserva : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Error";
        public string Mensaje { get; set; }

        public ExepcionGeneracionReserva(string msm)
        {
            Mensaje = $"Error, al momento de generar la reserva: {msm}";
        }

    }

    #endregion ExepcionGeneracionReserva
    #region ExepcionGeneracionCancelacion
    public sealed class ExepcionGeneracionCancelacion : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Error";
        public string Mensaje { get; set; }

        public ExepcionGeneracionCancelacion(string msm)
        {
            Mensaje = $"Error, al momento de generar cancelacion de la reserva: {msm}";
        }

    }

    #endregion ExepcionGeneracionCancelacion
    
    #region ExepcionGeneracionDetalleReserva
    public sealed class ExepcionGeneracionDetalleReserva : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Error";
        public string Mensaje { get; set; }

        public ExepcionGeneracionDetalleReserva(string msm)
        {
            Mensaje = $"Error, al momento de generar la consulta de la reserva: {msm}";
        }

    }

    #endregion ExepcionGeneracionDetalleReserva
    #region ExepcionConvitiendoTrm
    public sealed class ExepcionConvitiendoTrm : Exception, IExcepcion
    {
        public int Codigo => StatusCodes.Status404NotFound;
        public string Titulo => "Alerta";
        public string Mensaje => "Error Convitiendo valores por falta de TRM";

    }

    #endregion ExepcionConvitiendoTrm

}
