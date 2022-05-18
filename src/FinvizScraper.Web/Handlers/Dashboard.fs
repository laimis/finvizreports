namespace FinvizScraper.Web.Handlers

module Dashboard =

    open Giraffe.ViewEngine
    open FinvizScraper.Storage.Reports
    open FinvizScraper.Web.Shared
    open FinvizScraper.Core

    let private generateScreenerResultSection (screener:ScreenerResultReport) = 
        
        let screenerDate = screener.date.ToString("yyyy-MM-dd")
        
        div [_class "content"] [
            h2 [] [
                
                Views.generateHrefWithAttr
                    $"{screener.count}"
                    (Links.screenerResultsLink (screener.screenerid) screenerDate)
                    (_class "button is-primary mr-2 width50")

                str screener.name
            ]
        ]

    let generateHeaderRow =
        let titleDiv = div [ _class "column" ] [
            h1 [_class "title"] [ str "Dashboard" ]
        ]

        let searchDiv = div [ _class "column is-three-quarters" ] [
            div [ _class "columns"] [
                div [ _class "column" ] [
                    form [
                        _action "/stocks/search"
                        _method "GET"
                    ] [
                        input [
                            _class "input"
                            _type "text"
                            _placeholder "Search for stock"
                            _name "ticker"
                        ]
                    ]
                ]
                div [ _class "column" ] [
                    Views.generateHrefWithAttr
                        "Screener Trends"
                        Links.screenerTrends
                        (_class "button is-primary is-pulled-right mx-1")

                    Views.generateHrefWithAttr
                        "Industry Trends"
                        Links.industryTrends
                        (_class "button is-primary is-pulled-right mx-1")
                ]
            ]
        ]

        [
            div [ _class "columns mb-5" ] [
                titleDiv
                searchDiv
            ]
        ]

    let private generateJobStatusRow =
        div [ _class "columns" ] [
            div [ _class "column" ] [ 
                ScreenerJob |> Views.genericJobStatusGet |> str 
            ]
        ]

    
    let private filteredList func days screenerId =
        func screenerId days
        |> List.take 5

    let private generateIndustryTrendsRow days =

        let gainers = FinvizConfig.NewHighsScreener |> filteredList getTopIndustriesForScreener days
        let losers = FinvizConfig.NewLowsScreener |> filteredList getTopIndustriesForScreener days

        [
            div [_class "columns"] [
                div [ _class "column" ] [
                    Views.toNameCountTableWithLinks "Industries Trending Up" Links.industryLink gainers
                ]
                div [ _class "column" ] [
                    Views.toNameCountTableWithLinks "Industries Trending Down" Links.industryLink losers
                ]
            ]
        ]

    let private generateSectorTrendsRow days =

        let gainers = FinvizConfig.NewHighsScreener |> filteredList getTopSectorsForScreener days
        let losers = FinvizConfig.NewLowsScreener |> filteredList getTopSectorsForScreener days

        [
            div [_class "columns"] [
                div [ _class "column" ] [
                    Views.toNameCountTableWithLinks "Sectors Trending Up" Links.sectorLink gainers
                ]
                div [ _class "column" ] [
                    Views.toNameCountTableWithLinks "Sectors Trending Down" Links.sectorLink losers
                ]
            ]
        ]

    let private createView (screeners:list<ScreenerResultReport>) =
        
        let headerRow = generateHeaderRow

        let screenerRows = screeners |> List.map generateScreenerResultSection

        let industryTrendRows = generateIndustryTrendsRow FinvizConfig.industryTrendDayRange
        let sectorTrendRows = generateSectorTrendsRow FinvizConfig.sectorTrendDayRange

        let jobStatusRow = generateJobStatusRow

        headerRow @ screenerRows @ industryTrendRows @ sectorTrendRows @ [ jobStatusRow ]

    let handler()  = 
        
        // get screeners, render them in HTML
        getLatestScreeners()
        |> createView
        |> Views.mainLayout "Dashboard"