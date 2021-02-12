using System;

namespace FoxTunes
{
    public static class FormatterFactory
    {
        public static FormatterBase Create(string format)
        {
            if (TimeSpanFormatter.CanFormat(format))
            {
                return new TimeSpanFormatter(format);
            }
            return new StringFormatter(format);
        }

        public abstract class FormatterBase
        {
            protected FormatterBase(string format)
            {
                this.Format = format;
            }

            public string Format { get; private set; }

            public abstract object GetValue(object value);
        }

        public class StringFormatter : FormatterBase
        {
            public StringFormatter(string format) : base(format)
            {

            }

            public override object GetValue(object value)
            {
                if (string.IsNullOrEmpty(this.Format))
                {
                    return Convert.ToString(value);
                }
                return string.Format(this.Format, value);
            }
        }

        public class TimeSpanFormatter : FormatterBase
        {
            public TimeSpanFormatter(string format) : base(format)
            {

            }

            public override object GetValue(object value)
            {
                var ms = default(int);
                if (!int.TryParse(Convert.ToString(value), out ms))
                {
                    return value;
                }

                var s = Convert.ToInt32((ms / 1000) % 60);
                var m = Convert.ToInt32((ms / (1000 * 60)) % 60);
                var h = Convert.ToInt32((ms / (1000 * 60 * 60)) % 24);

                if (h > 0)
                {
                    return string.Format("{0:00}:{1:00}:{2:00}", h, m, s);
                }

                return string.Format("{0:00}:{1:00}", m, s);
            }

            public static bool CanFormat(string format)
            {
                return string.Equals(format, CommonFormats.TimeSpan, StringComparison.OrdinalIgnoreCase);
            }
        }
    }
}
