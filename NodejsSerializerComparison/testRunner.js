exports.TestRunner = class TestRunner {
    convertToMicroSeconds(hrtime) {
        return hrtime[0] * 1000000 + hrtime[1] / 1000;
    }

    run(action) {
        var startTime = process.hrtime();
        action();
        var diff = this.convertToMicroSeconds(process.hrtime(startTime));

        return diff;
    }
}