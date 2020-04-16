(function () {
    var peak = tag.replaygainalbumpeak;
    if (!peak) {
        return peak;
    }
    var parsed = parseFloat(peak);
    if (isNaN(parsed)) {
        return peak;
    }
    return parsed.toFixed(6);
})()