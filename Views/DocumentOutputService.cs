// Views/DocumentOutputService.cs

using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Win32;
using TradeSystem.App.ViewModels.Abstractions;
using TradeSystem.Application.Dto;

// Файл содержит класс DocumentOutputService,
// который реализует печать и сохранение отчётов средствами WPF в приложении торговой системы.

namespace TradeSystem.App.Views;

public sealed class DocumentOutputService : IDocumentOutputService
{
    public bool Print(ReportDocument document) // Напечатать документ через системный диалог
    {
        ArgumentNullException.ThrowIfNull(document);

        try
        {
            var dialog = new PrintDialog();
            if (dialog.ShowDialog() != true)
                return false;

            var flow = BuildFlowDocument(document);
            flow.PageWidth = dialog.PrintableAreaWidth;
            flow.PageHeight = dialog.PrintableAreaHeight;

            var paginator = ((IDocumentPaginatorSource)flow).DocumentPaginator;
            dialog.PrintDocument(paginator, document.Title);
            return true;
        }
        catch
        {
            // Нет принтера / ошибка драйвера / отмена на уровне ОС.
            // ViewModel по false покажет понятное сообщение пользователю.
            return false;
        }
    }

    public bool SaveDocument(string content, string suggestedFileName, string filter) // Сохранить документ в файл через диалог
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                FileName = suggestedFileName,
                Filter = filter,
                AddExtension = true,
                DefaultExt = Path.GetExtension(suggestedFileName)
            };

            if (dialog.ShowDialog() != true)
                return false;

            // UTF-8 с BOM, чтобы Excel/Блокнот корректно открывали кириллицу.
            var utf8Bom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
            File.WriteAllText(dialog.FileName, content, utf8Bom);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static FlowDocument BuildFlowDocument(ReportDocument document) // Построить FlowDocument для печати
    {
        var flow = new FlowDocument
        {
            FontFamily = new System.Windows.Media.FontFamily("Segoe UI"),
            FontSize = 12,
            PagePadding = new Thickness(40)
        };

        flow.Blocks.Add(new Paragraph(new Run(document.Title))
        {
            FontSize = 16,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 8)
        });

        // Моноширинный шрифт сохраняет выравнивание колонок из ToPlainText().
        var body = new Paragraph
        {
            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
            FontSize = 11
        };

        var lines = document.ToPlainText().Replace("\r\n", "\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            if (i > 0) body.Inlines.Add(new LineBreak());
            body.Inlines.Add(new Run(lines[i]));
        }

        flow.Blocks.Add(body);
        return flow;
    }
}