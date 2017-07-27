function timestamp(value) {
    var s = parseInt((value / 1000) % 60);
    var m = parseInt((value / (1000 * 60)) % 60);
    var h = parseInt((value / (1000 * 60 * 60)) % 24);

    var parts = [];

    if (h > 0) {
        if (h < 10) {
            h = "0" + h;
        }
        parts.push(h);
    }

    if (m < 10) {
        m = "0" + m;
    }
    parts.push(m);
    if (s < 10) {
        s = "0" + s;
    }
    parts.push(s);

    return parts.join(":");
}

function zeropad(value, width) {
    value = value + "";
    return value.length >= width ? value : new Array(width - value.length + 1).join("0") + value;
}