// ViewModels/ViewModelBase.cs

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using TradeSystem.App.ViewModels.Abstractions;
using TradeSystem.Application.Exceptions;

// Файл содержит абстрактный класс ViewModelBase,
// который представляет базовую ViewModel приложения торговой системы с флагом занятости и единой обработкой ошибок.

namespace TradeSystem.App.ViewModels;

/// <summary>
/// Общая база всех ViewModel: флаг занятости и единая обработка ошибок
/// бизнес-правил (BusinessException) и непредвиденных сбоев.
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    protected readonly IDialogService Dialog; // Сервис пользовательских диалогов

    [ObservableProperty]
    private bool _isBusy; // Признак выполнения длительной операции

    protected ViewModelBase(IDialogService dialog)
    {
        Dialog = dialog ?? throw new ArgumentNullException(nameof(dialog));
    }

    /// <summary>
    /// Выполняет асинхронное действие, скрывая шаблон try/catch/finally
    /// и переводя исключения в понятные пользователю сообщения.
    /// </summary>
    protected async Task RunAsync(Func<Task> action) // Выполнить асинхронное действие с обработкой ошибок
    {
        if (IsBusy) return; // защита от двойного нажатия

        try
        {
            IsBusy = true;
            await action();
        }
        catch (BusinessException ex)
        {
            Dialog.ShowError(ex.Message); // Ошибка бизнес-правила
        }
        catch (Exception ex)
        {
            Dialog.ShowError($"Непредвиденная ошибка: {ex.Message}"); // Непредвиденная ошибка
        }
        finally
        {
            IsBusy = false;
        }
    }
}