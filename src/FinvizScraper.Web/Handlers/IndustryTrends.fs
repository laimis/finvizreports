namespace FinvizScraper.Web.Handlers

module IndustryTrends =
    open FinvizScraper.Web.Shared
    open Giraffe.ViewEngine.HtmlElements
    open FinvizScraper.Storage
    open FinvizScraper.Web.Shared.Views
    open Giraffe.ViewEngine

    let handler()  =
        let date = Storage.getIndustryUpdatesLatestDate() |> FinvizScraper.Core.FinvizConfig.formatRunDate

        let getIndustryUpdatesAndTurnToMap (days:int) =
            date 
            |> Storage.getIndustryUpdates days
            |> List.map (fun x -> (x.industry, x))
            |> Map.ofList
        
        let industryUpdates20 = getIndustryUpdatesAndTurnToMap 20
        let industryUpdates200 = getIndustryUpdatesAndTurnToMap 200

        let dataRows =
            industryUpdates200
            |> Map.toList
            |> List.sortByDescending (fun (key, update) -> update.percentAbove)
            |> List.map (fun (key, iu) ->

                let toSMACells (update:FinvizScraper.Core.IndustryUpdate) =
                    [
                        td [] [ update.above.ToString() |> str  ]
                        td [] [ update.total.ToString() |> str ]
                        td [] [ System.String.Format("{0:N2}%", update.percentAbove) |> str ]
                    ]

                let sma20Cells = toSMACells (industryUpdates20[key])
                let sma200Cells = toSMACells (iu)

                let image = img [ 
                    _src Links.finvizLogoLink
                    _width "50px"
                ]

                let commonCells = [
                    td [] [ iu.industry |> Links.industryFinvizLink |> generateHrefWithElement image]
                    td [] [ iu.industry |> Links.industryLink |> generateHref iu.industry]
                ]

                let cells = List.append (List.append commonCells sma20Cells) sma200Cells

                tr [] cells
            )

        let header = tr [] [
            th [] [ ]
            th [] [ str "Industry" ]
            th [ _colspan "3"] [ str "20" ]
            th [ _colspan "3"] [ str "200" ]
        ]

        let table = header::dataRows |> fullWidthTable

        let view = [
            div [_class "content"] [
                h1 [] [
                    str "Industry Trends"
                ]
            ]
            table
        ]
        
        view |> mainLayout $"Industry Trends" 