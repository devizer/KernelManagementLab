const file = require('fs');
var colors = require('colors');

const Utils = require("./Utils");

class TestContext {
    
    constructor(Protocol, PageSpec, errors)
    {
        this.Protocol = Protocol;
        this.PageSpec = PageSpec;
        this.errors = errors;
    }
    
    addError(error) {
        console.error(`ERROR: ${error}`.red);
        this.errors.push(error);
    };

    async setWindowSize (width, height) {
        var window = await this.Protocol.Browser.getWindowForTarget();
        console.log(`New Window Size: [${width} * ${height}], Prev Size: ${Utils.asJSON(window)}`.grey);
        let newWidth = window.bounds.width, newHeight = window.bounds.height;
        if (width > 0) newWidth = Math.floor(width+0.9999);
        if (height > 0) newHeight = Math.floor(height+0.9999);
        window.bounds.width = newWidth;
        window.bounds.height = newHeight;
        await this.Protocol.Browser.setWindowBounds(window);
    };

    async getExpression (expression) {
        const expressionValue = await this.Protocol.Runtime.evaluate({expression: expression});
        // console.log(`Expression [${expression}] value is [%o]`, expressionValue);

        if (expressionValue.result.type === "undefined")
            return undefined;
        
        if (expressionValue.result.subtype === "error") {
            const err = new Error(expressionValue.result.description);
            // console.debug(`Warning! Expression [${expression}] returned error: [${err.message}]`.grey);
            return undefined;
        }

        if (expressionValue.result.className === "Date")
            return Date.parse(expressionValue.result.description);

        if (expressionValue.result.type === "number" || expressionValue.result.type === 'string')
            return expressionValue.result.value;
        
        throw new Error(`Unknown CDP expression type [${expressionValue.result.type}] for expression [${expression}]`);

    };

    async getElementById (idName) {
        return await this.getExpression(`document.getElementById('${idName}').innerText`);
    };

    async getVersion () {
        const userAgent = await this.getExpression("navigator.userAgent");
        const raw = userAgent.match(/Chrom(e|ium)\/([0-9]+)\./);
        return raw ? parseInt(raw[2], 10) : false;
    };

    async waitForElement(timeout, idElementName) {
        return this.waitForLambda(timeout, `Element ${idElementName}`, async () => {
            return undefined != await this.getElementById(idElementName);
        })
    }
    
    async waitForLambda (timeout, caption, func) {
        const start = new Date();
        let ms = 1;
        while(true)
        {
            let isOk = false;
            try
            {
                isOk = Boolean(await func());
            }
            catch(err)
            {
                isOk = false; 
            }
            // console.log(`Wait for ${triggerKey}: ${value} (${new Date() - start})`);
            if (isOk) {
                console.debug(`Waiter for ` + `[${caption}]`.magenta.bold + ` successfully ${"confirmed".magenta.bold} in ${new Date() - start} milliseconds.`);
                return true;
            }

            await this.delay(ms);
            ms = ms >= 128 ? ms : ms * 2;
            let elapsed = new Date() - start;
            if (elapsed > timeout) break;
        }

        console.warn(`Warning! Waiter for [${caption}] was incomplete in ${new Date() - start} milliseconds`.red);
        return false;
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
                console.debug(`Trigger ` + `[${triggerKey}]`.magenta.bold + ` successfully ${"confirmed".magenta.bold} in ${new Date() - start} milliseconds. Raised at ${value.toFixed(1)} milliseconds`);
                return ;
            }

            await this.delay(ms);
            ms = ms >= 128 ? ms : ms * 2;
            let elapsed = new Date() - start;
            if (elapsed > timeout) break;
        }

        console.warn(`Warning! trigger [${triggerKey}] was not raised in ${new Date() - start} milliseconds`.red);
        return false;
    };

    delay(ms) {
        return new Promise(res => setTimeout(res, ms));
    } 

    async saveScreenshot (fileName) {
        const ss = await this.Protocol.Page.captureScreenshot({format: 'png', fromSurface: true});
        file.writeFile(fileName, ss.data, 'base64', function (err) {
            if (err) console.error(`${fileName} screenshot error: ${err}`.red);
        });
    }
} 

module.exports = TestContext;
