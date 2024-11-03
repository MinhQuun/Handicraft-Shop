document.addEventListener("DOMContentLoaded", function () {
    const userMenuToggle = document.querySelector('.user-menu-toggle');
    const userDropdown = document.querySelector('.user-dropdown');
    let dropdownTimeout;

    function showDropdown() {
        clearTimeout(dropdownTimeout); // Ngăn không cho menu ẩn ngay khi di chuột vào
        userDropdown.style.display = "block";
    }

    function hideDropdown() {
        dropdownTimeout = setTimeout(() => {
            userDropdown.style.display = "none";
        }, 300); // Thời gian chờ trước khi ẩn menu
    }

    userMenuToggle.addEventListener("mouseenter", showDropdown);
    userMenuToggle.addEventListener("mouseleave", hideDropdown);
    userDropdown.addEventListener("mouseenter", showDropdown);
    userDropdown.addEventListener("mouseleave", hideDropdown);
});
