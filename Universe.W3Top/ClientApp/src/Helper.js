import * as Enumerable from "linq-es2015"

export const toConsole = function(caption, obj) {
    if (process.env.NODE_ENV !== 'production') {
        console.log(`--===**** ${caption} ****===--`);
        console.log(obj);
        console.log('\r\n');
    }
}

export const log = function(caption) {
    if (process.env.NODE_ENV !== 'production') {
        console.log(`${caption}`);
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