using HtmlAgilityPack;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;


List<CardRecord> cartas = new List<CardRecord> ();
var htmlListCard = new HtmlWeb().Load("https://sw-unlimited-db.com/cards/");
var listLinks = htmlListCard.DocumentNode.SelectNodes("/html/body/div[1]/div[2]/div/div/div[1]/div[1]/div[@class='relative']/a[1]");

Parallel.ForEach(listLinks, link =>
{
    string? linkPagina = link.GetAttributes().ToList().FirstOrDefault()?.Value;
    var htmlCard = new HtmlWeb().Load($"https://sw-unlimited-db.com/cards/{linkPagina}");
    var tituloPagina = htmlCard.DocumentNode.SelectSingleNode("//h1");
    if(tituloPagina.InnerText != "Page Not Found")
    {
        short custo = 0;
        string descricao = "Sem descrição";
        var descricaoCarta = htmlCard.DocumentNode.SelectNodes("//html/body/div[1]/div/div/div[2]/div[1]/div/div[preceding::*[name()='hr'] and following::*[name()='hr']]");
        var custoCarta = htmlCard.DocumentNode.SelectSingleNode("//*[text()='Cost:']")?.NextSibling?.InnerText.Trim();

        if (descricaoCarta != null) 
        {
            descricao = "";
            descricaoCarta.ToList().ForEach(x =>
            {
                descricao = Regex.Replace(descricao + x.InnerText.Replace("\n", " ").Replace("\r", "").Replace("\r\n", ""), @"\s+", " ").Trim();
            });

        }

        cartas.Add(new CardRecord(
            HttpUtility.HtmlDecode(tituloPagina.InnerText), 
            HttpUtility.HtmlDecode(descricao),
            short.TryParse(custoCarta, out custo) ? custo : null )
            );
    }
});

StringBuilder arquivo = new StringBuilder();
arquivo.AppendLine("Nome;Descrição,Custo");
cartas.ForEach(carta => arquivo.AppendLine($"{carta.nome};{carta.descricao};{carta.custo}"));
File.WriteAllText("cartas.xls", contents: arquivo.ToString(), UTF8Encoding.UTF8);
Console.WriteLine("Arquivo salvo. Pode ser encontrado na pasta bin do projeto");
