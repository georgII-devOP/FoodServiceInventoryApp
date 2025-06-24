using Xunit;
using FoodServiceInventoryApp.Converters;
using System.Windows;
using System;
using System.Globalization;

namespace FoodServiceInventoryApp.Tests
{
    public class InverseBooleanToVisibilityConverterTests
    {
        private readonly InverseBooleanToVisibilityConverter _converter;

        public InverseBooleanToVisibilityConverterTests()
        {
            _converter = new InverseBooleanToVisibilityConverter();
        }


        [Fact]
        public void Convert_True_ReturnsCollapsed()
        {
            var result = _converter.Convert(true, typeof(Visibility), null, CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void Convert_False_ReturnsVisible()
        {
            var result = _converter.Convert(false, typeof(Visibility), null, CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Visible, result);
        }

        [Fact]
        public void Convert_NonBooleanValue_ReturnsCollapsed()
        {
            var result = _converter.Convert("some string", typeof(Visibility), null, CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void Convert_Null_ReturnsCollapsed()
        {
            var result = _converter.Convert(null, typeof(Visibility), null, CultureInfo.InvariantCulture);

            Assert.Equal(Visibility.Collapsed, result);
        }

        [Fact]
        public void ConvertBack_Collapsed_ReturnsTrue()
        {
            var result = _converter.ConvertBack(Visibility.Collapsed, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.Equal(true, result);
        }

        [Fact]
        public void ConvertBack_Visible_ReturnsFalse()
        {
            var result = _converter.ConvertBack(Visibility.Visible, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.Equal(false, result);
        }

        [Fact]
        public void ConvertBack_Hidden_ReturnsFalse()
        {
            var result = _converter.ConvertBack(Visibility.Hidden, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.Equal(false, result);
        }

        [Fact]
        public void ConvertBack_NonVisibilityValue_ReturnsFalse()
        {
            var result = _converter.ConvertBack("some string", typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.Equal(false, result);
        }

        [Fact]
        public void ConvertBack_Null_ReturnsFalse()
        {
            var result = _converter.ConvertBack(null, typeof(bool), null, CultureInfo.InvariantCulture);

            Assert.Equal(false, result);
        }
    }
}