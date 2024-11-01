// Xác nhận trước khi đặt hàng
document.querySelector("form").addEventListener("submit", function (event) {
    event.preventDefault();

    // Kiểm tra ngày giao hàng hợp lệ
    const ngayGiaoInput = document.querySelector('input[name="ngayGiao"]');
    const today = new Date();
    today.setHours(0, 0, 0, 0); // Đặt giờ phút giây về 0 để so sánh chính xác ngày
    const selectedDate = new Date(ngayGiaoInput.value);

    // Kiểm tra nếu ngày được chọn không hợp lệ hoặc là quá khứ
    if (isNaN(selectedDate.getTime()) || selectedDate < today) {
        displayErrorMessage("Ngày giao hàng không hợp lệ. Vui lòng chọn ngày hôm nay hoặc trong tương lai.");
        ngayGiaoInput.value = ""; // Xóa giá trị không hợp lệ
        ngayGiaoInput.focus();
        return; // Dừng lại nếu ngày giao không hợp lệ
    } else {
        clearErrorMessage();
    }

    // Nếu tất cả đều hợp lệ, yêu cầu xác nhận từ người dùng và gửi form
    if (confirm("Bạn có chắc chắn muốn đặt hàng không?")) {
        this.submit();
    }
});

// Hiển thị và xóa thông báo lỗi
function displayErrorMessage(message) {
    let errorDiv = document.querySelector(".error-message");
    if (!errorDiv) {
        errorDiv = document.createElement("div");
        errorDiv.classList.add("error-message");
        document.querySelector(".compact-order-container").insertBefore(errorDiv, document.querySelector("form"));
    }
    errorDiv.textContent = message;
}

function clearErrorMessage() {
    const errorDiv = document.querySelector(".error-message");
    if (errorDiv) {
        errorDiv.remove();
    }
}
