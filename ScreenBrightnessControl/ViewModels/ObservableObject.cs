using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ScreenBrightnessControl.ViewModels
{
    /// <summary>
    /// Minimal <see cref="INotifyPropertyChanged"/> implementation shared by the view models.
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Assigns <paramref name="value"/> and raises a change notification when it differs.
        /// </summary>
        /// <returns><c>true</c> when the value actually changed.</returns>
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
