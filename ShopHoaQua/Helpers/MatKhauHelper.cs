using System;
using System.Security.Cryptography;

namespace ShopHoaQua.Helpers
{
    public static class MatKhauHelper
    {
        private const string TIEN_TO = "PBKDF2";
        private const int SO_VONG_LAP = 10000;
        private const int DO_DAI_SALT = 16;  
        private const int DO_DAI_HASH = 32;  

        public static string BamMatKhau(string matKhau)
        {
            byte[] salt = new byte[DO_DAI_SALT];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            byte[] hash = TinhHash(matKhau, salt, SO_VONG_LAP);
            return string.Join("$", TIEN_TO, SO_VONG_LAP, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public static bool KiemTra(string matKhauNhap, string matKhauDaLuu)
        {
            if (string.IsNullOrEmpty(matKhauNhap) || string.IsNullOrEmpty(matKhauDaLuu)) return false;

            if (!DaDuocBam(matKhauDaLuu))
            {
                return SoSanhAnToan(System.Text.Encoding.UTF8.GetBytes(matKhauNhap),
                                    System.Text.Encoding.UTF8.GetBytes(matKhauDaLuu));
            }

            var phan = matKhauDaLuu.Split('$');
            if (phan.Length != 4) return false;

            int soVongLap;
            if (!int.TryParse(phan[1], out soVongLap)) return false;

            try
            {
                byte[] salt = Convert.FromBase64String(phan[2]);
                byte[] hashDaLuu = Convert.FromBase64String(phan[3]);
                byte[] hashNhap = TinhHash(matKhauNhap, salt, soVongLap);
                return SoSanhAnToan(hashNhap, hashDaLuu);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool DaDuocBam(string matKhauDaLuu)
        {
            return matKhauDaLuu != null && matKhauDaLuu.StartsWith(TIEN_TO + "$", StringComparison.Ordinal);
        }

        private static byte[] TinhHash(string matKhau, byte[] salt, int soVongLap)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(matKhau, salt, soVongLap))
            {
                return pbkdf2.GetBytes(DO_DAI_HASH);
            }
        }

        private static bool SoSanhAnToan(byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            int khac = 0;
            for (int i = 0; i < a.Length; i++) khac |= a[i] ^ b[i];
            return khac == 0;
        }
    }
}
