import * as Enumerable from "linq-es2015"

export class Common
{
    static tryGetProperty(obj, propertyName) {
        if (typeof obj == "object" && (propertyName in obj))
            return [true, obj[propertyName]]
        
        return [false, null];
    }

    // object has at least one property
    static objectIsNotEmpty(obj) {
        return typeof obj == "object" && Object.keys(obj).length > 0;
    }
}

export class NetDev
{
    // returns either [null] or [object]
    static getOptionalInterfacesProperty(globalDataSource)
    {
        let [hasInterfaces, interfaces] = Common.tryGetProperty(globalDataSource, "interfaces");
        return hasInterfaces ? interfaces : null;
    }
    
    static isInterfaceActive(globalDataSource, interfaceName)
    {
        let isInactive = globalDataSource.interfaceTotals[interfaceName].isInactive;
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