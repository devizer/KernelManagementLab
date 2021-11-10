using System;
using System.Linq;
using Universe.DiskBench;

namespace Universe.Dashboard.DAL
{
    public static class DiskBenchmarkDataAccessExtensions
    {
        // Before: https://my-drive.github.io/?v=2&performance=PMRHI33LMVXCEORCMJQTAOLCGFRDKLJUMQ4GGLJUGFTGILLBGBRDILJTMNRDANJVGBTGEODEMYRCYITDOJSWC5DFMRAXIIR2EIZDAMRRFUYDMLJRGRKDANB2GMZDUNBSFY2TSNRRGM3TOIRMEJWW65LOORIGC5DIEI5CEL3UOJQW443JMVXHILLCOVUWYZDTEIWCEZTJNRSVG6LTORSW2IR2EJSXQ5BUEIWCEZLOM5UW4ZJCHIRGY2LCMFUW6IRMEJSW4Z3JNZSVMZLSONUW63RCHIRDELRSGERCYITXN5ZGW2LOM5JWK5CTNF5GKIR2GEYDIOBVG43CYITPL5CGS4TFMN2CEORCKRZHKZJCFQRGC3DMN5RWC5DFEI5DGNBWGYYTSMRXG4XDGMJSGM2DMNRMEJQWY3DPMNQXIZKDOB2VK43BM5SSEOT3EJ2XGZLSEI5DCLRRG44TONZVGQZDSNBWGYYDEMZUFQRGWZLSNZSWYIR2GB6SYITTMVYVEZLBMQRDUMJWGI3DGNBSGY2DQLRSGEZTGOBRFQRHGZLRKJSWCZCDOB2VK43BM5SSEOT3EJ2XGZLSEI5DALRQGE2DKOJZHA4TGNBYGYZTKMBTGA3CYITLMVZG4ZLMEI5DALRXGI4DQOJZGU4DENZVGIZDMMBTPUWCE43FOFLXE2LUMURDUMJXGM2TGOJTGI3TELRXGY4DQNJVGYWCE43FOFLXE2LUMVBXA5KVONQWOZJCHJ5SE5LTMVZCEORQFQRGWZLSNZSWYIR2GAXDQNZXGM4TSOBZGU2DCOJXGM2DQ7JMEJZGC3TEN5WUCY3DMVZXGQTMN5RWWU3JPJSSEORUGA4TMLBCORUHEZLBMRZU45LNMJSXEIR2GE3CYITSMFXGIUTFMFSDCVBCHI2DQOBWGM3DIMZOGEYDINJZGY4TMLBCOJQW4ZCSMVQWIMKUINYHKVLTMFTWKIR2PMRHK43FOIRDUMBMEJVWK4TOMVWCEORQFY3TAOBSHE4TKNJYGUZDANRVGMYX2LBCOJQW4ZCXOJUXIZJRKQRDUNBTGQYTCMBUHEXDKMBWG4YDENBUGUWCE4TBNZSFO4TJORSTCVCDOB2VK43BM5SSEOT3EJ2XGZLSEI5DALRQGA2DSOJZGUYTCNRXHE2TAMBYGE2CYITLMVZG4ZLMEI5DALRWGU4DMOJZGQYDMMBWGI4TOMZSPUWCE4TBNZSFEZLBMRHFIIR2G42TANZYGAZTMLRWG4YTONZRGU3CYITSMFXGIUTFMFSE4VCDOB2VK43BM5SSEOT3EJ2XGZLSEI5DALRQHE2DQOJZGUZTOMJTHE2DINZTFQRGWZLSNZSWYIR2GAXDSMBUGA4TSNJVHAZDSMZUGM2TM7JMEJZGC3TEK5ZGS5DFJZKCEORZG42DCMRXGEZC4MZRGE2DQNBVGUWCE4TBNZSFO4TJORSU4VCDOB2VK43BM5SSEOT3EJ2XGZLSEI5DALBCNNSXE3TFNQRDUMBOHE4TQOJZHE3DSMZZGA4DOMRQHB6SYITDOJSWC5DFMRCGC5DFEI5CESTVNYQDCNBMEAZDAMRREJ6Q
        // After1: https://my-drive.github.io/?v=2&performance=PMRHI33LMVXCEORCMJQTAOLCGFRDKLJUMQ4GGLJUGFTGILLBGBRDILJTMNRDANJVGBTGEODEMYRCYITDOJSWC5DFMRAXIIR2EIZDAMRRFUYDMLJRGRKDANB2GMZDUNBSFY2TSNRRGM3TOIRMEJWW65LOORIGC5DIEI5CEL3UOJQW443JMVXHILLCOVUWYZDTEIWCEZTJNRSVG6LTORSW2IR2EJSXQ5BUEIWCEZLOM5UW4ZJCHIRGY2LCMFUW6IRMEJSW4Z3JNZSVMZLSONUW63RCHIRDELRSGERCYITXN5ZGW2LOM5JWK5CTNF5GKIR2GEYDIOBVG43CYITPL5CGS4TFMN2CEORCKRZHKZJCFQRGC3DMN5RWC5DFEI5DGNBWGYYTSMRXG4WCEYLMNRXWGYLUMVBXA5KVONQWOZJCHJ5SE5LTMVZCEORRFYYTOOJXG42TIMRZGQ3DMMBSGM2CYITLMVZG4ZLMEI5DA7JMEJZWK4KSMVQWIIR2GE3DENRTGQZDMNBYFQRHGZLRKJSWCZCDOB2VK43BM5SSEOT3EJ2XGZLSEI5DALRQGE2DKOJZHA4TGNBYGYZTKMBTGA3CYITLMVZG4ZLMEI5DALRXGI4DQOJZGU4DENZVGIZDMMBTPUWCE43FOFLXE2LUMURDUMJXGM2TGOJTGI3TGLBCONSXCV3SNF2GKQ3QOVKXGYLHMURDU6ZCOVZWK4RCHIYCYITLMVZG4ZLMEI5DALRYG43TGOJZHA4TKNBRHE3TGNBYPUWCE4TBNZSG63KBMNRWK43TIJWG6Y3LKNUXUZJCHI2DAOJWFQRHI2DSMVQWI42OOVWWEZLSEI5DCNRMEJZGC3TEKJSWCZBRKQRDUNBYHA3DGNRUGMWCE4TBNZSFEZLBMQYVIQ3QOVKXGYLHMURDU6ZCOVZWK4RCHIYCYITLMVZG4ZLMEI5DALRXGA4DEOJZGU2TQNJSGA3DKMZRPUWCE4TBNZSFO4TJORSTCVBCHI2DGNBRGEYDKMBMEJZGC3TEK5ZGS5DFGFKEG4DVKVZWCZ3FEI5HWITVONSXEIR2GAXDAMBUHE4TSNJRGE3DOOJVGAYDQMJUFQRGWZLSNZSWYIR2GAXDMNJYGY4TSNBQGYYDMMRZG4ZTE7JMEJZGC3TEKJSWCZCOKQRDUNZVGA3TQMBTG4WCE4TBNZSFEZLBMRHFIQ3QOVKXGYLHMURDU6ZCOVZWK4RCHIYC4MBZGQ4DSOJVGM3TCMZZGQ2DOMZMEJVWK4TOMVWCEORQFY4TANBQHE4TKNJYGI4TGNBTGU3H2LBCOJQW4ZCXOJUXIZKOKQRDUOJXGQYTENZRGIWCE4TBNZSFO4TJORSU4VCDOB2VK43BM5SSEOT3EJ2XGZLSEI5DALBCNNSXE3TFNQRDUMBOHE4TQOJZHE3DSMZZGA4DOMRQHB6SYITDOJSWC5DFMRCGC5DFEI5CESTVNYQDCNBMEAZDAMRREJ6Q
        // After2: https://my-drive.github.io/?v=2&performance=PMRHI33LMVXCEORCMJQTAOLCGFRDKLJUMQ4GGLJUGFTGILLBGBRDILJTMNRDANJVGBTGEODEMYRCYITDOJSWC5DFMRAXIIR2EIZDAMRRFUYDMLJRGRKDANB2GMZDUNBSFY2TSNRRGM3TOIRMEJWW65LOORIGC5DIEI5CEL3UOJQW443JMVXHILLCOVUWYZDTEIWCEZTJNRSVG6LTORSW2IR2EJSXQ5BUEIWCEZLOM5UW4ZJCHIRGY2LCMFUW6IRMEJSW4Z3JNZSVMZLSONUW63RCHIRDELRSGERCYITXN5ZGW2LOM5JWK5CTNF5GKIR2GEYDIOBVG43CYITPL5CGS4TFMN2CEORCKRZHKZJCFQRGC3DMN5RWC5DFEI5DGNBWGYYTSMRXG4WCEYLMNRXWGYLUMVBXA5KVONQWOZJCHJ5SE5LTMVZCEORRFYYTOOJXG42SYITLMVZG4ZLMEI5DA7JMEJZWK4KSMVQWIIR2GE3DENRTGQZDMNBYFQRHGZLRKJSWCZCDOB2VK43BM5SSEOT3EJ2XGZLSEI5DALRQGE2DMLBCNNSXE3TFNQRDUMBOG4ZDQOL5FQRHGZLRK5ZGS5DFEI5DCNZTGUZTSMZSG4ZSYITTMVYVO4TJORSUG4DVKVZWCZ3FEI5HWITVONSXEIR2GAWCE23FOJXGK3BCHIYC4OBXG42H2LBCOJQW4ZDPNVAWGY3FONZUE3DPMNVVG2L2MURDUNBQHE3CYITUNBZGKYLEONHHK3LCMVZCEORRGYWCE4TBNZSFEZLBMQYVIIR2GQ4DQNRTGY2DGLBCOJQW4ZCSMVQWIMKUINYHKVLTMFTWKIR2PMRHK43FOIRDUMBMEJVWK4TOMVWCEORQFY3TAOBTPUWCE4TBNZSFO4TJORSTCVBCHI2DGNBRGEYDKMBMEJZGC3TEK5ZGS5DFGFKEG4DVKVZWCZ3FEI5HWITVONSXEIR2GAXDAMBVFQRGWZLSNZSWYIR2GAXDMNJYGY4TS7JMEJZGC3TEKJSWCZCOKQRDUNZVGA3TQMBTG4WCE4TBNZSFEZLBMRHFIQ3QOVKXGYLHMURDU6ZCOVZWK4RCHIYC4MBZGQ4SYITLMVZG4ZLMEI5DALRZGA2DC7JMEJZGC3TEK5ZGS5DFJZKCEORZG42DCMRXGEZCYITSMFXGIV3SNF2GKTSUINYHKVLTMFTWKIR2PMRHK43FOIRDUMBMEJVWK4TOMVWCEORQFY4TSOL5FQRGG4TFMF2GKZCEMF2GKIR2EJFHK3RAGE2CYIBSGAZDCIT5
        public static DiskBenchmarkHistoryRow RoundBenchmarkHistoryRow(this DiskBenchmarkHistoryRow benchmark)
        {
            const int BANDWIDTH_FRACTIONS = 0, CPU_USAGE_FRACTIONS = 6;

            double? RoundBandwidth(double? bw)
            {
                return bw.HasValue ? bw = Math.Round(bw.Value, BANDWIDTH_FRACTIONS) : null;
            }
            benchmark.Allocate = RoundBandwidth(benchmark.Allocate);
            benchmark.SeqRead = RoundBandwidth(benchmark.SeqRead);
            benchmark.SeqWrite = RoundBandwidth(benchmark.SeqWrite);
            benchmark.RandRead1T = RoundBandwidth(benchmark.RandRead1T);
            benchmark.RandReadNT = RoundBandwidth(benchmark.RandReadNT);
            benchmark.RandWrite1T = RoundBandwidth(benchmark.RandWrite1T);
            benchmark.RandWriteNT = RoundBandwidth(benchmark.RandWriteNT);

            DiskBenchmarkHistoryRow.StepCpuUsage RoundCpuUsage(DiskBenchmarkHistoryRow.StepCpuUsage cpuUsage)
            {
                if (cpuUsage != null)
                {
                    cpuUsage.Kernel = Math.Round(cpuUsage.Kernel, CPU_USAGE_FRACTIONS);
                    cpuUsage.User = Math.Round(cpuUsage.User, CPU_USAGE_FRACTIONS);
                }

                return cpuUsage;
            }
            benchmark.AllocateCpuUsage = RoundCpuUsage(benchmark.AllocateCpuUsage);
            benchmark.SeqReadCpuUsage = RoundCpuUsage(benchmark.SeqReadCpuUsage);
            benchmark.SeqWriteCpuUsage = RoundCpuUsage(benchmark.SeqWriteCpuUsage);
            benchmark.RandRead1TCpuUsage = RoundCpuUsage(benchmark.RandRead1TCpuUsage);
            benchmark.RandReadNTCpuUsage = RoundCpuUsage(benchmark.RandReadNTCpuUsage);
            benchmark.RandWrite1TCpuUsage = RoundCpuUsage(benchmark.RandWrite1TCpuUsage);
            benchmark.RandWriteNTCpuUsage = RoundCpuUsage(benchmark.RandWriteNTCpuUsage);
            
            return benchmark;
        }


