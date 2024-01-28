(function () {
    if (tag.__ft_variousartists) {
        return strings.general_variousartists;
    }
    return tag.artist || strings.general_noartist;
})()