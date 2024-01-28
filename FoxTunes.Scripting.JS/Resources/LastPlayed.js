(function () {
    var lastplayed = tag.lastplayed;
    if (!lastplayed) {
        return "Unknown";
    }
    return DateHelper.toLocaleDateString(lastplayed) || "Unknown";
})()