(function () {
  var parts = [];

  if (tag.__ft_variousartists) {
    parts.push("Various Artists");
  }
  else {
    if (tag.artist) {
        parts.push(tag.artist);
    }
  }

  if (tag.year) {
    parts.push(tag.year);
  }

  if (tag.album) {
    parts.push(tag.album);
  }

  if (tag.disccount != 1 && tag.disc) {
    parts.push(tag.disc);
  }

  if (tag.track) {
    parts.push(zeropad(tag.track, 2));
  }

  parts.push(fileName);

  return parts;
})();