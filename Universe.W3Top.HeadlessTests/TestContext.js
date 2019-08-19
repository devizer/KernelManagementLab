const file = require('fs');

export default class TestContext {
    
    errors = [];
    
    constructor(Protocol, PageSpec)
    {
        this.Protocol = Protocol;
        this.PageSpec = PageSpec;
    }
    
    addError = (error) => {
        this.errors.push(error);
    };

    setWindowSize = async (width, height) => {
        var window = await this.Protocol.Browser.getWindowForTarget();
        console.log("New Size: [%d * %d], Prev Window: %o", width, height, window);
        window.bounds.width=width;
        window.bounds.height=height;
        await this.Protocol.Browser.setWindowBounds(window);
    };

    getExpression = async (expression) => {
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

    getElementById = async (idName) => {
        return await this.getExpression(`document.getElementById('${idName}').innerText`);
    };

    getVersion = async () => {
        const userAgent = await this.getExpression("navigator.userAgent");
        const raw = userAgent.match(/Chrom(e|ium)\/([0-9]+)\./);
        return raw ? parseInt(raw[2], 10) : false;
    };

    waitForTrigger = async (timeout, triggerKey) => {
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

    delay = ms => new Promise(res => setTimeout(res, ms));

    saveScreenshot = async (fileName) => {
        const ss = await this.Protocol.Page.captureScreenshot({format: 'png', fromSurface: true});
        file.writeFile(fileName, ss.data, 'base64', function (err) {
            if (err) console.error(`${fileName} screenshot error: ${err}`);
        });
    }
} 

