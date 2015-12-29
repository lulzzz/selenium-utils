﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenQA.Selenium;
using Riganti.Utils.Testing.SeleniumCore.Exceptions;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Riganti.Utils.Testing.SeleniumCore
{
    public class SeleniumTestBase : ITestBase
    {
        public static readonly FastModeWebDriverFactoryRegistry FastModeFactoryRegistry;
        static SeleniumTestBase()
        {
            FastModeFactoryRegistry = new FastModeWebDriverFactoryRegistry();
            Loggers = new List<ILogger>();
            if (SeleniumTestsConfiguration.StandardOutputLogger) Loggers.Add(new StandardOutputLogger());
            if (SeleniumTestsConfiguration.TeamcityLogger) Loggers.Add(new TeamcityLogger());
            if (SeleniumTestsConfiguration.DebuggerLogger) Loggers.Add(new DebuggerLogger());
            if (SeleniumTestsConfiguration.DebugLogger) Loggers.Add(new DebugLogger());

            //default logger
            if (Loggers.Count == 0 && !SeleniumTestsConfiguration.DebugLoggerContainedKey)
            {
                Loggers.Add(new DebugLogger());
            }
        }


        public static List<ILogger> Loggers { get; protected set; }
        public TestContext TestContext { get; set; }
        private WebDriverFactoryRegistry factory;
        private string screenshotsFolderPath;
        private string CurrentSubSection { get; set; }
        private Type ExpectedExceptionType { get; set; }
        protected IWebDriver LatestLiveWebDriver { get; set; }

        public string ScreenshotsFolderPath
        {
            get
            {
                if (string.IsNullOrWhiteSpace(screenshotsFolderPath))
                {
                    screenshotsFolderPath = TestContext.TestDeploymentDir;
                }
                return screenshotsFolderPath;
            }
            set { screenshotsFolderPath = value; }
        }

        public SeleniumTestBase()
        {
            if (SeleniumTestsConfiguration.TestContextLogger)
            {
                var logger = Loggers.FirstOrDefault(s => s is TestContextLogger);
                Loggers.Remove(logger);
                Loggers.Add(new TestContextLogger(this));
            }
        }

        public WebDriverFactoryRegistry FactoryRegistry => factory ?? (factory = new WebDriverFactoryRegistry());

        protected virtual List<IWebDriverFactory> BrowserFactories => SeleniumTestsConfiguration.FastMode ? FastModeFactoryRegistry.BrowserFactories : FactoryRegistry.BrowserFactories;
        private BrowserWrapper wrapper;

        /// <summary>
        /// Runs the specified action in all configured browsers.
        /// </summary>
        protected virtual void RunInAllBrowsers(Action<BrowserWrapper> action)
        {
            CheckAvailableWebDriverFactories();
            foreach (var browserFactory in BrowserFactories)
            {
                string browserName;
                Exception exception;

                if (!SeleniumTestsConfiguration.DeveloperMode)
                {
                    TryExecuteTest(action, browserFactory, out browserName, out exception);
                }
                else
                {
                    //developer mode - it throws exception directly without catch statement
                    ExecuteTest(action, browserFactory, out browserName, out exception);
                }
                if (exception != null)
                {
                    if (CurrentSubSection == null)
                        throw new SeleniumTestFailedException(exception, browserName, ScreenshotsFolderPath);
                    throw new SeleniumTestFailedException(exception, browserName, ScreenshotsFolderPath, CurrentSubSection);
                }
            }
        }

        private void ExecuteTest(Action<BrowserWrapper> action, IWebDriverFactory browserFactory, out string browserName, out Exception exception)
        {
            try
            {
                exception = null;
                var browser = LatestLiveWebDriver = browserFactory.CreateNewInstance();
                wrapper = new BrowserWrapper(browser, this);
                browserName = browser.GetType().Name;
                action(wrapper);
            }
            finally
            {
                wrapper.Dispose();
            }
        }

        private void TryExecuteTest(Action<BrowserWrapper> action, IWebDriverFactory browserFactory, out string browserName, out Exception exception)
        {
            var attemptNumber = 0;
            do
            {
                attemptNumber++;
                exception = null;
                var browser = LatestLiveWebDriver = browserFactory.CreateNewInstance();
                if (attemptNumber > 1)
                {
                    (browserFactory as IFastModeFactory)?.Recreate();
                }

                wrapper = new BrowserWrapper(browser, this);
                browserName = browser.GetType().Name;

                WriteLine($"Testing browser '{browserName}' attempt no. {attemptNumber}");
                bool exceptionWasThrow = false;
                try
                {
                    BeforeSpecificBrowserTestStarts(browser);
                    action(wrapper);
                    AfterSpecificBrowserTestEnds(browser);

                }
                catch (Exception ex)
                {
                    exceptionWasThrow = true;
                    bool isExpected = false;
                    if (ExpectedExceptionType != null)
                    {
                        isExpected = ex.GetType() == ExpectedExceptionType || (AllowDerivedExceptionTypes && ExpectedExceptionType.IsInstanceOfType(ex));
                    }

                    if (!isExpected)
                    {
                        TakeScreenshot(attemptNumber, wrapper);
                        // fail the test
                        exception = ex;
                    }
                }
                finally
                {
                    if (browserFactory is IFastModeFactory)
                    {
                        ((IFastModeFactory)browserFactory).Clear();
                    }
                    else
                    {
                        wrapper.Dispose();
                    }
                }
                if (ExpectedExceptionType != null && !exceptionWasThrow)
                {
                    exception = new SeleniumTestFailedException("Test was supposted to fail and it did not.");
                }
            }
            while (exception != null && attemptNumber < SeleniumTestsConfiguration.TestAttemps);
        }

        private void CheckAvailableWebDriverFactories()
        {
            if (BrowserFactories.Count == 0)
            {
                throw new Exception("Factory doesn't contains drivers! Enable one driver at least to start UI Tests!");
            }
        }

        public virtual void RunTestSubSection(string subSectionName, Action<BrowserWrapper> action)
        {
            WriteLine($"Starts testing of section: {subSectionName}");
            CurrentSubSection = subSectionName;
            action(wrapper);
            CurrentSubSection = null;
            WriteLine($"Testing of section succesfully completed.");
        }

        protected virtual void TakeScreenshot(int attemptNumber, BrowserWrapper browserWrapper)
        {  // make screenshot
            try
            {
                LogCurrentlyPerformedAction("Taking screenshot");

                var filename = Path.Combine(ScreenshotsFolderPath, $"{TestContext.FullyQualifiedTestClassName}_{TestContext.TestName}" + attemptNumber + ".png");
                browserWrapper.TakeScreenshot(filename);
                TestContext.AddResultFile(filename);
            }
            catch
            {
                //ignore
            }
        }

        protected static void WriteLine(string message)
        {
            Loggers.ForEach(logger =>
            {
                logger.WriteLine(message);
            });
        }

        public static void Log(string message)
        {
            WriteLine(message);
        }

        public virtual void Log(Exception exception)
        {
            WriteLine(exception.ToString());
        }

        protected virtual void LogCurrentlyPerformedAction(string actionName)
        {
            WriteLine($"Currently performing: {actionName}");
        }

        protected void ExpectException(Type type, bool allowDerivedTypes = false)
        {
            AllowDerivedExceptionTypes = allowDerivedTypes;
            ExpectedExceptionType = type;
        }

        public bool AllowDerivedExceptionTypes { get; set; }


        public virtual void BeforeSpecificBrowserTestStarts(IWebDriver browser)
        {
        }
        public virtual void AfterSpecificBrowserTestEnds(IWebDriver browser)
        {
        }
    }
}