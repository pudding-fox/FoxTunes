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
    var artist = tag.firstalbumartist || tag.firstalbumartistsort || tag.firstartist;
    if (artist) {
        parts.push(artist);
    }
    if (tag.album) {
        parts.push(tag.album);
    }
    if (tag.title) {
        parts.push(tag.title);
    }
    else {
        parts.push(filename(item.FileName));
    }
    if (tag.firstperformer && tag.firstperformer != (tag.firstalbumartist || tag.firstalbumartistsort || tag.firstartist)) {
        parts.push(tag.firstperformer);
    }
    return parts.join(" - ");
})()