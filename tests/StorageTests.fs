module StorageTests

open Xunit
open Xunit.Abstractions
open FinvizScraper

type StorageTests(output:ITestOutputHelper) =

    let generateTicker() =
        "stock" + System.Guid.NewGuid().ToString()
    
    let generateScreener() =
        "screener" + System.Guid.NewGuid().ToString()

    let saveAndReturnTestStock ticker =
        Storage.saveStock ticker "Apple Inc." "Technology" "Computer Hardware" "United States"

    [<Fact>]
    let ``Getting stock works`` () =
        
        let ticker = generateTicker()

        let stockOrNone = Storage.getStockByTicker ticker
        Assert.True(stockOrNone.IsNone)

        let stock = saveAndReturnTestStock ticker

        Assert.True(stock.id > 0)
        Assert.Equal(ticker, stock.ticker)
        Assert.Equal("Apple Inc.", stock.company)
        Assert.Equal("Technology", stock.sector)
        Assert.Equal("Computer Hardware", stock.industry)
        Assert.Equal("United States", stock.country)

        let deleted = Storage.deleteStock stock.id
        Assert.Equal(1, deleted)

        let subsequent = Storage.getStockByTicker ticker

        Assert.True(subsequent.IsNone)

    [<Fact>]
    let ``Storing and retrieving screener works`` () =
        
        let screenerName = generateScreener()

        let screener = Storage.getScreenerByName screenerName
        Assert.True(screener.IsNone)

        let (id, name) = Storage.saveScreener screenerName "https://finviz.com/screener.ashx?v=111&s=ta_newhigh&f=fa_salesqoq_high,sh_avgvol_o200,sh_opt_optionshort,sh_price_o10,sh_relvol_o1.5,ta_perf_dup&ft=4&o=-volume"

        Assert.True(id > 0)
        Assert.Equal(screenerName, name)

        let subsequent = Storage.getScreenerByName screenerName
        Assert.True(subsequent.IsSome)
        let (_, subsequentName) = subsequent.Value
        Assert.Equal(screenerName, subsequentName)

        Storage.deleteScreener id |> ignore

        let subsequent = Storage.getScreenerByName screenerName
        Assert.True(subsequent.IsNone)

    // write test for screener results
    [<Fact>]
    let ``Storing and retrieving screener results works`` () =
        
        let screenerName = generateScreener()
        let ticker = generateTicker()

        let (screenerId, _) = Storage.saveScreener screenerName "https://finviz.com/screener.ashx?v=111&s=ta_newhigh&f=fa_salesqoq_high,sh_avgvol_o200,sh_opt_optionshort,sh_price_o10,sh_relvol_o1.5,ta_perf_dup&ft=4&o=-volume"

        let date = FinvizConfig.getRunDate()

        Storage.deleteScreenerResults screenerId date |> ignore

        let stock = saveAndReturnTestStock ticker
        
        let result = {
            ticker = stock.ticker;
            company = "Apple Inc.";
            sector = "Technology";
            industry = "Computer Hardware";
            country = "United States";
            marketCap = "1,000,000,000";
            price = 100m;
            change = 10m;
            volume = 1000;
        }

        let stored = Storage.saveScreenerResult screenerId date stock result

        Assert.Equal(1, stored)

        Storage.deleteScreenerResults screenerId date |> ignore
        Storage.deleteStock stock.id |> ignore
        Storage.deleteScreener screenerId |> ignore