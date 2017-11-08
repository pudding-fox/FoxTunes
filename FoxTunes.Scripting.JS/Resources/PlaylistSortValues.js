(function () {
  var parts = [];

  if (tag.firstalbumartist || tag.firstalbumartistsort || tag.firstartist) {
    parts.push(tag.firstalbumartist || tag.firstalbumartistsort || tag.firstartist);
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