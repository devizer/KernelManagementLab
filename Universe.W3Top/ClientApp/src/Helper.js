export default class Helper
{
    static tryGetProperty(obj, propertyName) {
        if (typeof obj == "object" && (propertyName in obj))
            return [true, obj[propertyName]]
        
        return [false, undefined];
    }
    
    static isInterfaceActive(globalDataSource, interfaceName)
    {
        let isInactive = globalDataSource.interfaceTotals[interfaceName].isInactive;
        return !isInactive;
    }

    static getActiveInterfaceList(globalDataSource, interfaceName)
    {
        let isInactive = globalDataSource.interfaceTotals[interfaceName].isInactive;
        return !isInactive;
    }

    static objectIsNotEmpty(obj) {
        return typeof obj == "object" && Object.keys(obj).length > 0;
    }
}