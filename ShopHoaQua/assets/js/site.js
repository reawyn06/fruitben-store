/* =========================================================================
   FRUITBEN – JavaScript dùng chung phía khách hàng
   (học theo common.js / HomePage.js / product-detail.js của SPORTIX, viết lại gọn cho MVC)
   - Hiệu ứng hiện dần khi cuộn (IntersectionObserver)
   - Slider ảnh ở trang chủ
   - Nút cuộn dải sản phẩm ngang
   - Toast thông báo tự ẩn
   - Ô số lượng có nút -/+
   - Thư viện ảnh trang chi tiết
   - Thanh trượt giá ở bộ lọc
   - Nút sao chép
   - Ẩn/hiện mật khẩu, đo độ mạnh mật khẩu
   Không phụ thuộc thư viện nào ngoài jQuery/Bootstrap đã có (dùng JS thuần là chính).
   ========================================================================= */
(function () {
  "use strict";

  function $all(sel, root) { return Array.prototype.slice.call((root || document).querySelectorAll(sel)); }

  // ---------- Hiện dần khi cuộn ----------
  function initReveal() {
    var items = $all(".reveal");
    if (!items.length) return;
    if (!("IntersectionObserver" in window)) { items.forEach(function (el) { el.classList.add("show"); }); return; }
    document.body.classList.add("reveal-ready");
    var ob = new IntersectionObserver(function (entries) {
      entries.forEach(function (e) {
        if (!e.isIntersecting) return;
        e.target.classList.add("show");
        ob.unobserve(e.target);
      });
    }, { threshold: 0.12, rootMargin: "0px 0px -40px 0px" });
    items.forEach(function (el) { ob.observe(el); });
  }

  // ---------- Slider ----------
  function initSlider(root) {
    var slides = $all(".fb-slide", root);
    if (slides.length < 2) return;
    var dots = root.querySelector(".fb-dots");
    var i = 0, timer = null;
    slides.forEach(function (_, idx) {
      var b = document.createElement("button");
      b.type = "button";
      b.setAttribute("aria-label", "Ảnh " + (idx + 1));
      b.addEventListener("click", function () { show(idx); restart(); });
      dots && dots.appendChild(b);
    });
    function show(n) {
      i = (n + slides.length) % slides.length;
      slides.forEach(function (s, k) { s.classList.toggle("active", k === i); });
      if (dots) $all("button", dots).forEach(function (d, k) { d.classList.toggle("active", k === i); });
    }
    function restart() {
      clearInterval(timer);
      if (window.matchMedia && window.matchMedia("(prefers-reduced-motion: reduce)").matches) return;
      timer = setInterval(function () { show(i + 1); }, 4500);
    }
    var prev = root.querySelector(".fb-slider-nav.prev"), next = root.querySelector(".fb-slider-nav.next");
    prev && prev.addEventListener("click", function () { show(i - 1); restart(); });
    next && next.addEventListener("click", function () { show(i + 1); restart(); });
    show(0); restart();
  }

  // ---------- Dải sản phẩm cuộn ngang ----------
  function initStrips() {
    $all("[data-strip-target]").forEach(function (btn) {
      btn.addEventListener("click", function () {
        var strip = document.getElementById(btn.getAttribute("data-strip-target"));
        if (!strip) return;
        var step = (strip.firstElementChild ? strip.firstElementChild.getBoundingClientRect().width : 240) + 16;
        strip.scrollBy({ left: step * parseInt(btn.getAttribute("data-dir"), 10), behavior: "smooth" });
      });
    });
  }

  // ---------- Toast ----------
  function hideToast(t) {
    t.classList.add("an");
    setTimeout(function () { t.remove(); }, 260);
  }
  function initToasts() {
    $all(".fb-toast").forEach(function (t) {
      var close = t.querySelector(".fb-toast-close");
      close && close.addEventListener("click", function () { hideToast(t); });
      setTimeout(function () { if (document.body.contains(t)) hideToast(t); }, t.classList.contains("loi") ? 8000 : 5000);
    });
  }

  // ---------- Ô số lượng -/+ ----------
  function initQty() {
    $all("[data-qty]").forEach(function (box) {
      var input = box.querySelector("input");
      var min = parseInt(input.getAttribute("min") || "1", 10);
      var max = parseInt(input.getAttribute("max") || "999", 10);
      $all("button[data-step]", box).forEach(function (b) {
        b.addEventListener("click", function () {
          var v = parseInt(input.value || "0", 10) + parseInt(b.getAttribute("data-step"), 10);
          if (isNaN(v)) v = min;
          v = Math.max(min, Math.min(max, v));
          input.value = v;
          input.dispatchEvent(new Event("change", { bubbles: true }));
          // Trong giỏ hàng: đổi số lượng là gửi form cập nhật luôn
          if (box.hasAttribute("data-auto-submit")) box.closest("form").submit();
        });
      });
      if (box.hasAttribute("data-auto-submit")) {
        input.addEventListener("change", function (e) { if (e.isTrusted) box.closest("form").submit(); });
      }
    });
  }

  // ---------- Thư viện ảnh ----------
  function initGallery() {
    var main = document.getElementById("anhChinh");
    if (!main) return;
    $all(".fb-thumbs button").forEach(function (b) {
      b.addEventListener("click", function () {
        main.src = b.getAttribute("data-src");
        $all(".fb-thumbs button").forEach(function (x) { x.classList.toggle("active", x === b); });
      });
    });
  }

  // ---------- Thanh trượt giá ----------
  function initRange() {
    $all("[data-range-label]").forEach(function (input) {
      var label = document.getElementById(input.getAttribute("data-range-label"));
      function fmt() { label.textContent = Number(input.value).toLocaleString("vi-VN") + "đ"; }
      input.addEventListener("input", fmt);
      fmt();
    });
    // Chọn trong bộ lọc là áp dụng ngay
    $all("[data-auto-apply]").forEach(function (el) {
      el.addEventListener("change", function () { el.form.submit(); });
    });
  }

  // ---------- Sao chép ----------
  function initCopy() {
    $all("[data-copy]").forEach(function (b) {
      b.addEventListener("click", function () {
        var text = b.getAttribute("data-copy");
        var done = function () { var old = b.textContent; b.textContent = "Đã chép"; setTimeout(function () { b.textContent = old; }, 1500); };
        if (navigator.clipboard && navigator.clipboard.writeText) {
          navigator.clipboard.writeText(text).then(done, function () { window.prompt("Sao chép nội dung:", text); });
        } else {
          window.prompt("Sao chép nội dung:", text);
        }
      });
    });
  }

  // ---------- Mật khẩu ----------
  function initPassword() {
    $all("[data-toggle-pass]").forEach(function (b) {
      b.addEventListener("click", function () {
        var input = document.getElementById(b.getAttribute("data-toggle-pass"));
        var hien = input.type === "password";
        input.type = hien ? "text" : "password";
        b.innerHTML = hien ? '<i class="far fa-eye-slash"></i>' : '<i class="far fa-eye"></i>';
        b.setAttribute("aria-label", hien ? "Ẩn mật khẩu" : "Hiện mật khẩu");
      });
    });
    $all("[data-strength]").forEach(function (input) {
      var bar = document.getElementById(input.getAttribute("data-strength"));
      var text = document.getElementById(input.getAttribute("data-strength") + "Text");
      var nhan = ["", "Yếu", "Trung bình", "Khá", "Mạnh"];
      input.addEventListener("input", function () {
        var v = input.value, d = 0;
        if (v.length >= 6) d++;
        if (/[A-Z]/.test(v) && /[a-z]/.test(v)) d++;
        if (/\d/.test(v)) d++;
        if (/[^A-Za-z0-9]/.test(v) || v.length >= 10) d++;
        if (!v) d = 0;
        bar.className = "fb-strength" + (d ? " m" + d : "");
        if (text) text.textContent = d ? "Độ mạnh: " + nhan[d] : "Nên dùng ít nhất 6 ký tự, có chữ hoa, chữ thường và số.";
      });
    });
  }

  // ---------- Đánh dấu menu đang ở trang nào ----------
  function initActiveNav() {
    var path = location.pathname.toLowerCase().replace(/\/+$/, "") || "/";
    $all(".fb-header [data-nav]").forEach(function (li) {
      var keys = li.getAttribute("data-nav").split(",");
      var match = keys.some(function (k) { return k === "/" ? (path === "/" || path === "/home" || path === "/home/index") : path.indexOf(k) === 0; });
      li.classList.toggle("active", match);
    });
  }

  document.addEventListener("DOMContentLoaded", function () {
    initActiveNav();
    initReveal();
    $all(".fb-slider").forEach(initSlider);
    initStrips();
    initToasts();
    initQty();
    initGallery();
    initRange();
    initCopy();
    initPassword();
  });
})();
