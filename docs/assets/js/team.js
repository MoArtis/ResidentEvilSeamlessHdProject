// Team toggle
$(".team__toggle").on("click", function () {
    $(".team-content").toggleClass("is--visible");
    $(".initial-content").toggleClass("is--hidden");
    // set focus on input
    setTimeout(function () {
        $(".team-content input").focus();
    }, 400);
});

// Close search screen with Esc key
$(document).keyup(function (e) {
    if (e.keyCode === 27) {
        if ($(".initial-content").hasClass("is--hidden")) {
            $(".team-content").toggleClass("is--visible");
            $(".initial-content").toggleClass("is--hidden");
        }
    }
});