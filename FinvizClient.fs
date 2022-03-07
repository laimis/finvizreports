namespace FinvizScraper

module FinvizClient =

    let fetchScreenerHtml (url:string) =
        let web = HtmlAgilityPack.HtmlWeb()
        web.CachePath <- "cache"
        web.UsingCache <- true

        web.Load(url)


    let parseScreenerHtml (doc:HtmlAgilityPack.HtmlDocument) =

        let nodes = doc.DocumentNode.SelectNodes("//table[@id='screener-views-table']/tr")

        // 3rd node has the tickers

        let skipAndTake skip take seq =
            seq 
            |> Seq.skip skip
            |> Seq.take take

        let extractValueFromScreenerCell (node:HtmlAgilityPack.HtmlNode) =
            let value = node.ChildNodes[0].InnerText
            value

        let processScreenerRow (node:HtmlAgilityPack.HtmlNode) : ScreenerResult =
            
            let tickerNode = extractValueFromScreenerCell node.ChildNodes[2]
            let companyNode = extractValueFromScreenerCell node.ChildNodes[3]
            let sectorNode = extractValueFromScreenerCell node.ChildNodes[4]
            let industryNode = extractValueFromScreenerCell node.ChildNodes[5]
            let countryNode = extractValueFromScreenerCell node.ChildNodes[6]
            let marketCapNode = extractValueFromScreenerCell node.ChildNodes[7]
            let priceNode = extractValueFromScreenerCell node.ChildNodes[9]
            let changeNode = extractValueFromScreenerCell node.ChildNodes[10]
            let volumeNode = extractValueFromScreenerCell node.ChildNodes[11]
            {
                ticker=tickerNode;
                company=companyNode;
                sector=sectorNode; 
                industry=industryNode;
                country=countryNode; 
                marketCap=marketCapNode;
                price=priceNode;
                change=changeNode;
                volume=volumeNode
            }

        nodes 
            |> skipAndTake 3 1                      // skip three tr nodes, take one that has the tickers table
            |> Seq.collect (fun n -> n.ChildNodes)  // this should contain #text, <td>, #text
            |> skipAndTake 1 1                      // skip one #text, take one that has td
            |> Seq.collect (fun n -> n.ChildNodes)  // this should contain #text <table> #text
            |> skipAndTake 1 1                      // skip one #text, take table
            |> Seq.collect (fun n -> n.ChildNodes)  // this should be all tr nodes
            |> Seq.skip 2                           // skip what looks like text element and a header row
            |> Seq.map processScreenerRow