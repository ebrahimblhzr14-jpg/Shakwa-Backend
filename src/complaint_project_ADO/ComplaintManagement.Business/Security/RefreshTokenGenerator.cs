using System.Security.Cryptography;

namespace ComplaintManagement.Business.Security
{
    public class RefreshTokenGenerator
    {
        public string Generate()
        {
            byte[] randomBytes =
                RandomNumberGenerator.GetBytes(64);

            return Convert.ToBase64String(
                randomBytes);
        }
    }
}