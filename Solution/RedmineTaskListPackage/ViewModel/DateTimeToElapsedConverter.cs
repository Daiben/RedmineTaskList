using System;
using System.Globalization;
using System.Windows.Data;

namespace RedmineTaskListPackage.ViewModel
{
    public class DateTimeToElapsedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var dateTime = (DateTime)value;
            var timeSpan = DateTime.Now - dateTime;

            var years = (int)Math.Floor((double)timeSpan.Days / 365);
            var months = (int)Math.Floor((double)timeSpan.Days / 30);

            if (years == 1)
                return "1 year ago";

            if (years > 1)
                return String.Concat(years, " years ago");

            if (months == 1)
                return "1 month ago";

            if (months > 1)
                return String.Concat(months, " months ago");

            if (timeSpan.Days == 1)
                return "1 day ago";
                
            if (timeSpan.Days > 1)
                return String.Concat(timeSpan.Days, " days ago");

            if (timeSpan.Hours == 1)
                return "1 hour ago";
                
            if (timeSpan.Hours > 1)
                return String.Concat(timeSpan.Hours, " hours ago");

            if (timeSpan.Minutes == 1)
                return "1 minute ago";

            if (timeSpan.Minutes > 1)
                return String.Concat(timeSpan.Minutes, " minutes ago");

            if (timeSpan.Seconds == 1)
                return "1 second ago";

            if (timeSpan.Seconds > 1)
                return String.Concat(timeSpan.Seconds, " seconds ago");

            return "just now";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}