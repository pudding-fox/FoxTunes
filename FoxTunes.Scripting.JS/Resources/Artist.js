(function () {
    if (tag.__ft_variousartists) {
        return "Various Artists";
    }
    return tag.albumartist || tag.artist || "No Artist";
})()