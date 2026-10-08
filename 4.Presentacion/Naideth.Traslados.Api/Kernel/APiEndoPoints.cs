namespace Naideth.Traslados.Api.Kernel
{
    internal static class ApiEndpoints
    {
        private const string ApiBase = "api";

        internal static class V1
        {
            private const string VersionBase = $"{ApiBase}/v1";
           
            internal static class Traslados
            {
                private const string Valor = $"{VersionBase}/Traslado";

                internal const string BusquedaAsync = $"{Valor}/BusquedaAsync";
                internal const string TrasladoIncluidaAsync = $"{Valor}/TrasladoIncluidoAsync";
                internal const string CancelacionAsync = $"{Valor}/CancelacionAsync";
                //internal const string EmisionAsync = $"{Valor}/EmisionAsync"; 
                internal const string BorrarCacheRedisAsync = $"{Valor}/BorrarCacheRedisAsync";
                internal const string PreReservaAsync = $"{Valor}/PreReservaAsync";
                internal const string PreReservaSeleccionadaAsync = $"{Valor}/PreReservaSeleccionadaAsync";
                internal const string ReservaAsync = $"{Valor}/ReservaAsync";

                internal const string PermisoBusquedaAsync = $"{Valor}/BusquedaApi";
                internal const string PermisoPreReservaHotelAsync = $"{Valor}/PreReservaHotelApi";

            }
            internal static class Autenticacion
            {
                private const string Valor = $"{VersionBase}/autenticacion";

                internal const string AutenticacionAsync = $"{Valor}/AutenticacionAsync"; 
                internal const string RefreshTokenAsync = $"{Valor}/RefreshTokenAsync"; 

            }
        }
     }

}
