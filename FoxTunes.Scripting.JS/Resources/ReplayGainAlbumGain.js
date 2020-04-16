(function () {
    var gain = tag.replaygainalbumgain;
    if (!gain) {
        return gain;
    }
    var parsed = parseFloat(gain);
    if (isNaN(parsed)) {
        return gain;
    }
    return (parsed > 0 ? "+" : "") + parsed.toFixed(2) + "dB";
})()