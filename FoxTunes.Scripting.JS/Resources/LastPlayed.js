(function () {
    var lastplayed = tag.lastplayed;
    if (!lastplayed) {
        return "Unknown";
    }
    return toLocaleDateString(lastplayed) || "Unknown";
})()