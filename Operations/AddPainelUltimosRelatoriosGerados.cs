using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkChanges.Operations;

internal sealed class AddPainelUltimosRelatoriosGerados() : IBulkChangesOperation
{
    public async Task<MemoryStream> DoChangesAsync(MemoryStream originalContents, Encoding encoding)
    {
        try
        {
            originalContents.Position = 0;
            int initialCapacity = (int)originalContents.Length;

            using var browsingContext = BrowsingContext.New(Configuration.Default);

            var parser = browsingContext.GetService<IHtmlParser>()!;

            using var document = await browsingContext.OpenAsync((req) => req.Content(stream: originalContents, shouldDispose: true));

            // Seleciona as divs relevantes
            var content = document.QuerySelector<IHtmlDivElement>("div.content")!;                               // Conteúdo principal da página
            var panelRow = content.QuerySelector<IHtmlDivElement>("div.row")!;                                   // Row que contém os panels
            var panelParametrosCol = panelRow.QuerySelector<IHtmlDivElement>("div.col-md-12")!;                  // Column que contém o panel de Parâmetros
            var panelParametros = panelParametrosCol.QuerySelector<IHtmlDivElement>("div.panel.panel-default")!; // Panel de Parâmetros
            var formParametros = panelParametros.QuerySelector<IHtmlFormElement>("div.panel-body > form")!;      // Form Parâmetros

            // Seta a largura do panel de Parâmetros para 1/3 da largura disponível do parent e atribui um id
            panelParametrosCol.ClassName = "col-md-4";
            panelParametros.Id = "panelParametros";

            // Remove a largura fixa nos campos dos Parâmetros
            var isFixedWidthElement = (IElement element) => element.GetAttribute("style")?.Contains("width") == true;
            panelParametros.QuerySelectorAll("*")
                           .Where(isFixedWidthElement)
                           .ToList()
                           .ForEach((element) => element.SetAttribute(name: "style", value: null));

            // Corrige a largura dos campos do form de Parâmetros caso necessário
            formParametros.QuerySelectorAll("div.col-md-8")
                          .ToList()
                          .ForEach((element) => { element.ClassList.Remove("col-md-8"); element.ClassList.Add("col-md-12"); });
            formParametros.QuerySelectorAll("div.col-md-4")
                          .ToList()
                          .ForEach((element) => { element.ClassList.Remove("col-md-4"); element.ClassList.Add("col-md-6"); });

            // Remove label-sm
            formParametros.QuerySelectorAll("div.panel-body > form label.label-sm")
                          .ToList()
                          .ForEach((element) => element.ClassList.Remove("label-sm"));

            // Adiciona header no panel de Parâmetros
            var panelParametrosHeading = (IHtmlDivElement)parser.ParseFragment(
                source:
                    """
                    <div class="panel-heading" style="font-size: 1.6rem;">
                        <h3 class="panel-title">Parâmetros</h3>
                    </div>
                    """,
                contextElement: document.Body!
            ).Single();
            panelParametros.FirstChild!.InsertBefore(panelParametrosHeading);

            // Adiciona o panel Últimos Gerados
            var panelUltimosGeradosCol = (IHtmlDivElement)parser.ParseFragment(
                source:
                    """
                    <div class="col-md-8">
                        <div class="panel panel-default" id="panelUltimosGerados">
                            <div class="panel-heading" style="font-size: 1.6rem;">
                                <h3 class="panel-title">Últimos Relatórios Gerados</h3>
                            </div>
                            <div class="panel-body" style="padding: 0 1px 1px 0;">
                                <iframe
                                    id="iframeUltimosGerados"
                                    src="relatorios-gerados-operador#<%= (int)this.Funcao() %>"
                                    style="height: 100%; width: 100%; border: none;"
                                ></iframe>
                            </div>
                        </div>
                    </div>
                    """,
                contextElement: document.Body!
            ).Single();
            panelRow.AppendChild(panelUltimosGeradosCol);

            return new MemoryStream(buffer: encoding.GetBytes(content.Prettify()));
        }
        catch
        {
            return new();
        }
    }
}
