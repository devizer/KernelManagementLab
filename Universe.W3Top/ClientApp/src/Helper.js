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

export class Common {
    
    static tryGetProperty(obj, propertyName) {
        if (typeof obj == "object" && (propertyName in obj))
            return [true, obj[propertyName]];

        return [false, null];
    }

    // object has at least one property
    static objectIsNotEmpty(obj) {
        return typeof obj == "object" && Object.keys(obj).length > 0;
    }

    static htmlEncode(str) {
        return str.replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");
    }

    static formatBytes(number) {

        let format = num => (Math.round(num * 100.0) / 100).toLocaleString(undefined, {useGrouping: true});
        if (number == 0)
            return "0";

        if (number < 1499)
            return format(number) + " B";

        if (number < 1499999)
            return format((number / 1024.0)) + " Kb";

        if (number < 1499999999)
            return format((number / 1024.00 / 1024.00)) + " Mb";

        if (number < 1499999999999)
            return format((number / 1024 / 1024 / 1024)) + " Gb";

        return format((number / 1024 / 1024 / 1024 / 1024)) + " Tb";
    }
    
    static formatInfoHeader(text) {
        if (text === undefined || text === null) return null;
        
        // TODO: CHECK IT FOR Android 5, Safari 9.1.3 and Chrome 41 
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