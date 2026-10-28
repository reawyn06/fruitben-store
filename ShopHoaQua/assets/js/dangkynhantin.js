function dangKyNhanTin(event) {
    event.preventDefault();

    var emailInput = document.getElementById('email-nhan-tin');
    var thongBao = document.getElementById('thong-bao-nhan-tin');

    if (emailInput && emailInput.value.trim() !== "") {
        emailInput.value = '';
        thongBao.innerHTML = '<i class="fas fa-check-circle mr-1"></i> Đăng ký thành công! Cảm ơn bạn đã quan tâm FruitBen.';
        thongBao.style.display = 'block';
        setTimeout(function () {
            thongBao.style.display = 'none';
        }, 5000);
    }

    return false;
}