using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Martridge.Models {
    public static class HashHelper {

        public static string ComputeFileSha256Hash(string filePath) {
            using (SHA256 sha256Hash = SHA256.Create()) {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    // get hash bytes from file stream
                    byte[] bytes = sha256Hash.ComputeHash(fileStream);

                    // convert byte array to lowercase string  
                    StringBuilder builder = new StringBuilder();
                    for (int i = 0; i < bytes.Length; i++) {
                        builder.Append(bytes[i].ToString("x2"));
                    }
                    return builder.ToString();
                }
            }
        }
    }
}
