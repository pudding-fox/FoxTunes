(function () {
    var peak = tag.replaygaintrackpeak;
    if (!peak) {
        return peak;
    }
    var parsed = parseFloat(peak);
    if (isNaN(parsed)) {
        return peak;
    }
    return parsed.toFixed(6);
})()