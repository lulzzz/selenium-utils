using Riganti.Selenium.Core.Abstractions;

namespace Riganti.Selenium.Validators.Checkers.ElementWrapperCheckers
{
    public class NotContainsTextValidator : IValidator<IElementWrapper>
    {
        public CheckResult Validate(IElementWrapper wrapper)
        {
            string value = wrapper.GetText();
            if (string.IsNullOrWhiteSpace(value))
            {
                return CheckResult.Succeeded;
            }
            else
            {
                return new CheckResult(
                    $"Element contain text '{value}'. Element should be empty.\r\n Element selector: {wrapper.Selector} \r\n");
            }
        }
    }
}