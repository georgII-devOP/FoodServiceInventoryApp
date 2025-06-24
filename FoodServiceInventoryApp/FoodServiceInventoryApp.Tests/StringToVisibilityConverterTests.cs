using Xunit;
using FoodServiceInventoryApp.Converters;
using System.Windows;
using System;
using System.Globalization;

namespace FoodServiceInventoryApp.Tests
{
    public class StringToVisibilityConverterTests
    {
        private readonly StringToVisibilityConverter _converter;

        public StringToVisibilityConverterTests()
        {
            _converter = new StringToVisibilityConverter();
        }


        [Fact]
        public void Convert_NonNullOrWhitespaceString_ReturnsVisible()
        {
            string value = "Hello World";

            var result = _converter.Convert(value, typeof(Visibility), null, CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Visible, result);
        }

        [Fact]
        public void Convert_EmptyString_ReturnsCollapsed()
        {
            string value = "";

            var result = _converter.Convert(value, typeof(Visibility), null, CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void Convert_NullString_ReturnsCollapsed()
        {
            string value = null;

            var result = _converter.Convert(value, typeof(Visibility), null, CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void Convert_WhitespaceString_ReturnsCollapsed()
        {
            string value = "   \t\n ";

            var result = _converter.Convert(value, typeof(Visibility), null, CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void Convert_NonStringObject_ReturnsVisible()
        {
            object value = 123;

            var result = _converter.Convert(value, typeof(Visibility), null, CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void ConvertBack_ThrowsNotImplementedException()
        {
            Assert.Throws<NotImplementedException>(() =>
            {
                _converter.ConvertBack(Visibility.Visible, typeof(string), null, CultureInfo.InvariantCulture);
            });
        }
    }
}