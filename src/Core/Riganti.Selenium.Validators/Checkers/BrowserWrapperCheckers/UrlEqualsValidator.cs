using System;
using Riganti.Selenium.Core.Abstractions;

namespace Riganti.Selenium.Validators.Checkers.BrowserWrapperCheckers
{
    public class UrlEqualsValidator : IValidator<IBrowserWrapper>
    {
        private readonly string url;

        public UrlEqualsValidator(string url)
        {
            this.url = url;
        }

        public CheckResult Validate(IBrowserWrapper wrapper)
        {
            var uri1 = new Uri(wrapper.CurrentUrl, UriKind.Absolute);
            var uri2 = new Uri(url, UriKind.RelativeOrAbsolute);

            var isSucceeded = uri1 == uri2;

            return isSucceeded ? CheckResult.Succeeded : new CheckResult($"Current url is not expected. Current url: '{wrapper.CurrentUrl}', Expected url: '{url}'.");
        }
    }
}