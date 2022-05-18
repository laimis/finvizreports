module ReportTests

open Xunit
open Xunit.Abstractions
open System

type ReportTests(output:ITestOutputHelper) =
    do
        FinvizScraper.Storage.Reports.configureConnectionString (System.Environment.GetEnvironmentVariable(StorageTests.dbEnvironmentVariableName))
        FinvizScraper.Storage.Storage.configureConnectionString (System.Environment.GetEnvironmentVariable(StorageTests.dbEnvironmentVariableName))

    let getTestScreener = 
        FinvizScraper.Storage.Storage.getScreenerByName StorageTests.testScreenerName

    let getTestSector = "Energy"
    let getTestIndustry = "Agricultural Inputs"
    let getTestCountry = "USA"

    let getTestStartDate() = DateTime.Now.AddDays(-7)
    let getTestEndDate() = DateTime.Now

    let topGroupingTest resultGenerator containsMember =

        let screener = getTestScreener
        
        match screener with
            | Some screener ->
                let grouping = screener |> resultGenerator
                let length = grouping |> Seq.length
                Assert.NotEqual(0, length)

                let index = Seq.tryFind (fun (name, _) -> name = containsMember) grouping
                match index with
                    | Some _ ->
                        Assert.True(true)
                    | None ->
                        Assert.True(false, $"{containsMember} not found in {grouping}")

            | None -> Assert.True(false, "Expected screener to be found")

    let parseDate dateString =
        System.DateTime.ParseExact(dateString, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)

    [<Theory>]
    [<InlineData("2022-04-01")>]
    let ``Getting sectors works`` dateStr =
        let date = parseDate dateStr
        "Energy" |> topGroupingTest (fun x-> FinvizScraper.Storage.Reports.topSectors x.id date)

    [<Theory>]
    [<InlineData("2022-04-01")>]
    let ``Getting industry works`` dateStr =
        let date = parseDate dateStr
        "Telecom Services" |> topGroupingTest (fun x-> FinvizScraper.Storage.Reports.topIndustries x.id date)

    [<Theory>]
    [<InlineData("2022-04-01")>]
    let ``Getting countries works`` dateStr =
        let date = parseDate dateStr
        "USA" |> topGroupingTest (fun x-> FinvizScraper.Storage.Reports.topCountries x.id date)

    [<Fact>]
    let ``Screener result list works``() =
        
        let expectedScreenerCount = 
            FinvizScraper.Storage.Storage.getScreeners()
            |> List.filter (fun x -> x.name.StartsWith("screener") |> not)
            |> Seq.length

        let screenerResults = FinvizScraper.Storage.Reports.getLatestScreeners()

        Assert.Equal(expectedScreenerCount, screenerResults.Length)

    [<Fact>]
    let ``Particular screener results list works``() =

        let screener = FinvizScraper.Storage.Reports.getLatestScreeners().Head

        let results = FinvizScraper.Storage.Reports.getScreenerResults screener.screenerid (screener.date.ToString("yyyy-MM-dd"))

        Assert.NotEmpty(results)

    [<Fact>]
    let ``Particular screener daily counts work``() =

        let screener = getTestScreener

        let results = FinvizScraper.Storage.Reports.getDailyCountsForScreener screener.Value.id 7

        Assert.NotEmpty(results)

        let (_,firstCount) = results.Item(0)
        Assert.True(firstCount > 0)

    [<Fact>]
    let ``Date range sector grouping works``() =
        "Energy" |> topGroupingTest (
            fun x -> 
                FinvizScraper.Storage.Reports.topSectorsOverDays x.id (getTestStartDate()) (getTestEndDate())
            )

    [<Fact>]
    let ``Date range industry grouping works``() =
        "Food Distribution" |> topGroupingTest (
            fun x -> 
                FinvizScraper.Storage.Reports.topIndustriesOverDays x.id (getTestStartDate()) (getTestEndDate())
            )

    [<Fact>]
    let ``Date range country grouping works``() =
        "USA" |> topGroupingTest (fun x -> FinvizScraper.Storage.Reports.topCountriesOverDays x.id (DateTime.Now.AddDays(-7)) DateTime.Now)

    [<Fact>]
    let ``getting screener results for ticker works``() =
        let ticker = FinvizScraper.Core.StockTicker.create "cutr"
        let results = FinvizScraper.Storage.Reports.getScreenerResultsForTicker ticker
        Assert.NotEmpty(results)

    [<Fact>]
    let ``getting daily counts for screeners filtered by sector works``() =
        let screener = getTestScreener
        let sector = getTestSector

        let results = FinvizScraper.Storage.Reports.getDailyCountsForScreenerAndSector screener.Value.id sector 7
        
        Assert.NotEmpty(results)

    [<Fact>]
    let ``getting daily counts for screeners filtered by industry works``() =
        let screener = getTestScreener
        let industry = getTestIndustry

        let results = FinvizScraper.Storage.Reports.getDailyCountsForScreenerAndIndustry screener.Value.id industry 30
        
        Assert.NotEmpty(results)

    [<Fact>]
    let ``getting daily counts for screeners filtered by country works``() =
        let screener = getTestScreener
        let industry = getTestCountry

        let results = FinvizScraper.Storage.Reports.getDailyCountsForScreenerAndCountry screener.Value.id industry 30
        
        Assert.NotEmpty(results)

    [<Fact>]
    let ``getting trending industries works``() =
        let results = 
            FinvizScraper.Core.FinvizConfig.NewHighsScreener
            |> FinvizScraper.Storage.Reports.getTopIndustriesForScreener 14

        Assert.NotEmpty(results)

    [<Fact>]
    let ``getting trending sectors works``() =
        let results = 
            FinvizScraper.Core.FinvizConfig.NewHighsScreener
            |> FinvizScraper.Storage.Reports.getTopSectorsForScreener 14

        Assert.NotEmpty(results)