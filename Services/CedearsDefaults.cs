using static Services.CedearsService;

namespace Services;

public static class CedearsDefaults
{
    public static readonly CedearReq[] Pairs = new[]
    {
        new CedearReq("AAPL.BA",  "AAPL", 10m),
        new CedearReq("MSFT.BA",  "MSFT", 5m),
        new CedearReq("NVDA.BA",  "NVDA", 20m),
        new CedearReq("AMZN.BA",  "AMZN", 72m),
        new CedearReq("GOOGL.BA", "GOOGL", 29m),
        new CedearReq("META.BA",  "META",  30m),
        new CedearReq("TSLA.BA",  "TSLA",  15m),
        new CedearReq("BRKB.BA",  "BRK-B", 45m),
        new CedearReq("KO.BA",    "KO",    5m),

        new CedearReq("YPFD.BA",  "YPF",   null),
        new CedearReq("PAMP.BA",  "PAM",   null),
        new CedearReq("VIST.BA",  "VIST",  null),
        new CedearReq("BMA.BA",   "BMA",   null),
        new CedearReq("GGAL.BA",  "GGAL",  null),
        new CedearReq("SUPV.BA",  "SUPV",  null),
        new CedearReq("CEPU.BA",  "CEPU",  null),
        new CedearReq("LOMA.BA",  "LOMA",  null),

    };
}
