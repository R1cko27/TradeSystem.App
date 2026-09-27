// ViewModels/Abstractions/IActivatable.cs

using System.Threading.Tasks;

// Файл содержит интерфейс IActivatable,
// который определяет контракт перезагрузки данных ViewModel при её активации в приложении торговой системы.

namespace TradeSystem.App.ViewModels.Abstractions;

/// <summary>
/// Интерфейс для ViewModel, которым нужно перезагружать данные при активации.
/// </summary>
public interface IActivatable
{
    Task OnActivatedAsync(); // Перезагрузить данные при активации ViewModel
}