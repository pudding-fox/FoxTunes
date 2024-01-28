(function () {
    var parts = [];
    if (tag.title) {
        parts.push(tag.title);
    }
    if (tag.performer && tag.performer != (tag.albumartist || tag.artist)) {
        parts.push(tag.performer);
    }
    if (parts.length) {
        return parts.join(" - ");
    }
    else {
        return filename(item.FileName);
    }
})()