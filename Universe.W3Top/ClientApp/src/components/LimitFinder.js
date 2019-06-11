let thresholds = null;

export function findLimit(num)
{
    const buildLimits = () => {
        let base = [4, 8, 12, 16, 20, 24, 32, 40, 48, 64, 80, 100];
        let ret = [];
        for (let i = 0; i < 14; i++) {
            for (let j = 0; j < base.length; j++) {
                ret.push(base[j]);
                base[j] = base[j] * 10;
            }
        }
        return ret;
    };
    
    if (thresholds === null)
        thresholds = buildLimits();

    num = num * 1.03;
    for(let i=0; i < thresholds.length; i++) {
        if (num > thresholds[i]) continue;
        return thresholds[i];
    }

    return num;
}
