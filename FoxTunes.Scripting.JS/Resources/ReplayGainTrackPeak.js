(function () {
    var peak = tag.replaygaintrackpeak;
    if (!peak) {
        return peak;
    }
    var parsed = NumberHelper.parseFloat(peak);
    if (isNaN(parsed)) {
        return peak;
    }
    return NumberHelper.toFixed(parsed, 6);
})()