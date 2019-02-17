(function () {
    var parts = [tag.albumartist || tag.artist || "No Artist"];
    if (tag.album) {
        parts.push(tag.album);
    }
    return parts.join(" - ");
})()