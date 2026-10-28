using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace ShopHoaQua.Data
{
    public class AdoNetHelper
    {
        private static readonly string ConnStr =
            ConfigurationManager.ConnectionStrings["ShopHoaQuaContext"].ConnectionString;
        public DataTable LayDanhSachSanPham()
        {
            DataTable bang = new DataTable();

            using (SqlConnection ket_noi = new SqlConnection(ConnStr))
            {
                string cauTruyVan = "SELECT SanPhamId, TenSanPham, Loai, Gia, SoLuongTon, ConHang " +
                                     "FROM SanPhams ORDER BY SanPhamId";

                using (SqlDataAdapter adapter = new SqlDataAdapter(cauTruyVan, ket_noi))
                {
                    adapter.Fill(bang);
                }
            }

            return bang;
        }
        public DataSet ThongKeDoanhThuTheoSanPham()
        {
            DataSet tapDuLieu = new DataSet();

            string cauTruyVan = @"
                SELECT sp.TenSanPham,
                       ISNULL(SUM(ct.SoLuong), 0)                AS TongSoLuongBan,
                       ISNULL(SUM(ct.SoLuong * ct.DonGia), 0)     AS TongDoanhThu
                FROM SanPhams sp
                LEFT JOIN (ChiTietDonHangs ct
                           INNER JOIN DonHangs dh ON dh.DonHangId = ct.DonHangId AND dh.TrangThai <> N'Đã hủy')
                       ON ct.SanPhamId = sp.SanPhamId
                GROUP BY sp.SanPhamId, sp.TenSanPham
                ORDER BY TongDoanhThu DESC";

            using (SqlConnection ket_noi = new SqlConnection(ConnStr))
            using (SqlDataAdapter adapter = new SqlDataAdapter(cauTruyVan, ket_noi))
            {
                adapter.Fill(tapDuLieu, "DoanhThu");
            }

            return tapDuLieu;
        }

        public int DemTongSoDonHang()
        {
            using (SqlConnection ket_noi = new SqlConnection(ConnStr))
            {
                SqlCommand lenh = new SqlCommand("SELECT COUNT(*) FROM DonHangs", ket_noi);

                ket_noi.Open();               
                object ketQua = lenh.ExecuteScalar();
                return ketQua != null ? (int)ketQua : 0;
            }
        }

        public decimal TinhTongDoanhThu()
        {
            decimal tongDoanhThu = 0;

            using (SqlConnection ket_noi = new SqlConnection(ConnStr))
            {
                SqlCommand lenh = new SqlCommand("SELECT TongTien FROM DonHangs WHERE TrangThai <> N'Đã hủy'", ket_noi);
                ket_noi.Open();

                using (SqlDataReader doc = lenh.ExecuteReader())
                {
                    while (doc.Read())
                    {
                        tongDoanhThu += doc.GetDecimal(0);
                    }
                }
            }

            return tongDoanhThu;
        }
    }
}
