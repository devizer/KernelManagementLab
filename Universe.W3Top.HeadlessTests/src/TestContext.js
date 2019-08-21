const file = require('fs');

const myJSON = arg => {
    const s1 = JSON.stringify(arg,null, ' ');
    let s2 = s1.replace(/\n/g, " ");
    // .replace(/  /g, " ")
    while(s2.indexOf("  ") >= 0) {s2 = s2.replace("  ", " "); }
    return s2;
}

class TestContext {
    
    constructor(Protocol, PageSpec, errors)
    {
        this.Protocol = Protocol;
        this.PageSpec = PageSpec;
        this.errors = errors;
    }
    
    addError(error) {
        console.error(`ERROR: ${error}`);
        this.errors.push(error);
    };

    async setWindowSize (width, height) {
        var window = await this.Protocol.Browser.getWindowForTarget();
        console.log("New Window Size: [%d * %d], Prev Size: %s", width, height, myJSON(window));
        let newWidth = window.bounds.width, newHeight = window.bounds.height;
        if (width > 0) newWidth = width;
        if (height > 0) newHeight = height;
        window.bounds.width = newWidth;
        window.bounds.height = newHeight;
        await this.Protocol.Browser.setWindowBounds(window);
    };

    async getExpression (expression) {
        const expressionValue = await this.Protocol.Runtime.evaluate({expression: expression});
        // console.log(`Expression [${expression}] value is [%o]`, expressionValue);

        if (expressionValue.subtype === "error")
            return undefined;

        if (expressionValue.result.className === "Date")
            return Date.parse(expressionValue.result.description);

        if (expressionValue.result.type === "number")
            return expressionValue.result.value;

        return expressionValue.result.value;
    };

    async getElementById (idName) {
        return await this.getExpression(`document.getElementById('${idName}').innerText`);
    };

    async getVersion () {
        const userAgent = await this.getExpression("navigator.userAgent");
        const raw = userAgent.match(/Chrom(e|ium)\/([0-9]+)\./);
        return raw ? parseInt(raw[2], 10) : false;
    };

    async waitForTrigger (timeout, triggerKey) {
        const start = new Date();
        let ms = 1;
        while(true)
        {
            let value = await this.getExpression(`window.ApplicationLevelTriggers.${triggerKey}.first`);
            // console.log(`Wait for ${triggerKey}: ${value} (${new Date() - start})`);
            if (value > 0) {
                value = Math.round(value*10)/10;
                if (value < 0.00999) value = 0.01;
                console.debug(`Trigger [${triggerKey}] successfully confirmed in ${new Date() - start} milliseconds. Raised at ${value.toFixed(1)} milliseconds`);
                return ;
            }

            await this.delay(ms);
            ms = ms >= 128 ? ms : ms * 2;
            let elapsed = new Date() - start;
            if (elapsed > timeout) break;
        }

        console.warn(`Warning! trigger [${triggerKey}] was not raised in ${new Date() - start} milliseconds`);
        return false;
    };

    delay(ms) {
        return new Promise(res => setTimeout(res, ms));
    } 

    async saveScreenshot (fileName) {
        const ss = await this.Protocol.Page.captureScreenshot({format: 'png', fromSurface: true});
        file.writeFile(fileName, ss.data, 'base64', function (err) {
            if (err) console.error(`${fileName} screenshot error: ${err}`);
        });
    }
} 

module.exports = TestContext;
