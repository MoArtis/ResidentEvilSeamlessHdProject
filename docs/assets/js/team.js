// Team toggle
$(".team__toggle").on("click", function () {
    $(".team-content").toggleClass("is--visible");
    $(".initial-content").toggleClass("is--hidden");
    // set focus on input
    setTimeout(function () {
        $(".team-content input").focus();
    }, 400);
});
