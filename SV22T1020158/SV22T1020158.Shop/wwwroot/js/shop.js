// Shop JavaScript
$(document).ready(function () {
    // Auto close alerts after 3 seconds
    setTimeout(function () {
        $('.alert').fadeOut('slow');
    }, 3000);

    // Update cart count (nếu có API)
    function updateCartCount() {
        $.get('/Cart/GetCount', function (data) {
            $('#cartCount').text(data.count);
        });
    }

    // Call updateCartCount if needed
    // updateCartCount();
});