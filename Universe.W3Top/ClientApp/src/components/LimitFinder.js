let __limitFinder_Thresholds = null;

export function findLimit(num)
{
    let buildLimits = () => {
        let base = [1, 4, 8, 12, 16, 20, 24, 32, 40, 48, 64, 80];
        __limitFinder_Thresholds = [];
        for (var i = 0; i < 14; i++) {
            for (let j = 0; j < base.length; j++) {
                __limitFinder_Thresholds.push(base[j]);
                base[j] = base[j] * 10;
            }
        }
    }
    
    if (__limitFinder_Thresholds === null)
        buildLimits();

    num=num*1.03;
    for(let i=0; i < __limitFinder_Thresholds.length; i++) {
        if (num > __limitFinder_Thresholds[i]) continue;
        return __limitFinder_Thresholds[i];
    }

    return num;
}
