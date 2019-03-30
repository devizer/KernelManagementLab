export default class Helper
{
    static tryGetProperty(obj, propertyName) {
        if (obj && (propertyName in obj))
            return [true, obj[propertyName]]
        
        return [false, undefined];
    }
}