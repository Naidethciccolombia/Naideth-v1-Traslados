using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Naideth.Central.Dominio.Aerolineas;
using Naideth.Central.Dominio.Aeropuertos; 
using Naideth.Central.Dominio.Hoteles;
using Naideth.Central.Dominio.Ciudades;
using Naideth.Central.Dominio.CiudadesAeropuertos; 
using Naideth.Central.Dominio.Credenciales;
using Naideth.Central.Dominio.EdadIntegraciones; 
using Naideth.Central.Dominio.IntegracionNombres; 
using Naideth.Central.Dominio.Integradores;
using Naideth.Central.Dominio.MercadoVentas;
using Naideth.Central.Dominio.Monedas;
using Naideth.Central.Dominio.MultiCredenciales;
using Naideth.Central.Dominio.Paises;
using Naideth.Central.Dominio.ParametroCredenciales;
using Naideth.Central.Dominio.Roles;
using Naideth.Central.Dominio.TarifasAdministrativas; 
using Naideth.Central.Dominio.TasasCambio;
using Naideth.Central.Dominio.Tenant;
using Naideth.Central.Dominio.Terceros;
using Naideth.Central.Dominio.TipoProductos;
using Naideth.Central.Dominio.TipoVuelos;
using Naideth.Central.Dominio.Usuarios;
using Naideth.Central.Dominio.UsuariosRoles;
using Naideth.Central.Dominio.UsuariosTenant;

namespace Naideth.Traslados.Infrastructura.AccesoDatos.interfaces
{
    public interface IApplicationDbContext
    {

        public DbSet<IntegracionNombre> IntegracionNombres { get; set; }
        public DbSet<EdadIntegracion> EdadIntegracion { get; set; }
        public DbSet<Credencial> Credenciales { get; set; }
        public DbSet<ParametroCredencial> ParametroCredencial { get; set; }
        public DbSet<MultiCredencial> MultiCredenciales { get; set; }
        public DbSet<Ciudad> Ciudad { get; set; }
        public DbSet<Pais> Paises { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Tercero> Tercero { get; set; }
        public DbSet<Tenant> Tenant { get; set; }
        public DbSet<UsuarioTenant> UsuarioTenant { get; set; }
        public DbSet<TipoProducto> TipoProducto { get; set; }
        public DbSet<UsuarioRoles> UsuarioRoles { get; set; }
        public DbSet<Integrador> Integradores { get; set; } 
        public DbSet<Rol> Rol { get; set; }
        public DbSet<TasaCambio> TasaCambio { get; set; }
        public DbSet<TarifaAdministrativa> TarifaAdministrativa { get; set; }
        public DbSet<TipoVuelo> TipoVuelo { get; set; }
        public DbSet<MercadoVenta> MercadoVenta { get; set; }
        public DbSet<Moneda> Moneda { get; set; }

        public DbSet<Aeropuerto> Aeropuertos { get; set; }
        public DbSet<CiudadAeropuerto> CiudadAeropuerto { get; set; }
        public DbSet<Ciudad> Ciudades { get; set; }
        public DbSet<Aerolinea> Aerolineas { get; set; }
        public DbSet<Hotel> Hoteles { get; set; }


        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
        List<string> GetTenants();
         



    }
}
