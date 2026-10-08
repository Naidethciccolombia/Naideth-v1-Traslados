 
using Microsoft.EntityFrameworkCore;
using Microsoft.WindowsAzure.Storage.Table;
using Naideth.Central.Dominio.AdicionalComisiones;
using Naideth.Central.Dominio.CategoriaMarkups;
using Naideth.Central.Dominio.Comisiones;
using Naideth.Central.Dominio.Credenciales;
using Naideth.Central.Dominio.MarkUpsTarifas;
using Naideth.Central.Dominio.Oficinas;
using Naideth.Central.Dominio.ParametroCredenciales;
using Naideth.Central.Dominio.Parametros;
using Naideth.Central.Dominio.ServidoresEmail;
using Naideth.Central.Dominio.TarifasAdministrativas;
using Naideth.Central.Dominio.Tenant;
using Naideth.Central.Dominio.Terceros;
using Naideth.Central.Dominio.UsuariosTenant;
using System.IO.Compression;
using System.Text;

namespace Naideth.Central.Infrastructura.AccesoDatos
{
    public static class ExtensionesData
    {

        public static IQueryable<TarifaAdministrativa> FiltroTenant(this DbSet<TarifaAdministrativa> dbSet, List<string> _IdTenants)
        {
            var tenantGuids = _IdTenants.Where(x => x != string.Empty).Select(Guid.Parse).Cast<Guid?>().ToList();
            return (tenantGuids.Count > 0 ? dbSet.Where(e => tenantGuids.Contains(e.IdTenant)) : dbSet);

        }
         
        public static string Decompress(this string compressedString)
        {
            byte[] decompressedBytes;

            var compressedStream = new MemoryStream(Convert.FromBase64String(compressedString));

            using (var decompressorStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
            {
                using (var decompressedStream = new MemoryStream())
                {
                    decompressorStream.CopyTo(decompressedStream);

                    decompressedBytes = decompressedStream.ToArray();
                }
            }

            return Encoding.UTF8.GetString(decompressedBytes);
        }
        public static string Compress(this string uncompressedString)
        {
            byte[] compressedBytes;

            using (var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(uncompressedString)))
            {
                using (var compressedStream = new MemoryStream())
                {
                    // setting the leaveOpen parameter to true to ensure that compressedStream will not be closed when compressorStream is disposed
                    // this allows compressorStream to close and flush its buffers to compressedStream and guarantees that compressedStream.ToArray() can be called afterward
                    // although MSDN documentation states that ToArray() can be called on a closed MemoryStream, I don't want to rely on that very odd behavior should it ever change
                    using (var compressorStream = new DeflateStream(compressedStream, CompressionLevel.Fastest, true))
                    {
                        uncompressedStream.CopyTo(compressorStream);
                    }

                    // call compressedStream.ToArray() after the enclosing DeflateStream has closed and flushed its buffer to compressedStream
                    compressedBytes = compressedStream.ToArray();
                }
            }

            return Convert.ToBase64String(compressedBytes);
        }


        public static string WrapWithCData(this string input)
        {
            if (string.IsNullOrEmpty(input)) return "";
            return $"<![CDATA[{input.Replace("<![CDATA[", "").Replace("]]>", "")}]]>";
        }

        public static IQueryable<ParametroCredencial> FiltroTenant(this DbSet<ParametroCredencial> dbSet, List<string> _IdTenants)
        {
            var tenantGuids = _IdTenants.Where(x => x != string.Empty).Select(Guid.Parse).Cast<Guid?>().ToList();
            return (tenantGuids.Count > 0 ? dbSet.Where(e => tenantGuids.Contains(e.IdTenant)) : dbSet);
        }
        public static IQueryable<Credencial> FiltroTenant(this DbSet<Credencial> dbSet, List<string> _IdTenants)
        {
            var tenantGuids = _IdTenants.Where(x => x != string.Empty).Select(Guid.Parse).Cast<Guid?>().ToList();
            return (tenantGuids.Count > 0 ? dbSet.Where(e => tenantGuids.Contains(e.IdTenant)) : dbSet);
        }



    }
}
