namespace FinvizScraper.Web.Handlers

module ScreenerResults =

    open Giraffe.ViewEngine
    open FinvizScraper.Storage.Reports
    open FinvizScraper.Web.Shared
    open FinvizScraper.Web.Shared.Views

    let screenerResultToTr (result:ScreenerResultReportItem) =
        
        let toTd input =
            td [] [ str input ]

        let toTdWithNode node =
            td [] [ node ]

        let rowAttributes = [_height "50px"]

        tr rowAttributes [
            toTdWithNode (generateTickerLink result.ticker)
            toTd result.name
            result.sector |> Links.sectorLink |> generateHref result.sector |> toTdWithNode
            result.industry |> Links.industryLink |> generateHref result.industry |> toTdWithNode
            result.country |> Links.countryLink |> generateHref result.country |> toTdWithNode
            result.marketCap |> marketCapFormatted |> toTd
            result.price |> dollarFormatted |> toTd
            result.change |> percentFormatted |> toTd
            result.volume |> volumeFormatted |> toTd
            result.ticker |> Links.tradingViewLink |> generateHref "link" |> toTdWithNode
        ]

    let toBreakdownTable breakdownTitle linkFunction (breakdown:seq<string * list<ScreenerResultReportItem>>) =
        // row with headers for each column and another row with length
        let headerRow = tr [] [
            th [] [str breakdownTitle]
            th [] [str ""]
        ] 

        let valueRows =
            breakdown
            |> Seq.map (fun (name, list) ->
                tr [] [
                    td [] [
                        a [_href (linkFunction name)] [str name]
                    ]
                    td [] [str (list.Length.ToString())]
                ]
            )
            |> Seq.toList
        
        headerRow::valueRows |> Views.fullWidthTable
        
    let calculateBreakdowns screenerResults =
        
        let convertToBreakdown (name, linkFunction, groupByProperty) =
            (name, linkFunction, (screenerResults |> List.groupBy groupByProperty |> List.sortByDescending (fun (_, list) -> list.Length)))

        let breakdowns = [
            ("Sectors", Links.sectorLink, fun a -> a.sector);
            ("Industries", Links.industryLink, fun a -> a.industry);
            ("Countries", Links.countryLink, fun a -> a.country);
        ]
        
        breakdowns |> List.map convertToBreakdown

    let headerRow =
        let headers = [ 
            "Ticker"
            "Company"
            "Sector"
            "Industry"
            "Country" 
            "Market Cap"
            "Price"
            "Change"
            "Volume"
            "Link"
        ]

        let toHeader title =
            th [] [str title]

        let headerCells = headers |> List.map toHeader

        tr [] headerCells

    let private view (screener:FinvizScraper.Core.Screener) (results:list<ScreenerResultReportItem>) =

        let screenerTable =
            results
            |> List.map screenerResultToTr
            |> List.append [headerRow]
            |> fullWidthTable

        let breakdowns = calculateBreakdowns results
                
        let breakdownDivs = 
            breakdowns
            |> List.map (
                fun (breakdownTitle,linkFunction,breakdownList) ->
                    toBreakdownTable breakdownTitle linkFunction breakdownList
                )
            |> List.map (
                fun breakdownTable ->
                    div [_class "column"] [breakdownTable]
                )

        [
            div [_class "content"] [
                h1 [] [
                    str ("Screener: " + screener.name)
                ]
                div [_class "block"] [
                    str ("Total Results: " + (string results.Length))
                ]
                
                div [_class "block"] [
                    a [
                        _class "button is-primary" 
                        _href screener.url
                        _target "_blank"
                    ] [
                        str "View on Finviz"
                    ]
                ]
            ]
            div [_class "columns"] breakdownDivs
            div [_class "block"] [screenerTable]
        ]

    let handler ((id:int),(date:string))  = 
        
        // get screeners, render them in HTML
        let byIdOption = FinvizScraper.Storage.Storage.getScreenerById id
        match byIdOption with
        | Some screener -> 
            let screenerResults = getScreenerResults screener.id date
            let view      = view screener screenerResults
            view |> Views.mainLayout $"Screener: {screener.name}"
        | None ->
            Views.notFound "Screener not found"