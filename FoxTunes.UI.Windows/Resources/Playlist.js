(function () {
    if (item == null) {
        return version();
    }
    var parts = [];
    if (tag.disccount != 1 && tag.disc) {
        parts.push(tag.disc);
    }
    if (tag.track) {
        parts.push(zeropad(tag.track, 2));
    }
    if (tag.title) {
        parts.push(tag.title);
    }
    else {
        parts.push(filename(item.FileName));
    }
    return parts.join(" - ");
})()