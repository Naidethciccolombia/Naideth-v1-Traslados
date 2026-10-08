
using MathNet.Numerics.Statistics.Mcmc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Naideth.Central.Dominio.Aerolineas;
using Naideth.Central.Dominio.Aeropuertos;
using Naideth.Central.Dominio.Alimentaciones;
using Naideth.Central.Dominio.Ciudades;
using Naideth.Central.Dominio.CiudadesAeropuertos;
using Naideth.Central.Dominio.CiudadesIntegraciones;
using Naideth.Central.Dominio.Credenciales;
using Naideth.Central.Dominio.EdadIntegraciones;
using Naideth.Central.Dominio.Hoteles;
using Naideth.Central.Dominio.IntegracionNombres;
using Naideth.Central.Dominio.Integradores;
using Naideth.Central.Dominio.MapeoAlimentaciones;
using Naideth.Central.Dominio.MapeoIntegracionesHoteles;
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
using Naideth.Traslados.Aplicacion.Autorizaciones.Interfaces; 
using Naideth.Traslados.Dominio.Kernel.Exepciones;
using Naideth.Traslados.Infrastructura.AccesoDatos.interfaces; 

using System.Linq; 

namespace Naideth.Traslados.Infrastructura.AccesoDatos
{
    public sealed class ApplicationDbContext : DbContext, IApplicationDbContext
    {
        private readonly List<string> _IdTenants;
        //private readonly List<string> _IdOrganizacion;


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenanizacionServicio tenanizacionServicio)
          : base(options)
        { 
            _IdTenants = tenanizacionServicio.GetIdTenants() ?? new List<string>();
        }  
        public DbSet<IntegracionNombre> IntegracionNombres { get; set; }
        public DbSet<EdadIntegracion> EdadIntegracion { get; set; }
        public DbSet<Credencial> Credenciales { get; set; }
        public DbSet<ParametroCredencial> ParametroCredencial { get; set; }
        public DbSet<MultiCredencial> MultiCredenciales { get; set; }  
        public DbSet<Ciudad> Ciudad { get; set; }
        public DbSet<Pais> Paises { get; set; }
        public DbSet<Tercero> Tercero { get; set; }
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Tenant> Tenant { get; set; }
        public DbSet<UsuarioTenant> UsuarioTenant { get; set; }
        public DbSet<TipoProducto> TipoProducto { get; set; }
        public DbSet<UsuarioRoles> UsuarioRoles { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<Integrador> Integradores { get; set; } 
        public DbSet<TasaCambio> TasaCambio { get; set; }
        public DbSet<Moneda> Moneda { get; set; }
        public DbSet<CiudadAeropuerto> CiudadAeropuerto { get; set; }
        public DbSet<Aeropuerto> Aeropuertos { get; set; }
        public DbSet<Ciudad> Ciudades { get; set; }
        public DbSet<Aerolinea> Aerolineas { get; set; }
        public DbSet<TarifaAdministrativa> TarifaAdministrativa { get; set; }
        public DbSet<TipoVuelo> TipoVuelo { get; set; }
        public DbSet<MercadoVenta> MercadoVenta { get; set; }
        public DbSet<Hotel> Hoteles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            #region Usuarios
            // Configurar la clave primaria
            modelBuilder.Entity<Usuario>().HasKey(r => r.IdUsuario);
            modelBuilder.Entity<Usuario>().Property(r => r.Estado).HasConversion<bool>();
       
            #endregion Usuarios 

            #region Tenant
            // Configurar la clave primaria
            modelBuilder.Entity<Tenant>().HasKey(r => r.IdTenant);
            #endregion Tenant
            #region Tercero
            // Configurar la clave primaria
            modelBuilder.Entity<Tercero>().HasKey(r => r.IdTercero);
            modelBuilder.Entity<Tercero>().Property(r => r.Estado).HasConversion<bool>();
            #endregion Tercero

            #region UsuariosTenant
            // Configurar la clave primaria
            modelBuilder.Entity<UsuarioTenant>()
            .HasKey(r => new { r.IdUsuario, r.IdTenant });

            #endregion UsuariosTenant

            #region Ciudades
            // Configurar la clave primaria
            modelBuilder.Entity<Ciudad>().HasKey(r => new { r.IdCiudad, r.IdPais });
            #endregion Ciudades
            #region Pais
            // Configurar la clave primaria
            modelBuilder.Entity<Pais>().HasKey(r => r.IdPais);
            #endregion Pais
            #region TipoProductos
            // Configurar la clave primaria
            modelBuilder.Entity<TipoProducto>().HasKey(r => r.IdTipoProducto);
            modelBuilder.Entity<TipoProducto>().Property(r => r.Estado).HasConversion<bool>();

            #endregion TipoProductos


            #region IntegracionNombres
            // Configurar la clave primaria
            modelBuilder.Entity<IntegracionNombre>().HasKey(r => r.IdIntegracionNombre);
            modelBuilder.Entity<IntegracionNombre>().Property(r => r.Estado).HasConversion<bool>();

            #endregion IntegracionNombres

            #region EdadIntegracion
            //Configurar la clave primaria
            modelBuilder.Entity<EdadIntegracion>().HasKey(r => r.IdEdadIntegracion);
            //modelBuilder.Entity<EdadIntegracion>().Property(r => r.Estado).HasConversion<bool>();
            #endregion EdadIntegracion

            #region Credencial
            //Configurar la clave primaria
            modelBuilder.Entity<Credencial>().HasKey(r => r.IdIntegracionCredencial);
            modelBuilder.Entity<Credencial>().Property(r => r.Estado).HasConversion<bool>();
            #endregion Credencial

            #region ParametroCredencial
            //Configurar la clave primaria
            modelBuilder.Entity<ParametroCredencial>().HasKey(r => r.IdParametroCredencial);
            modelBuilder.Entity<ParametroCredencial>().Property(r => r.Estado).HasConversion<bool>();
            #endregion ParametroCredencial

            #region MultiCredencial
            //Configurar la clave primaria
            modelBuilder.Entity<MultiCredencial>().HasKey(r => r.IdMultiple);
            modelBuilder.Entity<MultiCredencial>().Property(r => r.Estado).HasConversion<bool>();
            #endregion MultiCredencial
              
            #region Roles  
            // Configurar la clave primaria
            modelBuilder.Entity<Rol>().HasKey(r => r.IdRol);
            modelBuilder.Entity<Rol>().Property(r => r.Estado).HasConversion<bool>(); 
            #endregion Roles

            #region UsuariosRoles
            // Configurar la clave primaria
            modelBuilder.Entity<UsuarioRoles>().HasKey(r => new { r.IdRol, r.IdUsuario });
            #endregion UsuariosRoles 
            #region Integrador
            // Configurar la clave primaria
            modelBuilder.Entity<Integrador>().HasKey(r => r.IdIntegrador);
            #endregion Integrador 
             
            #region TasaCambio
            // Configurar la clave primaria
            modelBuilder.Entity<TasaCambio>().HasKey(r => r.IdTasaCambio);
            modelBuilder.Entity<TasaCambio>()
             .Property(t => t.Valor)
             .HasColumnType("decimal(18,6)");
            #endregion TasaCambio 
            #region Moneda
            // Configurar la clave primaria
            modelBuilder.Entity<Moneda>().HasKey(r => r.IdMoneda);
            #endregion Moneda 


            #region CiudadAeropuerto
            // Configurar la clave primaria
            modelBuilder.Entity<CiudadAeropuerto>().HasKey(r => new { r.IdCiudad, r.IdAeropuerto });
            #endregion CiudadAeropuerto

            #region Aeropuertos
            // Configurar la clave primaria
            modelBuilder.Entity<Aeropuerto>().HasKey(r => r.IdAeropuerto);
            modelBuilder.Entity<Aeropuerto>().Property(r => r.Estado).HasConversion<bool>();
            #endregion Aeropuertos

            #region Ciudades
            // Configurar la clave primaria
            modelBuilder.Entity<Ciudad>().ToTable("Ciudades");
            modelBuilder.Entity<Ciudad>().HasKey(r => new { r.IdCiudad });
            #endregion Ciudades

            #region Aerolineas
            // Configurar la clave primaria
            modelBuilder.Entity<Aerolinea>().HasKey(r => r.IdAerolineas);
            modelBuilder.Entity<Aerolinea>().Property(r => r.Estado).HasConversion<bool>();
            #endregion Aerolineas

            #region TarifaAdministrativa
            // Configurar la clave primaria
            modelBuilder.Entity<TarifaAdministrativa>().HasKey(r => r.IdTarifa);
            modelBuilder.Entity<TarifaAdministrativa>().Property(r => r.Estado).HasConversion<bool>();
            #endregion TarifaAdministrativa

            #region TipoVuelo
            // Configurar la clave primaria
            modelBuilder.Entity<TipoVuelo>().HasKey(r => r.IdTipoVuelo);
            modelBuilder.Entity<TipoVuelo>().Property(r => r.Estado).HasConversion<bool>();
            #endregion TipoVuelo

            #region MercadoVenta
            // Configurar la clave primaria
            modelBuilder.Entity<MercadoVenta>().HasKey(r => r.IdMercadoVenta);
            modelBuilder.Entity<MercadoVenta>().Property(r => r.Estado).HasConversion<bool>();
            #endregion MercadoVenta

            #region Hoteles
            // Configurar la clave primaria
            modelBuilder.Entity<Hotel>().HasKey(r => r.IdHotel);
            #endregion Hoteles


        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            try
            {
                return await base.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (ObjectDisposedException ex)
            {
                var stackTrace = new System.Diagnostics.StackTrace(true);
                System.Diagnostics.Debug.WriteLine($"[CONTEXTO YA DESECHADO] ContextId: {ContextId} | Thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {stackTrace}");
                throw new ObjectDisposedException(
                    $"ApplicationDbContext (ContextId: {ContextId})",
                    $"El contexto fue desechado prematuramente. Thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}. Ver Debug Output para detalles.");
            }
        }

        public override void Dispose()
        {
            var stackTrace = new System.Diagnostics.StackTrace(true);
            System.Diagnostics.Debug.WriteLine($"[CONTEXTO DISPOSE] ContextId: {ContextId} | Thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            System.Diagnostics.Debug.WriteLine($"Dispose StackTrace: {stackTrace}");
            base.Dispose();
        }

        public override async ValueTask DisposeAsync()
        {
            var stackTrace = new System.Diagnostics.StackTrace(true);
            System.Diagnostics.Debug.WriteLine($"[CONTEXTO DISPOSE ASYNC] ContextId: {ContextId} | Thread: {System.Threading.Thread.CurrentThread.ManagedThreadId}");
            System.Diagnostics.Debug.WriteLine($"DisposeAsync StackTrace: {stackTrace}");
            await base.DisposeAsync();
        }
         
        public List<string> GetTenants()
        {
            var lst = this._IdTenants.ToList();

            if (lst.Count == 0)
                throw new TenantsNoDisponibles();

            return lst;
        }
    }
}
