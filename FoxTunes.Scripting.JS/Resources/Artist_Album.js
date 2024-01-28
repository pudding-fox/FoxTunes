(function () {
    var parts = [tag.artist || "No Artist"];
    if (tag.album) {
        parts.push(tag.album);
    }
    return parts.join(" - ");
})()