        public static DiskBenchmarkHistoryRow ToHistoryItem(this DiskBenchmarkEntity benchmark)
        {
            // TODO: Build history row on SaveChanges and store as additional column
            ProgressStep GetStep(ProgressStepHistoryColumn column) => benchmark.Report.Steps.FirstOrDefault(step => step.Column == column);
            double? GetSpeed(ProgressStepHistoryColumn column) => GetStep(column)?.AvgBytesPerSecond;

            DiskBenchmarkHistoryRow.StepCpuUsage GetStepCpuUsage(ProgressStepHistoryColumn column)
            {
                var step = GetStep(column);
                if (step != null)
                {
                    double? userUsage = step.CpuUsage?.UserUsage.TotalSeconds / step.Seconds;
                    double? kernelUsage = step.CpuUsage?.KernelUsage.TotalSeconds / step.Seconds;
                    return userUsage.HasValue && kernelUsage.HasValue
                        ? new DiskBenchmarkHistoryRow.StepCpuUsage() {User = userUsage.Value, Kernel = kernelUsage.Value}
                        : null;
                }

                return null;
            }
            
            return new DiskBenchmarkHistoryRow()
            {
                Token = benchmark.Token,
                CreatedAt = benchmark.CreatedAt,
                MountPath = benchmark.Args.WorkFolder,
                FileSystem = benchmark.Environment?.FileSystems,
                Engine = benchmark.Environment?.Engine,
                EngineVersion = benchmark.Environment?.EngineVersion,
                WorkingSetSize = benchmark.Args.WorkingSetSize,
                O_Direct = Convert.ToString(GetStep(ProgressStepHistoryColumn.CheckODirect)?.Value),
                Allocate = GetSpeed(ProgressStepHistoryColumn.Allocate),
                AllocateCpuUsage = GetStepCpuUsage(ProgressStepHistoryColumn.Allocate),
                SeqRead = GetSpeed(ProgressStepHistoryColumn.SeqRead),
                SeqReadCpuUsage = GetStepCpuUsage(ProgressStepHistoryColumn.SeqRead), 
                SeqWrite = GetSpeed(ProgressStepHistoryColumn.SeqWrite),
                SeqWriteCpuUsage = GetStepCpuUsage(ProgressStepHistoryColumn.SeqWrite),
                RandomAccessBlockSize = benchmark.Args.RandomAccessBlockSize,
                ThreadsNumber = benchmark.Args.ThreadsNumber,
                RandRead1T = GetSpeed(ProgressStepHistoryColumn.RandRead1T),
                RandRead1TCpuUsage = GetStepCpuUsage(ProgressStepHistoryColumn.RandRead1T),
                RandWrite1T = GetSpeed(ProgressStepHistoryColumn.RandWrite1T),
                RandWrite1TCpuUsage = GetStepCpuUsage(ProgressStepHistoryColumn.RandWrite1T),
                RandReadNT = GetSpeed(ProgressStepHistoryColumn.RandReadNT),
                RandReadNTCpuUsage = GetStepCpuUsage(ProgressStepHistoryColumn.RandReadNT),
                RandWriteNT = GetSpeed(ProgressStepHistoryColumn.RandWriteNT),
                RandWriteNTCpuUsage = GetStepCpuUsage(ProgressStepHistoryColumn.RandWriteNT),
            };
        }
    }
    
