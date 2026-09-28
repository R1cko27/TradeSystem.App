// ViewModels/Abstractions/IDocumentOutputService.cs

using TradeSystem.Application.Dto;

// Файл содержит интерфейс IDocumentOutputService,
// который определяет контракт вывода сформированного документа на печать или в файл в приложении торговой системы.

namespace TradeSystem.App.ViewModels.Abstractions;

/// <summary>
/// Доставка сформированного документа вовне: на принтер или в файл.
/// Обе операции требуют UI-взаимодействия (выбор принтера / пути), поэтому
/// контракт живёт в Presentation-абстракциях, а реализация — в Views.
/// ViewModel не знает про PrintDialog/SaveFileDialog/System.IO.
/// </summary>
public interface IDocumentOutputService
{
    /// <summary>Печатает документ через системный диалог. true — отправлено, false — отменено/недоступно.</summary>
    bool Print(ReportDocument document); // Напечатать документ через системный диалог

    /// <summary>
    /// Сохраняет готовый контент в файл через диалог «Сохранение как».
    /// Контент формирует вызывающая сторона (ViewModel), чтобы логика формата
    /// не утекала в Presentation. true — сохранено, false — отменено/ошибка.
    /// </summary>
    bool SaveDocument(string content, string suggestedFileName, string filter); // Сохранить документ в файл через диалог
}