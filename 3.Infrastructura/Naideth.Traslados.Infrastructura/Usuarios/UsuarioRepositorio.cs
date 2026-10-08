
using Microsoft.EntityFrameworkCore; 
using Naideth.Central.Dominio.Kernel.Exepciones;
using Naideth.Central.Dominio.Usuarios;
using Naideth.Central.Dominio.Usuarios.Excepciones;
using Naideth.Traslados.Aplicacion.Usuarios.Interfaces; 
using Naideth.Traslados.Infrastructura.AccesoDatos.interfaces;

namespace Naideth.Traslados.Infrastructura.Usuarios
{
    internal class UsuarioRepositorio : IUsuarioRepositorio
    {

        private readonly IApplicationDbContext _context;  

        public UsuarioRepositorio(IApplicationDbContext context)
        {
            _context = context;  
        } 
        public async Task AddRefreshTokenAsync(Guid id, string refresToken, CancellationToken TokenCancelacion)
        {

            // Recupera el usuario existente de la base de datos
            var usuarioExistente = await _context.Usuario.FirstOrDefaultAsync(x => x.IdUsuario == id, TokenCancelacion).ConfigureAwait(false);

            if (usuarioExistente == null)
                throw new NoEncontradoExcepcion();

            usuarioExistente.RefresToken = refresToken;
            usuarioExistente.Expire = DateTime.Now.AddMonths(2);

            _context.Usuario.Entry(usuarioExistente).Property(u => u.RefresToken).IsModified = true;
            _context.Usuario.Entry(usuarioExistente).Property(u => u.Expire).IsModified = true;

            await _context.SaveChangesAsync(TokenCancelacion).ConfigureAwait(false);
             
        }

        public async Task<bool> ValidateRefreshTokenAsync(Guid id, string refresToken, CancellationToken TokenCancelacion)
        {

            // Recupera el usuario existente de la base de datos
            var usuarioExistente = await _context.Usuario.FirstOrDefaultAsync(x => x.IdUsuario == id, TokenCancelacion).ConfigureAwait(false);

            if (usuarioExistente == null)
                throw new NoEncontradoExcepcion();
             
            return usuarioExistente?.RefresToken == refresToken;
        }
        public async Task<Usuario> GetUsuarioAsync(string Usuario, CancellationToken TokenCancelacion) {
            
            var usuario = await _context.Usuario
                               .FirstOrDefaultAsync(r => r.Login == Usuario && r.Estado, TokenCancelacion)
                               .ConfigureAwait(false);

            if (usuario == null)
                throw new NoEncontradoExcepcion(); 

            if(usuario.Bloqueo)
                throw new UsuarioBloqueadoExcepcion();
             

            return usuario;

        }


    }
}