    // Table row on the history UI
    public class DiskBenchmarkHistoryRow
    {
        public Guid Token { get; set; } // It is used for tests only
        public DateTime CreatedAt { get; set; }
        public string MountPath { get; set; }
        public string FileSystem { get; set; }
        public string Engine { get; set; }
        public string EngineVersion { get; set; }
        public long WorkingSetSize { get; set; }
        public string O_Direct { get; set; } // "" (disabled) | "True" (present) | "False" (absent)
        public double? Allocate { get; set; }
        public StepCpuUsage AllocateCpuUsage { get; set; }
        public double? SeqRead { get; set; }
        public StepCpuUsage SeqReadCpuUsage { get; set; }
        public double? SeqWrite { get; set; }
        public StepCpuUsage SeqWriteCpuUsage { get; set; }
        public int RandomAccessBlockSize { get; set; }
        public int ThreadsNumber { get; set; }
        public double? RandRead1T { get; set; } 
        public StepCpuUsage RandRead1TCpuUsage { get; set; }
        public double? RandWrite1T { get; set; } 
        public StepCpuUsage RandWrite1TCpuUsage { get; set; }
        public double? RandReadNT { get; set; } 
        public StepCpuUsage RandReadNTCpuUsage { get; set; }
        public double? RandWriteNT { get; set; }
        public StepCpuUsage RandWriteNTCpuUsage { get; set; }

        public class StepCpuUsage
        {
            public double User { get; set; }
            public double Kernel { get; set; }
        }
    }

}