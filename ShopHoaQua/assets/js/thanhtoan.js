/* =========================================================================
   FRUITBEN – trang Thanh toán (học theo checkout.js của SPORTIX)
   - Đổ danh sách tỉnh/thành và phường/xã từ window.DIA_CHI_VN (assets/js/diachi-vn.js)
   - Đổi tỉnh hoặc áp mã giảm giá -> gọi /DonHang/TinhTien để XEM TRƯỚC phí ship, giảm giá, tổng tiền
     (server vẫn tính lại toàn bộ khi bấm Đặt hàng, số tiền trên trang chỉ để hiển thị)
   - Tô sáng phương thức thanh toán đang chọn, chặn bấm Đặt hàng 2 lần
   ========================================================================= */
(function ($) {
  "use strict";

  var cfg = window.FB_THANH_TOAN || {};
  var data = window.DIA_CHI_VN || [];
  var $tinh = $("#TinhThanh"), $phuong = $("#PhuongXa"), $ma = $("#MaGiamGia");

  function tien(v) { return Number(v).toLocaleString("vi-VN") + "đ"; }

  // ---------- Tỉnh / phường ----------
  function napTinh() {
    var chon = $tinh.val();
    $tinh.empty().append($("<option>", { value: "", text: "Chọn tỉnh / thành phố" }));
    $.each(data, function (_, t) {
      $tinh.append($("<option>", { value: t.t, text: t.t, selected: t.t === chon }));
    });
    napPhuong($phuong.val());
  }

  function napPhuong(chon) {
    var t = null;
    $.each(data, function (_, x) { if (x.t === $tinh.val()) t = x; });
    $phuong.empty().append($("<option>", { value: "", text: t ? "Chọn phường / xã" : "Chọn tỉnh trước" }));
    $phuong.prop("disabled", !t);
    if (!t) return;
    $.each(t.p, function (_, p) {
      $phuong.append($("<option>", { value: p, text: p, selected: p === chon }));
    });
  }

  // ---------- Xem trước tiền ----------
  var dangGoi = null;
  function capNhatTien(baoMa) {
    if (!cfg.urlTinhTien) return;
    if (dangGoi) dangGoi.abort();
    dangGoi = $.getJSON(cfg.urlTinhTien, { tinh: $tinh.val() || "", ma: $ma.val() || "" })
      .done(function (r) {
        if (!r || !r.ok) return;
        $("#tamTinh").text(tien(r.tamTinh));
        $("#phiShip").text(r.chuaChonTinh && r.phiVanChuyen === 0 && r.conThieuMienPhiShip > 0
          ? "Chọn tỉnh để tính" : (r.phiVanChuyen === 0 ? "Miễn phí" : tien(r.phiVanChuyen)));
        $("#giamGia").text(r.giamGia > 0 ? "-" + tien(r.giamGia) : "0đ");
        $("#tongTien").text(tien(r.tongTien));
        $("#conThieu").text(tien(r.conThieuMienPhiShip));
        $("#hopFreeship").prop("hidden", r.conThieuMienPhiShip <= 0);

        if (baoMa) {
          // Ẩn lỗi server cũ của ô mã (nếu có) rồi hiện kết quả mới
          $ma.removeClass("input-validation-error");
          $("[data-valmsg-for='MaGiamGia']").empty();
          $("#thongBaoMa")
            .text(r.coNhapMa ? r.thongBaoMa : "")
            .toggleClass("ok", r.maHopLe).toggleClass("loi", !r.maHopLe);
        }
      });
  }

  // ---------- Sự kiện ----------
  $tinh.on("change", function () { napPhuong(""); capNhatTien(false); });
  $("#btnApMa").on("click", function () {
    $ma.val(($ma.val() || "").trim().toUpperCase());
    capNhatTien(true);
  });
  $ma.on("keydown", function (e) {
    if (e.key === "Enter") { e.preventDefault(); $("#btnApMa").click(); }
  });
  $(".fb-coupon-list").on("click", "button[data-ma]", function () {
    $ma.val($(this).data("ma"));
    capNhatTien(true);
  });

  $("input[name='PhuongThucThanhToan']").on("change", function () {
    $(".fb-pay-option").removeClass("checked");
    $(this).closest(".fb-pay-option").addClass("checked");
  });

  $("#formThanhToan").on("submit", function () {
    var $f = $(this);
    if ($f.valid && !$f.valid()) return; // jQuery Validate báo lỗi thì không khoá nút
    $("#btnDatHang").prop("disabled", true).html('<i class="fas fa-spinner fa-spin"></i> Đang đặt hàng...');
  });

  $(function () {
    if (data.length) napTinh();
  });
})(jQuery);
