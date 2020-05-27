import React from 'react';
import * as Enumerable from "linq-es2015"
import MomentFormat from 'moment';

export const toConsole = function(caption, obj) {
    if (process.env.NODE_ENV !== 'production') {
        console.log(`--===**** ${caption} ****===--`);
        console.log(obj);
        console.log('\r\n');
    }
}

export const runInBackground = callBack => {
    if (typeof callBack !== "function") console.warn(`callback is not a function. it's a ${typeof callBack}`);
    
    if (window && window.requestIdleCallback)
        window.requestIdleCallback(callBack);
    else
        callBack();
}

export const isDocumentHidden = () => {
    // https://www.w3.org/TR/page-visibility-2/#idl-def-document-visibilitystate
    let isHidden = false;
    if (document && document.visibilityState && document.visibilityState !== 'visible') { isHidden = true; }
    return isHidden;
}

global.ApplicationLevelTriggers = {};
export const notifyTrigger = (triggerName) => {
    const start = window.LoadingStartedAt || 0;
    const current = (window.performance && window.performance.now) ? window.performance.now() : +new Date();
    if (window.ApplicationLevelTriggers === undefined) window.ApplicationLevelTriggers = {};
    let prev = window.ApplicationLevelTriggers[triggerName];
    if (prev === undefined) {
        prev = {first: current - start};
        window.ApplicationLevelTriggers[triggerName] = prev;
    }
    
    prev.last = current - start;
};

export const log = function(caption) {
    if (process.env.NODE_ENV !== 'production') {
        console.log(`${caption}`);
    }
}

export class Chart {
    static tooltipTitleFormat(secondsPer) {
        if (!secondsPer) secondsPer = 1;
        return (x,index) => {
            let mo = MomentFormat();
            mo.add((x-60) * secondsPer, 'seconds');
            return mo.format("LTS");
        }
    }
}

export class Numbers
{
    static isInRange(arg, minNumber, maxNumber)
    {
        const v = Number.parseFloat(arg);
        return v >= minNumber && v <= maxNumber;
    }

    static greaterOrEqual(arg, minNumber)
    {
        const v = Number.parseFloat(arg);
        return v >= minNumber;
    }
    
    static isInt(value)
    {
        return !isNaN(Numbers.filterInt(value));
    }

    static filterInt (value) {
        if (/^(\-|\+)?([0-9]+|Infinity)$/.test(value)) {
            return Number(value);
        }
        
        return NaN;
    }

}

export class Common {
    
    static tryGetProperty(obj, propertyName) {
        if (typeof obj == "object" && (propertyName in obj))
            return [true, obj[propertyName]];

        return [false, null];
    }

    // object has at least one property
    static objectIsNotEmpty(obj) {
        return typeof obj === "object" && Object.keys(obj).length > 0;
    }

    static htmlEncode(str) {
        return str.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    }

    static formatAnything(number, fractionCount = 2, spacer = ' ', unit = "B") {

        let scale = 1;
        if (fractionCount === 1) scale = 10.0;
        else if (fractionCount === 2) scale = 100.0;
        else if (fractionCount === 3) scale = 1000.0;
        else if (fractionCount > 0) for(let i=0; i<fractionCount; i++) scale=scale*10.0;

        let format = num => (Math.round(num * scale) / scale).toLocaleString(undefined, {useGrouping: true});
        if (number === null || number === undefined)
            return null;

        if (number === 0)
            return "0";

        if (number < 1499)
            return `${format(number)}${spacer}${unit}`;

        if (number < 1499999)
            return `${format(number / 1024.0)}${spacer}K${unit}`;

        if (number < 1499999999)
            return `${format(number / 1024.0 / 1024.0)}${spacer}M${unit}`;

        if (number < 1499999999999)
            return `${format(number / 1024.0 / 1024.0 / 1024.0)}${spacer}G${unit}`;

        return `${format(number / 1024.0 / 1024.0 / 1024.0 / 1024.0)}${spacer}T${unit}`;
    }
    
    static formatBytes(number, fractionCount) {
        if (typeof fractionCount !== "number") fractionCount = 2; 
            return Common.formatAnything(number, fractionCount, ' ', "B");
    }
    
    static formatDuration(totalSeconds, styleNormal, styleZero) {
        const seconds = totalSeconds % 60;
        const minutes = Math.floor(totalSeconds / 60) % 60;
        const hours = Math.floor(totalSeconds / 3600);
        // return (<>{hours}<span style={styleZero}>:</span>{minutes}<span style={styleZero}>:</span>{seconds}</>)

        const sSeconds = seconds > 9 ? seconds : ("0" + seconds);
        const sMinutes = minutes > 9 ? minutes : ("0" + minutes);

        let comHours;
        if (hours > 0) 
            comHours = (<span style={styleNormal}>{hours}</span>);
        else
            comHours = (<span style={styleZero}>0</span>);
        
        return (<>{comHours}<span style={styleZero}>:</span>{sMinutes}<span style={styleZero}>:</span>{sSeconds}</>)
    }
    
    static formatInfoHeader(text) {
        if (text === undefined || text === null) return null;
        
        // OK for Android 5, Safari 9.1.3 and Chrome 41 
        var arr = text.split(/[,]/g);
        var ret = arr
            .map(x => `<span style='white-space: nowrap'>${x.trim()}</span>`)
            .join(', ');
        
        return ret;
    }
}

export class System
{
    static getHostName(message)
    {
        try {
            let hostname = message.system.hostname;
            if (hostname.length >= 0) {
                return hostname; 
            }
        }catch{}
        
        return null;
    }
}

export class Disks
{
    static getOptionalMountsProperty(globalDataSource)
    {
        let [hasMounts, mounts] = Common.tryGetProperty(globalDataSource, "mounts");
        return hasMounts ? mounts : null;
    }
}

export class NetDev
{
    // returns either [null] or [object]
    static getOptionalInterfacesProperty(globalDataSource)
    {
        let [hasNet, net] = Common.tryGetProperty(globalDataSource, "net");
        if (hasNet)
        {
            let [hasInterfaces, interfaces] = Common.tryGetProperty(net, "interfaces");
            return hasInterfaces ? interfaces : null;
        }
        
        return null;
    }
    
    // metadata check is skipped for performance - should not be used outside of 
    // Helper.NetDev class
    static isInterfaceActive(globalDataSource, interfaceName)
    {
        let isInactive = globalDataSource.net.interfaceTotals[interfaceName].isInactive;
        return !isInactive;
    }

    static getActiveInterfaceList(globalDataSource)
    {
        return NetDev.getActiveInterfaceListByActiveStatus(globalDataSource, true);
    }

    static getInActiveInterfaceList(globalDataSource)
    {
        return NetDev.getActiveInterfaceListByActiveStatus(globalDataSource, false);
    }

    // activeStatus is true|false
    static getActiveInterfaceListByActiveStatus(globalDataSource, activeStatus)
    {
        // interfaces - object: key is interfaceName
        let interfaces = NetDev.getOptionalInterfacesProperty(globalDataSource);
        if (interfaces !== null) {
            return Enumerable.AsEnumerable(Object.keys(interfaces))
                .Where(interfaceName => interfaces.hasOwnProperty(interfaceName))
                .Where(interfaceName => NetDev.isInterfaceActive(globalDataSource, interfaceName) === activeStatus)
                .Select(interfaceName => interfaceName)
                .ToArray();
        }

        throw Error("interface property is expected for globalDataSource");
    }

}