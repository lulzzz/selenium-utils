﻿using System;
using Riganti.Utils.Testing.Selenium.Core;
using Riganti.Utils.Testing.Selenium.Core.Abstractions;
using Riganti.Utils.Testing.Selenium.LambdaApi;
using Xunit.Abstractions;

namespace Riganti.Utils.Testing.Selenium.Core.Samples.LambdaApi.Tests
{
    public abstract class AppSeleniumTest : SeleniumTest 
    {
        public void RunInAllBrowsers()
        {
        }
        
        protected AppSeleniumTest(ITestOutputHelper output) : base(output) {}
    }
}