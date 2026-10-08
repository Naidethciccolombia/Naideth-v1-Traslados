
using Naideth.Traslados.Dominio.Kernel.Encriptado.DTO;
using Naideth.Traslados.Dominio.Kernel.Encriptados.DTO;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text; 

namespace Naideth.Traslados.Dominio.Encriptados
{
    public sealed class Encriptado
    {  
        public static string Encriptar(string texto)
        {
            if (string.IsNullOrWhiteSpace(texto))
                throw new ArgumentException("Texto a encriptar no puede ser nula o vacía.");

            VariableEncriptadaDTO encriptado= new VariableEncriptadaDTO();

            using (Aes myAes = Aes.Create())
            {
                var (encrypted, key, iv) = AesEncryption.Encrypt(texto, myAes.Key, myAes.IV);


                encriptado = new VariableEncriptadaDTO
                    {
                        EncryptedValue = encrypted,
                        Key = key,
                        IV = iv
                    };
                     
            }

            return JsonConvert.SerializeObject(encriptado);
        }


        public static string DesEncriptar(string TextoEncriptado)
        {
            VariableEncriptadaDTO encriptado = JsonConvert.DeserializeObject<VariableEncriptadaDTO>(TextoEncriptado);
            string  texto = string.Empty;

            using (Aes myAes = Aes.Create())
            {
                texto = AesEncryption.Decrypt(encriptado.EncryptedValue, encriptado.Key, encriptado.IV);
                 
            }

            return texto;
        }
    
    }
}
