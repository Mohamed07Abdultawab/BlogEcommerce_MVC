// A separate file for cart-related functions that require authentication/API calls

/**
 * دالة لجلب وتحديث عدد العناصر في سلة التسوق.
 * تفترض وجود عنصر HTML بالـ ID: #cartCount
 */
function updateCartCount() {
    // تحقق مما إذا كان العنصر موجودًا قبل إجراء الـ AJAX call
    if ($('#cartCount').length === 0) {
        return;
    }

    // يتم هنا استخدام مسار `GetCartCount` من الـ Layout الأصلي
    // يجب التأكد أن الـ URL صحيح في بيئة MVC الخاصة بك
    // نستخدم 'data-url' لتجنب مشاكل الـ Razor إذا كان الملف JS خارجيًا
    // بما أنك تستخدم Razor في الـ Layout، سنعتمد على الكود كما هو في الـ Layout:
    $.ajax({
        // يجب أن يتم تعيين هذا المسار في سياق Razor (في الـ Layout نفسه)
        url: '@Url.Action("GetCartCount", "Cart")',
        type: 'GET',
        success: function (data) {
            // تحقق من وجود 'count' في البيانات المستلمة
            if (data && typeof data.count === 'number') {
                $('#cartCount').text(data.count);
            } else {
                $('#cartCount').text('0');
            }
        },
        error: function (jqXHR, textStatus, errorThrown) {
            console.error("Error fetching cart count:", textStatus, errorThrown);
            $('#cartCount').text('!'); // علامة خطأ
        }
    });
}

// تشغيل الدالة عند تحميل الصفحة
$(document).ready(function () {
    updateCartCount();
    // يمكن هنا إضافة مؤقت (interval) لتحديث العدد تلقائيًا كل فترة
    // setInterval(updateCartCount, 60000); // تحديث كل 60 ثانية (اختياري)
});

/**
 * دالة مساعدة لتحديث السلة بعد إضافة منتج جديد (كمثال)
 * يمكن استدعاؤها من أي صفحة منتج
 */
function notifyCartUpdate() {
    // يمكن استخدام هذه الدالة لإضافة تأثير بصري سريع لعداد السلة (مثل وميض أو اهتزاز)
    $('#cartCount').addClass('animate__animated animate__tada'); // يتطلب مكتبة animate.css

    // تأخير إزالة الـ Class بعد انتهاء الـ Animation
    setTimeout(function () {
        $('#cartCount').removeClass('animate__animated animate__tada');
    }, 1000);

    updateCartCount();
}