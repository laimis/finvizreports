namespace FinvizScraper.Web.Handlers

module Dashboard =

    open Giraffe.ViewEngine
    open FinvizScraper.Storage.Reports

    type DashboardViewModel =
        {
            screener:ScreenerResultReport;
            sectors:list<(string * int)>;
            industries:list<(string * int)>;
            countries:list<(string * int)>;
        }

    let private generateBreakdownParts screener = 
        
        let sectorsTable = screener.sectors |> Shared.toNameCountTable "Sectors"
        let industriesTable = screener.industries |> Shared.toNameCountTable "Industries"
        let countriesTable = screener.countries |> Shared.toNameCountTable "Countries"

        let screenerDate = screener.screener.date.ToString("yyyy-MM-dd")
        
        div [_class "content"] [
            h2 [] [ str screener.screener.name ]
            h5 [] [
                div [ _class "buttons"] [
                    a [ _class "button is-primary"
                        _href $"/screeners/{screener.screener.screenerid}/results/{screenerDate}"] [
                        str (screener.screener.count.ToString() + " results")
                    ]
                    a [ _class "button is-primary"
                        _href $"/screeners/{screener.screener.screenerid}"] [
                        str "Screener Details"
                    ]
                ]
            ]
            div [_class "columns"] [
                div [_class "column"] [sectorsTable]
                div [_class "column"] [industriesTable]
                div [_class "column"] [countriesTable]
            ]
        ]

    let private view (screeners:list<DashboardViewModel>) =
        let screenerRows =
            screeners
            |> List.map generateBreakdownParts

        let nodes = [
            h1 [_class "title"] [ str "Dashboard" ]
        ]

        List.append nodes screenerRows

    let handler()  = 
        
        // get screeners, render them in HTML
        let screenerResults = getLatestScreeners()

        let screenerResultWithBreakdowns =
            screenerResults
            |> List.map (fun x -> 
                let sectorBreakdown = topSectors x.screenerid x.date
                let industryBreakdown = topIndustries x.screenerid x.date
                let countryBreakdown = topCountries x.screenerid x.date
                {
                    screener=x;
                    sectors=sectorBreakdown;
                    industries=industryBreakdown;
                    countries=countryBreakdown
                }
            )

        view screenerResultWithBreakdowns
        |> Shared.mainLayout "Dashboard"