using System;
using System.Linq;
using System.Net;
using System.Web.Http;
using ShopHoaQua.Data;
using ShopHoaQua.Filters;
using ShopHoaQua.Models;

namespace ShopHoaQua.Controllers.Api
{
    public class SanPhamApiController : ApiController
    {
        private ShopHoaQuaContext db = new ShopHoaQuaContext();

        // GET: api/SanPhamApi
        public IHttpActionResult GetSanPhams()
        {
            var danhSach = db.SanPhams.Where(sp => sp.ConHang).OrderBy(sp => sp.SanPhamId).ToList();
            return Ok(danhSach);
        }

        // GET: api/SanPhamApi?tatCa=true 
        [ApiQuyenAdmin]
        public IHttpActionResult GetTatCaSanPham(bool tatCa)
        {
            var danhSach = db.SanPhams.OrderBy(sp => sp.SanPhamId).ToList();
            return Ok(danhSach);
        }

        // GET: api/SanPhamApi/id
        [HttpGet]
        [Route("api/SanPhamApi/{id:int}")]
        public IHttpActionResult GetSanPham(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null || !sp.ConHang) return NotFound();
            return Ok(sp);
        }

        // GET: api/SanPhamApi/loai/Trái cây nội địa
        [HttpGet]
        [Route("api/SanPhamApi/loai/{loai}")]
        public IHttpActionResult GetTheoLoai(string loai)
        {
            var danhSach = db.SanPhams.Where(sp => sp.ConHang && sp.Loai == loai).ToList();
            return Ok(danhSach);
        }

        // POST: api/SanPhamApi
        [ApiQuyenAdmin]
        public IHttpActionResult PostSanPham([FromBody] SanPham sanPham)
        {
            if (sanPham == null) return BadRequest("Thiếu dữ liệu sản phẩm.");
            if (!ModelState.IsValid) return BadRequest(ModelState);

            sanPham.SanPhamId = 0;            
            sanPham.NgayTao = DateTime.Now;

            db.SanPhams.Add(sanPham);
            db.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = sanPham.SanPhamId }, sanPham);
        }

        // PUT: api/SanPhamApi/id
        [ApiQuyenAdmin]
        public IHttpActionResult PutSanPham(int id, [FromBody] SanPham sanPham)
        {
            if (sanPham == null) return BadRequest("Thiếu dữ liệu sản phẩm.");
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (id != sanPham.SanPhamId) return BadRequest("Id không khớp");

            var spTrongDb = db.SanPhams.Find(id);
            if (spTrongDb == null) return NotFound();
            spTrongDb.TenSanPham = sanPham.TenSanPham;
            spTrongDb.Loai = sanPham.Loai;
            spTrongDb.Gia = sanPham.Gia;
            spTrongDb.SoLuongTon = sanPham.SoLuongTon;
            spTrongDb.HinhAnh = sanPham.HinhAnh;
            spTrongDb.MoTa = sanPham.MoTa;
            spTrongDb.ConHang = sanPham.ConHang;

            db.SaveChanges();
            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/SanPhamApi/id
        [ApiQuyenAdmin]
        public IHttpActionResult DeleteSanPham(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return NotFound();

            sp.ConHang = false;
            db.SaveChanges();

            return Ok(sp);
        }

        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}
