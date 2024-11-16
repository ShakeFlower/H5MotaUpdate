using System.Collections.ObjectModel;

using Brush = System.Windows.Media.Brush;
using Brushes = System.Windows.Media.Brushes;
public class ColoredString
{
    public string Text { get; set; }
    public Brush Color { get; set; }

    public ColoredString(string text, Brush color)
    {
        Text = text;
        Color = color;
    }
}

namespace H5MotaUpdate.ViewModels
{
    internal static class ErrorLogger
    {
        private static ObservableCollection<ColoredString> _errorMessages = new ObservableCollection<ColoredString>();

        public static ObservableCollection<ColoredString> ErrorMessages => _errorMessages;

        public static void LogError(string error, string color)
        {
            switch (color)
            {
                case "red":
                    ErrorMessages.Add(new ColoredString(error, Brushes.Red));
                    break;
                case "black":
                default:
                    ErrorMessages.Add(new ColoredString(error, Brushes.Black));
                    break;
            }

        }

        public static void LogError(string error)
        {
            ErrorMessages.Add(new ColoredString(error, Brushes.Black));
        }

        public static void Clear()
        {
            _errorMessages.Clear();
        }
    }
}
