// ViewModels/Abstractions/IDialogService.cs

// Файл содержит интерфейс IDialogService,
// который определяет контракт отображения пользовательских диалогов в приложении торговой системы.

namespace TradeSystem.App.ViewModels.Abstractions;

/// <summary>
/// Абстракция пользовательских диалогов. ViewModel не знает про MessageBox/WPF —
/// только про этот контракт. Реализация живёт в Views (UI-адаптер).
/// </summary>
public interface IDialogService
{
    void ShowInfo(string message, string title = "Информация"); // Показать информационное сообщение
    void ShowWarning(string message, string title = "Внимание"); // Показать предупреждение
    void ShowError(string message, string title = "Ошибка"); // Показать сообщение об ошибке
    bool Confirm(string message, string title = "Подтверждение"); // Запросить подтверждение у пользователя
}