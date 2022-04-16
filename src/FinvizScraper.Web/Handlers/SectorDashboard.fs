namespace FinvizScraper.Web.Handlers

module SectorDashboard =
    open FinvizScraper.Web.Shared
    open Giraffe.ViewEngine.HtmlElements
    open FinvizScraper.Storage
    open Giraffe.ViewEngine.Attributes
    open FinvizScraper.Web.Shared.Views

    let handler sectorName =
        let screeners = Storage.getScreeners()

        let days = FinvizScraper.Core.FinvizConfig.dayRange

        let list = [for i in -days .. 0 -> (System.DateTime.UtcNow.Date.AddDays(i),0) ]

        let charts = 
            screeners
            |> List.map (fun screener ->
                let data = Reports.getDailyCountsForScreenerAndSector screener.id sectorName days

                let mapped = data |> Map.ofList

                list
                |> List.map(fun (date,count) ->
                    let found = mapped.TryFind date
                    match found with
                    | Some c -> (date,c)
                    | None -> (date,count)
                )
                |> Charts.convertNameCountsToChart screener.name Charts.smallChart
                |> div [_class "block"] 
            )

        let stocks = Storage.getStocksBySector sectorName

        let stockTable =
            stocks
            |> List.map (fun stock ->
                tr [] [
                    td [] [
                        stock.ticker |> FinvizScraper.Core.StockTicker.value |> Views.generateTickerLink
                    ]
                    td [] [str stock.company]
                    td [] [str stock.sector]
                    td [] [str stock.industry]
                ]
            )
            |> fullWidthTable 

        let view = 
            div [_class "content"] [
                h1 [] [
                    str sectorName
                ]
            ]::charts @ [stockTable]
            
        
        view |> mainLayout $"Sector Dashboard for {sectorName}" 