namespace Naideth.Traslados.Dominio.Kernel.Interfaces
{
    public interface IExcepcion
    {
        int Codigo { get; }
        string Titulo { get; }
        string Mensaje { get; }
    }
}
