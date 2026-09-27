// Views/DialogService.cs

using System.Windows;
using TradeSystem.App.ViewModels.Abstractions;

// Файл содержит класс DialogService,
// который реализует отображение пользовательских диалогов средствами WPF в приложении торговой системы.

namespace TradeSystem.App.Views;

public sealed class DialogService : IDialogService
{
    public void ShowInfo(string message, string title = "Информация") => // Показать информационное сообщение
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);

    public void ShowWarning(string message, string title = "Внимание") => // Показать предупреждение
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);

    public void ShowError(string message, string title = "Ошибка") => // Показать сообщение об ошибке
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);

    public bool Confirm(string message, string title = "Подтверждение") => // Запросить подтверждение у пользователя
        MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question)
        == MessageBoxResult.Yes;
}