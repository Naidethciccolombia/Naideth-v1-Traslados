namespace Naideth.Traslados.Aplicacion.Autorizaciones.DTO
{ 
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class PermisosAttribute : Attribute
    {
        public string Permisos { get; }

        public PermisosAttribute(string permisos)
        {
            Permisos = permisos;
        }
    }
}
