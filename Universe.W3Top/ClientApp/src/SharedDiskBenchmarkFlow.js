// polyfill for TextDecoder/Encoder https://stackoverflow.com/a/11411402/864690
import base32Encode from "base32-encode"
import base32Decode from "base32-decode"
import {TextDecoder, TextEncoder} from "text-encoding"

/*
window.location properties:
    hostname: "localhost"
    host: "localhost:5010"
    search: "?sdfsdfsdf"
    hash: "#45646"
    href: "http://localhost:5010/disk-benchmark?sdfsdfsdf#45646"
    origin: "http://localhost:5010"
    pathname: "/disk-benchmark"
    port: "5010"
    protocol: "http:"
 */

const fakeDiskBenchmarkResult =
    {
        "token":"88ffbad1-0f1a-4555-8285-d4bdd6790869",
        "createdAt":"2021-06-14T16:42:14.7052295",
        "mountPath":"/transient-builds",
        "fileSystem":"ext4",
        "engine":"libaio",
        "engineVersion":"2.21",
        "workingSetSize":1048576,
        "o_Direct":"True",
        "allocate":382909138.9788223,
        "allocateCpuUsage":{
            "user":0.0,
            "kernel":1.6096720548807608
        },
        "seqRead":1633664918.2644455,
        "seqReadCpuUsage":{
            "user":0.0,
            "kernel":0.7140995472778577
        },
        "seqWrite":1999622948.9514863,
        "seqWriteCpuUsage":{
            "user":0.010899906593854099,
            "kernel":0.7651999231575354
        },
        "randomAccessBlockSize":4096,
        "threadsNumber":16,
        "randRead1T":40894228.84416376,
        "randRead1TCpuUsage":{
            "user":0.0,
            "kernel":0.6369994434977012
        },
        "randWrite1T":38587337.604287215,
        "randWrite1TCpuUsage":{
            "user":0.005999997772728099,
            "kernel":0.646799346677102
        },
        "randReadNT":99194725.49338001,
        "randReadNTCpuUsage":{
            "user":0.010999746062534415,
            "kernel":0.9659997313705755
        },
        "randWriteNT":97412166.35418832,
        "randWriteNTCpuUsage":{
            "user":0.0,
            "kernel":0.9919996614756711
        }
    };

export class SharedDiskBenchmarkFlow
{

    static isSharedBenchmarkResult() {
        const loc = window && window.location ? window.location : null;
        if (!loc) return false;
        return loc.hostname === "my-drive.github.io";
    }
    
    static tryParse()
    {
        const loc = window && window.location ? window.location : null;
        if (!loc) return null;

        if (loc.search === "?v=1") {
            const rawHash = loc.hash;
            if (rawHash.length > 1 && rawHash.substring(0,1) == '#') {
                const base32encoded = rawHash.substring(1);
                try {
                    const bytes = base32Decode(base32encoded, 'RFC4648');
                    const jsonString = new TextDecoder().decode(bytes);
                    const ret = JSON.parse(jsonString);
                    return ret;
                }
                catch (e) {
                    return null;
                }
            }
        }
        // WORKS
        // return fakeDiskBenchmarkResult;
    }

    // https://my-drive.github.io/?v=1#PMRHI33LMVXCEORCHA4GMZTCMFSDCLJQMYYWCLJUGU2TKLJYGI4DKLLEGRRGIZBWG44TAOBWHERCYITDOJSWC5DFMRAXIIR2EIZDAMRRFUYDMLJRGRKDCNR2GQZDUMJUFY3TANJSGI4TKIRMEJWW65LOORIGC5DIEI5CEL3UOJQW443JMVXHILLCOVUWYZDTEIWCEZTJNRSVG6LTORSW2IR2EJSXQ5BUEIWCEZLOM5UW4ZJCHIRGY2LCMFUW6IRMEJSW4Z3JNZSVMZLSONUW63RCHIRDELRSGERCYITXN5ZGW2LOM5JWK5CTNF5GKIR2GEYDIOBVG43CYITPL5CGS4TFMN2CEORCKRZHKZJCFQRGC3DMN5RWC5DFEI5DGOBSHEYDSMJTHAXDSNZYHAZDEMZMEJQWY3DPMNQXIZKDOB2VK43BM5SSEOT3EJ2XGZLSEI5DALBCNNSXE3TFNQRDUMJOGYYDSNRXGIYDKNBYHAYDONRQHB6SYITTMVYVEZLBMQRDUMJWGMZTMNRUHEYTQLRSGY2DINBVGUWCE43FOFJGKYLEINYHKVLTMFTWKIR2PMRHK43FOIRDUMBMEJVWK4TOMVWCEORQFY3TCNBQHE4TKNBXGI3TOOBVG43X2LBCONSXCV3SNF2GKIR2GE4TSOJWGIZDSNBYFY4TKMJUHA3DGLBCONSXCV3SNF2GKQ3QOVKXGYLHMURDU6ZCOVZWK4RCHIYC4MBRGA4DSOJZGA3DKOJTHA2TIMBZHEWCE23FOJXGK3BCHIYC4NZWGUYTSOJZGIZTCNJXGUZTKND5FQRHEYLOMRXW2QLDMNSXG42CNRXWG22TNF5GKIR2GQYDSNRMEJ2GQ4TFMFSHGTTVNVRGK4RCHIYTMLBCOJQW4ZCSMVQWIMKUEI5DIMBYHE2DEMRYFY4DINBRGYZTONRMEJZGC3TEKJSWCZBRKRBXA5KVONQWOZJCHJ5SE5LTMVZCEORQFQRGWZLSNZSWYIR2GAXDMMZWHE4TSNBUGM2DSNZXGAYTE7JMEJZGC3TEK5ZGS5DFGFKCEORTHA2TQNZTGM3S4NRQGQZDQNZSGE2SYITSMFXGIV3SNF2GKMKUINYHKVLTMFTWKIR2PMRHK43FOIRDUMBOGAYDKOJZHE4TSNZXG4ZDOMRYGA4TSLBCNNSXE3TFNQRDUMBOGY2DMNZZHEZTINRWG43TCMBSPUWCE4TBNZSFEZLBMRHFIIR2HE4TCOJUG4ZDKLRUHEZTGOBQGAYSYITSMFXGIUTFMFSE4VCDOB2VK43BM5SSEOT3EJ2XGZLSEI5DALRQGEYDSOJZG42DMMBWGI2TGNBUGE2SYITLMVZG4ZLMEI5DALRZGY2TSOJZG4ZTCMZXGA2TONJVPUWCE4TBNZSFO4TJORSU4VBCHI4TONBRGIYTMNROGM2TIMJYHAZTELBCOJQW4ZCXOJUXIZKOKRBXA5KVONQWOZJCHJ5SE5LTMVZCEORQFQRGWZLSNZSWYIR2GAXDSOJRHE4TSNRWGE2DONJWG4YTC7JMEJRXEZLBORSWIRDBORSSEORCJJ2W4IBRGQWCAMRQGIYSE7I
    static buildLink(selectedRow) {
        const jsonString = JSON.stringify(selectedRow);
        var uint8array = new TextEncoder().encode(jsonString);
        const safeUrlEncoded = base32Encode(uint8array, 'RFC4648', { padding: false });
        const ret = `https://my-drive.github.io/?v=1#${safeUrlEncoded}`;
        return ret;
    }
}