﻿using System.Linq;
using Riganti.Utils.Testing.Selenium.Core.Checkers;
using Riganti.Utils.Testing.Selenium.Core.Exceptions;

namespace Riganti.Utils.Testing.Selenium.Core.Api
{
    public class AnyOperationRunner<T> : OperationRunnerBase<T>
    {
        private readonly T[] wrappers;

        internal AnyOperationRunner(T[] wrappers)
        {
            this.wrappers = wrappers;
        }

        public override void Evaluate<TException>(ICheck<T> check)
        {
            var checkResults = wrappers.Select(check.Validate).ToArray();
            var checkResult = CreateCheckResult(checkResults);
            EvaluateResult<TException>(checkResult);
        }

        private static CheckResult CreateCheckResult(CheckResult[] checkResults)
        {
            var isSucceeded = checkResults.Any(result => result.IsSucceeded);
            if (isSucceeded)
            {
                return CheckResult.Succeeded;
            }
            return new CheckResult($"The check doesn't match on any element. See {nameof(CheckResult.InnerResults)} for more details:", checkResults);
        }
    }
}