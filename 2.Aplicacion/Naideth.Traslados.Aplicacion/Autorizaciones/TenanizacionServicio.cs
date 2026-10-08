using Microsoft.AspNetCore.Http;
using Naideth.Traslados.Aplicacion.Autorizaciones.Interfaces; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Naideth.Traslados.Aplicacion.Autorizaciones
{
    public sealed class TenanizacionServicio : ITenanizacionServicio
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public TenanizacionServicio(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public List<string> GetIdTenants()
        {
            return _httpContextAccessor.HttpContext?.User.Claims
                .Where(c => c.Type == "IdTenants")
                .Select(c => c.Value)
                .ToList() ?? new List<string>();
        } 
        public List<string> GetIdOrganization()
        {
            List<string> IdOrgs = new List<string>();
            var tenant = _httpContextAccessor.HttpContext?.User.Claims
                .Where(c => c.Type == "IdTenants")
                .Select(c => c.Value)
                .ToList() ?? new List<string>();
            
                foreach (var item in tenant)
                {
                    //var organizacion = _tenantRepositorio.GetIdAsync(Guid.Parse(item.ToString()), CancellationToken.None).Result;
                   // IdOrgs.Add(organizacion.IdOrganizacion.ToString());
                }

             
            return IdOrgs;
        }
         
    }

}
