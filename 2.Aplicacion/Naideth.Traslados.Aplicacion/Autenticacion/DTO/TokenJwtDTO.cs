
 

namespace Naideth.Traslados.Aplicacion.Autenticacion.DTO
{ 
      public sealed record TokenJwtDTO(Guid idUser, List<string> roles, List<string> idTenants);
}
