using System;
using Riganti.Utils.Testing.Selenium.Core.Abstractions;

namespace Riganti.Utils.Testing.Selenium.Validators.Checkers.BrowserWrapperCheckers
{
    public class HyperLinkEqualsValidator : ICheck<IBrowserWrapper>
    {
        private readonly string selector;
        private readonly string url;
        private readonly UrlKind kind;
        private readonly UriComponents[] components;

        public HyperLinkEqualsValidator(string selector, string url, UrlKind kind, params UriComponents[] components)
        {
            this.selector = selector;
            this.url = url;
            this.kind = kind;
            this.components = components;
        }

        public CheckResult Validate(IBrowserWrapper wrapper)
        {
            wrapper.ForEach(selector, element =>
            {
                throw new NotImplementedException();
                //AssertUI.HyperLinkEqualsValidator(element, url, kind, components);
            });
            return CheckResult.Succeeded;
        }
    }
}