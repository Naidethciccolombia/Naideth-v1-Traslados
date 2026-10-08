 
namespace Naideth.Traslados.Aplicacion.Comunes.DTO.Rq
{
    public sealed record IntegracionDTO(Guid idIntegracion, Guid idIntegrador, string CodigoIntegrador, string codigo, string nombre, string EndPoint, string Usuario, string Clave, List<CredencialDTO> credencial, List<EdadIntegracionDTO> edades, Guid IdTenant);
            
}

     