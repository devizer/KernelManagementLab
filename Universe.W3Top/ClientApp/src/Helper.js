export default class Helper
{
    static tryGetProperty(obj, propertyName) {
        if (typeof obj == "object" && (propertyName in obj))
            return [true, obj[propertyName]]
        
        return [false, undefined];
    }
}