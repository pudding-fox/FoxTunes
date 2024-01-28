(function () {
    var gain = tag.replaygaintrackgain;
    if (!gain) {
        return gain;
    }
    var parsed = NumberHelper.parseFloat(gain);
    if (isNaN(parsed)) {
        return gain;
    }
    return (parsed > 0 ? "+" : "") + NumberHelper.toFixed(parsed, 2) + "dB";
})()